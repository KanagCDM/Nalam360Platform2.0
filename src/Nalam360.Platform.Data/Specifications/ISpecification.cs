using System.Linq.Expressions;

namespace Nalam360.Platform.Data.Specifications;

/// <summary>
/// Specification pattern interface for building complex queries.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the filter criteria.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Gets the include expressions for eager loading.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the include string expressions for eager loading.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the order by expression.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Gets the order by descending expression.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Gets the group by expression.
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }

    /// <summary>
    /// Gets the number of items to take.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets the number of items to skip.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets whether to disable change tracking.
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Gets whether to split queries.
    /// </summary>
    bool AsSplitQuery { get; }
}
