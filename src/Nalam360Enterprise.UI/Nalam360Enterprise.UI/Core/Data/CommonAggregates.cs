using System;
using System.Collections.Generic;
using System.Linq;

namespace Nalam360Enterprise.UI.Core.Data;

/// <summary>
/// Collection of common custom aggregate functions
/// </summary>
public static class CommonAggregates
{
    /// <summary>
    /// Creates a Variance aggregate function
    /// </summary>
    public static ICustomAggregateFunction<TValue> Variance<TValue>() 
        => new VarianceAggregate<TValue>();

    /// <summary>
    /// Creates a Standard Deviation aggregate function
    /// </summary>
    public static ICustomAggregateFunction<TValue> StandardDeviation<TValue>() 
        => new StandardDeviationAggregate<TValue>();

    /// <summary>
    /// Creates a Median aggregate function
    /// </summary>
    public static ICustomAggregateFunction<TValue> Median<TValue>() 
        => new MedianAggregate<TValue>();

    /// <summary>
    /// Creates a Mode aggregate function
    /// </summary>
    public static ICustomAggregateFunction<TValue> Mode<TValue>() 
        => new ModeAggregate<TValue>();

    /// <summary>
    /// Creates a Range aggregate function (Max - Min)
    /// </summary>
    public static ICustomAggregateFunction<TValue> Range<TValue>() 
        => new RangeAggregate<TValue>();

    /// <summary>
    /// Creates a Product aggregate function (multiply all values)
    /// </summary>
    public static ICustomAggregateFunction<TValue> Product<TValue>() 
        => new ProductAggregate<TValue>();

    /// <summary>
    /// Creates a custom aggregate function using a lambda expression
    /// </summary>
    public static ICustomAggregateFunction<TValue> Custom<TValue>(
        string name, 
        Func<IEnumerable<TValue>, string, object?> calculator,
        string? format = null)
        => new LambdaAggregate<TValue>(name, calculator, format);
}

// Implementations
internal class VarianceAggregate<TValue> : CustomAggregateFunction<TValue>
{
    public VarianceAggregate() : base("Variance", "N2") { }

    public override object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        var values = GetNumericValues(data, fieldName).ToList();
        if (values.Count == 0) return null;

        var mean = values.Average();
        var sumOfSquaredDifferences = values.Sum(v => Math.Pow(v - mean, 2));
        return sumOfSquaredDifferences / values.Count;
    }
}

internal class StandardDeviationAggregate<TValue> : CustomAggregateFunction<TValue>
{
    public StandardDeviationAggregate() : base("Standard Deviation", "N2") { }

    public override object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        var values = GetNumericValues(data, fieldName).ToList();
        if (values.Count == 0) return null;

        var mean = values.Average();
        var sumOfSquaredDifferences = values.Sum(v => Math.Pow(v - mean, 2));
        var variance = sumOfSquaredDifferences / values.Count;
        return Math.Sqrt(variance);
    }
}

internal class MedianAggregate<TValue> : CustomAggregateFunction<TValue>
{
    public MedianAggregate() : base("Median", "N2") { }

    public override object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        var values = GetNumericValues(data, fieldName).OrderBy(v => v).ToList();
        if (values.Count == 0) return null;

        var mid = values.Count / 2;
        return values.Count % 2 == 0 
            ? (values[mid - 1] + values[mid]) / 2.0 
            : values[mid];
    }
}

internal class ModeAggregate<TValue> : CustomAggregateFunction<TValue>
{
    public ModeAggregate() : base("Mode", "N2") { }

    public override object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        var values = GetNumericValues(data, fieldName).ToList();
        if (values.Count == 0) return null;

        var grouped = values.GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return grouped?.Key;
    }
}

internal class RangeAggregate<TValue> : CustomAggregateFunction<TValue>
{
    public RangeAggregate() : base("Range", "N2") { }

    public override object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        var values = GetNumericValues(data, fieldName).ToList();
        if (values.Count == 0) return null;

        return values.Max() - values.Min();
    }
}

internal class ProductAggregate<TValue> : CustomAggregateFunction<TValue>
{
    public ProductAggregate() : base("Product", "N2") { }

    public override object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        var values = GetNumericValues(data, fieldName).ToList();
        if (values.Count == 0) return null;

        return values.Aggregate(1.0, (acc, v) => acc * v);
    }
}

internal class LambdaAggregate<TValue> : ICustomAggregateFunction<TValue>
{
    private readonly Func<IEnumerable<TValue>, string, object?> _calculator;

    public LambdaAggregate(string name, Func<IEnumerable<TValue>, string, object?> calculator, string? format)
    {
        Name = name;
        _calculator = calculator;
        Format = format;
    }

    public string Name { get; }
    public string? Format { get; }

    public object? Calculate(IEnumerable<TValue> data, string fieldName)
    {
        return _calculator(data, fieldName);
    }
}
