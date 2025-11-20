using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Messaging.Idempotency;

/// <summary>
/// Store for idempotency keys and their results.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Stores a result for an idempotency key.
    /// </summary>
    Task StoreResultAsync<TResponse>(
        string idempotencyKey,
        Result<TResponse> result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a stored result for an idempotency key.
    /// </summary>
    Task<Result<TResponse>?> GetResultAsync<TResponse>(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an idempotency key exists.
    /// </summary>
    Task<bool> ExistsAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an idempotency key from the store.
    /// </summary>
    Task RemoveAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
