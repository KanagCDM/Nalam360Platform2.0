using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Data.UnitOfWork;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    Task<Result<int>> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task<Result> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task<Result> CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task<Result> RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether a transaction is currently active.
    /// </summary>
    bool HasActiveTransaction { get; }
}
