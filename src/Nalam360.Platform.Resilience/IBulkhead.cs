using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Resilience;

/// <summary>
/// Abstraction for bulkhead isolation pattern.
/// </summary>
public interface IBulkhead
{
    /// <summary>
    /// Executes an action with bulkhead isolation.
    /// </summary>
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action with bulkhead isolation.
    /// </summary>
    Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the available capacity.
    /// </summary>
    int AvailableCapacity { get; }
}
