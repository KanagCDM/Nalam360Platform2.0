using System;
using System.Collections.Generic;
using System.Linq;

namespace Nalam360Enterprise.UI.Core.Data;

/// <summary>
/// Manages custom aggregate functions for grid components
/// </summary>
/// <typeparam name="TValue">The type of the data items</typeparam>
public class GridAggregateManager<TValue>
{
    private readonly Dictionary<string, ICustomAggregateFunction<TValue>> _aggregates = new();

    /// <summary>
    /// Registers a custom aggregate function
    /// </summary>
    public void Register(ICustomAggregateFunction<TValue> aggregate)
    {
        _aggregates[aggregate.Name] = aggregate;
    }

    /// <summary>
    /// Registers multiple custom aggregate functions
    /// </summary>
    public void RegisterRange(params ICustomAggregateFunction<TValue>[] aggregates)
    {
        foreach (var aggregate in aggregates)
        {
            Register(aggregate);
        }
    }

    /// <summary>
    /// Unregisters a custom aggregate function
    /// </summary>
    public bool Unregister(string name)
    {
        return _aggregates.Remove(name);
    }

    /// <summary>
    /// Gets a registered aggregate function by name
    /// </summary>
    public ICustomAggregateFunction<TValue>? GetAggregate(string name)
    {
        return _aggregates.TryGetValue(name, out var aggregate) ? aggregate : null;
    }

    /// <summary>
    /// Checks if an aggregate function is registered
    /// </summary>
    public bool IsRegistered(string name)
    {
        return _aggregates.ContainsKey(name);
    }

    /// <summary>
    /// Gets all registered aggregate function names
    /// </summary>
    public IEnumerable<string> GetRegisteredNames()
    {
        return _aggregates.Keys;
    }

    /// <summary>
    /// Calculates an aggregate value for the given data and field
    /// </summary>
    public object? Calculate(string aggregateName, IEnumerable<TValue> data, string fieldName)
    {
        if (!_aggregates.TryGetValue(aggregateName, out var aggregate))
        {
            throw new InvalidOperationException($"Aggregate function '{aggregateName}' is not registered.");
        }

        return aggregate.Calculate(data, fieldName);
    }

    /// <summary>
    /// Calculates multiple aggregates for the given data and field
    /// </summary>
    public Dictionary<string, object?> CalculateMultiple(
        IEnumerable<string> aggregateNames, 
        IEnumerable<TValue> data, 
        string fieldName)
    {
        var results = new Dictionary<string, object?>();
        var dataList = data.ToList(); // Materialize once

        foreach (var name in aggregateNames)
        {
            if (_aggregates.TryGetValue(name, out var aggregate))
            {
                results[name] = aggregate.Calculate(dataList, fieldName);
            }
        }

        return results;
    }

    /// <summary>
    /// Clears all registered aggregate functions
    /// </summary>
    public void Clear()
    {
        _aggregates.Clear();
    }

    /// <summary>
    /// Gets the count of registered aggregate functions
    /// </summary>
    public int Count => _aggregates.Count;
}

/// <summary>
/// Extension methods for GridAggregateManager
/// </summary>
public static class GridAggregateManagerExtensions
{
    /// <summary>
    /// Registers all common aggregate functions
    /// </summary>
    public static GridAggregateManager<TValue> RegisterCommonAggregates<TValue>(
        this GridAggregateManager<TValue> manager)
    {
        manager.RegisterRange(
            CommonAggregates.Variance<TValue>(),
            CommonAggregates.StandardDeviation<TValue>(),
            CommonAggregates.Median<TValue>(),
            CommonAggregates.Mode<TValue>(),
            CommonAggregates.Range<TValue>(),
            CommonAggregates.Product<TValue>()
        );

        return manager;
    }

    /// <summary>
    /// Registers a custom aggregate function using a lambda
    /// </summary>
    public static GridAggregateManager<TValue> RegisterCustom<TValue>(
        this GridAggregateManager<TValue> manager,
        string name,
        Func<IEnumerable<TValue>, string, object?> calculator,
        string? format = null)
    {
        manager.Register(CommonAggregates.Custom(name, calculator, format));
        return manager;
    }
}
