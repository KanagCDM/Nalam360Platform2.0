using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Resilience;

/// <summary>
/// Abstraction for retry policies.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Executes an action with retry logic.
    /// </summary>
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action with retry logic.
    /// </summary>
    Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default);
}
