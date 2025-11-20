using Microsoft.Extensions.Logging;
using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Messaging.Idempotency;

/// <summary>
/// Handler for ensuring idempotent message processing.
/// </summary>
public class IdempotencyHandler : IIdempotencyHandler
{
    private readonly ILogger<IdempotencyHandler> _logger;
    private readonly IIdempotencyStore _store;

    public IdempotencyHandler(
        ILogger<IdempotencyHandler> logger,
        IIdempotencyStore store)
    {
        _logger = logger;
        _store = store;
    }

    /// <inheritdoc/>
    public async Task<Result<TResponse>> HandleAsync<TResponse>(
        string idempotencyKey,
        Func<CancellationToken, Task<Result<TResponse>>> handler,
        CancellationToken cancellationToken = default)
    {
        // Check if already processed
        var existingResult = await _store.GetResultAsync<TResponse>(
            idempotencyKey,
            cancellationToken);

        if (existingResult != null && existingResult.Value.IsSuccess)
        {
            _logger.LogInformation(
                "Idempotency key {Key} already processed, returning cached result",
                idempotencyKey);

            return existingResult.Value;
        }

        // Process the request
        var result = await handler(cancellationToken);

        // Store the result
        await _store.StoreResultAsync(idempotencyKey, result, cancellationToken);

        _logger.LogInformation(
            "Processed request with idempotency key {Key}",
            idempotencyKey);

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> IsProcessedAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _store.ExistsAsync(idempotencyKey, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result> RemoveAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        await _store.RemoveAsync(idempotencyKey, cancellationToken);
        
        _logger.LogInformation(
            "Removed idempotency key {Key}",
            idempotencyKey);

        return Result.Success();
    }
}
