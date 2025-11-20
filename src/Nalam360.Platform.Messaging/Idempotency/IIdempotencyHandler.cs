using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Messaging.Idempotency;

/// <summary>
/// Handler for ensuring idempotent message processing.
/// </summary>
public interface IIdempotencyHandler
{
    /// <summary>
    /// Handles a request with idempotency guarantee.
    /// </summary>
    Task<Result<TResponse>> HandleAsync<TResponse>(
        string idempotencyKey,
        Func<CancellationToken, Task<Result<TResponse>>> handler,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a request has already been processed.
    /// </summary>
    Task<bool> IsProcessedAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an idempotency key from the store.
    /// </summary>
    Task<Result> RemoveAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}
