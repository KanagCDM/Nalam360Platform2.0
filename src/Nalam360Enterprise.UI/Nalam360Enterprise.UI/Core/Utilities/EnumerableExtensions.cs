namespace Nalam360Enterprise.UI.Core.Utilities;

/// <summary>
/// Extension methods for IEnumerable collections
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Chunks a collection into batches of specified size
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="size">Chunk size</param>
    /// <returns>Collection of chunks</returns>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int size)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (size <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than 0", nameof(size));
        }

        var list = source.ToList();
        for (int i = 0; i < list.Count; i += size)
        {
            yield return list.Skip(i).Take(size);
        }
    }

    /// <summary>
    /// Returns distinct elements by a key selector
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="keySelector">Key selector function</param>
    /// <returns>Distinct elements</returns>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var seenKeys = new HashSet<TKey>();
        foreach (var element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    /// <summary>
    /// Checks if a collection is null or empty
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <returns>True if null or empty</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source == null || !source.Any();
    }

    /// <summary>
    /// Executes an action on each element
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="action">Action to execute</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Executes an async action on each element
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="action">Async action to execute</param>
    public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> action)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        foreach (var item in source)
        {
            await action(item);
        }
    }

    /// <summary>
    /// Converts an enumerable to a delimited string
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="delimiter">Delimiter string</param>
    /// <returns>Delimited string</returns>
    public static string ToDelimitedString<T>(this IEnumerable<T> source, string delimiter = ", ")
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return string.Join(delimiter, source);
    }

    /// <summary>
    /// Gets a random element from the collection
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <returns>Random element</returns>
    public static T Random<T>(this IEnumerable<T> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var list = source.ToList();
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Cannot get random element from empty collection");
        }

        var random = new Random();
        return list[random.Next(list.Count)];
    }

    /// <summary>
    /// Shuffles the collection randomly
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <returns>Shuffled collection</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var list = source.ToList();
        var random = new Random();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            yield return list[j];
            list[j] = list[i];
        }

        if (list.Count > 0)
        {
            yield return list[0];
        }
    }

    /// <summary>
    /// Takes a random sample of specified size from the collection
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="count">Sample size</param>
    /// <returns>Random sample</returns>
    public static IEnumerable<T> Sample<T>(this IEnumerable<T> source, int count)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (count < 0)
        {
            throw new ArgumentException("Count must be non-negative", nameof(count));
        }

        return source.Shuffle().Take(count);
    }

    /// <summary>
    /// Partitions a collection into two groups based on a predicate
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="predicate">Predicate function</param>
    /// <returns>Tuple of (matching items, non-matching items)</returns>
    public static (IEnumerable<T> Matching, IEnumerable<T> NonMatching) Partition<T>(
        this IEnumerable<T> source,
        Func<T, bool> predicate)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var matching = new List<T>();
        var nonMatching = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
            {
                matching.Add(item);
            }
            else
            {
                nonMatching.Add(item);
            }
        }

        return (matching, nonMatching);
    }

    /// <summary>
    /// Returns the element with the maximum value for a selector
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="selector">Value selector</param>
    /// <returns>Element with maximum value</returns>
    public static T? MaxBy<T, TValue>(this IEnumerable<T> source, Func<T, TValue> selector)
        where TValue : IComparable<TValue>
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return default;
        }

        var maxItem = enumerator.Current;
        var maxValue = selector(maxItem);

        while (enumerator.MoveNext())
        {
            var currentValue = selector(enumerator.Current);
            if (currentValue.CompareTo(maxValue) > 0)
            {
                maxValue = currentValue;
                maxItem = enumerator.Current;
            }
        }

        return maxItem;
    }

    /// <summary>
    /// Returns the element with the minimum value for a selector
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="selector">Value selector</param>
    /// <returns>Element with minimum value</returns>
    public static T? MinBy<T, TValue>(this IEnumerable<T> source, Func<T, TValue> selector)
        where TValue : IComparable<TValue>
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return default;
        }

        var minItem = enumerator.Current;
        var minValue = selector(minItem);

        while (enumerator.MoveNext())
        {
            var currentValue = selector(enumerator.Current);
            if (currentValue.CompareTo(minValue) < 0)
            {
                minValue = currentValue;
                minItem = enumerator.Current;
            }
        }

        return minItem;
    }

    /// <summary>
    /// Groups consecutive elements that match a predicate
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="source">Source collection</param>
    /// <param name="predicate">Grouping predicate</param>
    /// <returns>Groups of consecutive matching elements</returns>
    public static IEnumerable<IEnumerable<T>> GroupConsecutive<T>(
        this IEnumerable<T> source,
        Func<T, T, bool> predicate)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        using var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            yield break;
        }

        var currentGroup = new List<T> { enumerator.Current };

        while (enumerator.MoveNext())
        {
            if (predicate(currentGroup[currentGroup.Count - 1], enumerator.Current))
            {
                currentGroup.Add(enumerator.Current);
            }
            else
            {
                yield return currentGroup;
                currentGroup = new List<T> { enumerator.Current };
            }
        }

        yield return currentGroup;
    }
}
