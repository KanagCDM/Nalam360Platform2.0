using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nalam360.Platform.Core.Results;
using Nalam360.Platform.Data.UnitOfWork;

namespace Nalam360.Platform.Data.EntityFramework;

/// <summary>
/// Entity Framework implementation of Unit of Work pattern.
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public EfUnitOfWork(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public bool HasActiveTransaction => _currentTransaction is not null;

    /// <inheritdoc />
    public async Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.Success(rowsAffected);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            return Result<int>.Failure(Error.Conflict("A concurrency conflict occurred while saving changes."));
        }
        catch (DbUpdateException ex)
        {
            return Result<int>.Failure(Error.Internal($"A database error occurred: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(Error.Internal($"An unexpected error occurred: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            return Result.Failure(Error.Conflict("A transaction is already active."));
        }

        try
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Internal($"Failed to begin transaction: {ex.Message}"));
        }
    }

    /// <inheritdoc />
    public async Task<Result> CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return Result.Failure(Error.Conflict("No active transaction to commit."));
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync(cancellationToken);
            return Result.Failure(Error.Internal($"Failed to commit transaction: {ex.Message}"));
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task<Result> RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
        {
            return Result.Failure(Error.Conflict("No active transaction to rollback."));
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Internal($"Failed to rollback transaction: {ex.Message}"));
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DisposeTransactionAsync().GetAwaiter().GetResult();
        _context.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await _context.DisposeAsync();
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
