using Nalam360.Platform.Core.Results;
using Nalam360.Platform.Data.Specifications;
using Nalam360.Platform.Domain.Primitives;
using System.Linq.Expressions;

namespace Nalam360.Platform.Data.Repositories;

/// <summary>
/// Read-only repository interface for query operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public interface IReadRepository<TEntity, TId>
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
    /// Gets entities matching a specification.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the predicate.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching the specification or null.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specification.
    /// </summary>
    Task<int> CountAsync(
        ISpecification<TEntity>? specification = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated results matching the specification.
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Non-generic read repository interface.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IReadRepository<TEntity> : IReadRepository<TEntity, Guid>
    where TEntity : Entity<Guid>
{
}

/// <summary>
/// Represents a paginated result.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
