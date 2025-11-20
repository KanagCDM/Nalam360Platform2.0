namespace Nalam360Enterprise.UI.Core.Data;

/// <summary>
/// Represents a custom aggregation function that can be applied to grid data
/// </summary>
/// <typeparam name="TValue">The type of the data items</typeparam>
public interface ICustomAggregateFunction<TValue>
{
    /// <summary>
    /// Name of the aggregate function (e.g., "Variance", "Median")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Display format for the aggregate result
    /// </summary>
    string? Format { get; }

    /// <summary>
    /// Calculates the aggregate value from a collection of items
    /// </summary>
    /// <param name="data">The data items to aggregate</param>
    /// <param name="fieldName">The field name to aggregate on</param>
    /// <returns>The calculated aggregate value</returns>
    object? Calculate(IEnumerable<TValue> data, string fieldName);
}

/// <summary>
/// Base class for implementing custom aggregate functions
/// </summary>
/// <typeparam name="TValue">The type of the data items</typeparam>
public abstract class CustomAggregateFunction<TValue> : ICustomAggregateFunction<TValue>
{
    protected CustomAggregateFunction(string name, string? format = null)
    {
        Name = name;
        Format = format;
    }

    public string Name { get; }
    public string? Format { get; }

    public abstract object? Calculate(IEnumerable<TValue> data, string fieldName);

    /// <summary>
    /// Helper method to extract numeric values from data using reflection
    /// </summary>
    protected IEnumerable<double> GetNumericValues(IEnumerable<TValue> data, string fieldName)
    {
        var property = typeof(TValue).GetProperty(fieldName);
        if (property == null)
            yield break;

        foreach (var item in data)
        {
            var value = property.GetValue(item);
            if (value != null && TryConvertToDouble(value, out var numericValue))
            {
                yield return numericValue;
            }
        }
    }

    private bool TryConvertToDouble(object value, out double result)
    {
        return double.TryParse(value.ToString(), out result);
    }
}
