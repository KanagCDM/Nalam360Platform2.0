using System.Linq.Expressions;

namespace Nalam360.Platform.Data.Specifications;

/// <summary>
/// Base class for implementing specifications.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes { get; } = new();

    /// <inheritdoc />
    public List<string> IncludeStrings { get; } = new();

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? GroupBy { get; private set; }

    /// <inheritdoc />
    public int? Take { get; private set; }

    /// <inheritdoc />
    public int? Skip { get; private set; }

    /// <inheritdoc />
    public bool AsNoTracking { get; private set; }

    /// <inheritdoc />
    public bool AsSplitQuery { get; private set; }

    /// <summary>
    /// Adds filter criteria.
    /// </summary>
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Adds an include expression for eager loading.
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds an include string for eager loading.
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Adds ordering expression.
    /// </summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Adds descending ordering expression.
    /// </summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// Adds group by expression.
    /// </summary>
    protected void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }

    /// <summary>
    /// Applies paging.
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Disables change tracking for queries.
    /// </summary>
    protected void ApplyNoTracking()
    {
        AsNoTracking = true;
    }

    /// <summary>
    /// Enables split query mode.
    /// </summary>
    protected void ApplySplitQuery()
    {
        AsSplitQuery = true;
    }
}
