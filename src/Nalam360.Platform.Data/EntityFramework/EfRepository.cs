using Microsoft.EntityFrameworkCore;
using Nalam360.Platform.Core.Results;
using Nalam360.Platform.Data.Repositories;
using Nalam360.Platform.Data.Specifications;
using Nalam360.Platform.Domain.Primitives;
using System.Linq.Expressions;

namespace Nalam360.Platform.Data.EntityFramework;

/// <summary>
/// Entity Framework implementation of the repository pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public class EfRepository<TEntity, TId> : IRepository<TEntity, TId>, IReadRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected readonly DbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public EfRepository(DbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        return entity is not null
            ? Result<TEntity>.Success(entity)
            : Result<TEntity>.Failure(Error.NotFound(typeof(TEntity).Name, id));
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(
        ISpecification<TEntity>? specification = null,
        CancellationToken cancellationToken = default)
    {
        return specification is null
            ? await DbSet.CountAsync(cancellationToken)
            : await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<PagedResult<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TEntity>(items, pageNumber, pageSize, totalCount);
    }

    /// <inheritdoc />
    public virtual async Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return Result<TEntity>.Success(entity);
    }

    /// <inheritdoc />
    public virtual async Task<Result> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public virtual Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        return Task.FromResult(Result<TEntity>.Success(entity));
    }

    /// <inheritdoc />
    public virtual Task<Result> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.FromResult(Result.Success());
    }

    /// <inheritdoc />
    public virtual Task<Result> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbSet.RemoveRange(entities);
        return Task.FromResult(Result.Success());
    }

    /// <summary>
    /// Applies a specification to a queryable.
    /// </summary>
    protected virtual IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator.GetQuery(DbSet.AsQueryable(), specification);
    }
}

/// <summary>
/// Entity Framework repository with Guid identifier.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EfRepository<TEntity> : EfRepository<TEntity, Guid>, IRepository<TEntity>, IReadRepository<TEntity>
    where TEntity : Entity<Guid>
{
    public EfRepository(DbContext context) : base(context)
    {
    }
}
