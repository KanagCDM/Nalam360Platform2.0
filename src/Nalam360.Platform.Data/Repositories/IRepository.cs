using Nalam360.Platform.Core.Results;
using Nalam360.Platform.Domain.Primitives;
using System.Linq.Expressions;

namespace Nalam360.Platform.Data.Repositories;

/// <summary>
/// Generic repository interface for data access operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<Result<TEntity>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the predicate.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching the predicate or null.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the predicate.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task<Result> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    Task<Result> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple entities.
    /// </summary>
    Task<Result> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}

/// <summary>
/// Non-generic repository interface.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : Entity<Guid>
{
}
