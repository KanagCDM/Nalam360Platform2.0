using Microsoft.EntityFrameworkCore;
using Nalam360.Platform.Data.Specifications;

namespace Nalam360.Platform.Data.EntityFramework;

/// <summary>
/// Evaluates specifications and applies them to queryables.
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies a specification to a queryable.
    /// </summary>
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

        // Apply criteria
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply include strings
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply grouping
        if (specification.GroupBy is not null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        // Apply paging
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        // Apply no tracking
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply split query
        if (specification.AsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        return query;
    }
}
