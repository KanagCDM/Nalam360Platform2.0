using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Resilience;

/// <summary>
/// Abstraction for rate limiting.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Executes an action with rate limiting.
    /// </summary>
    Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action with rate limiting.
    /// </summary>
    Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the rate limit has been exceeded.
    /// </summary>
    Task<bool> IsAllowedAsync(CancellationToken cancellationToken = default);
}
