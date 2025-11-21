using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using Polly;
using Polly.Timeout;

namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Polly-based rate limiter implementation using concurrency limiter.
/// </summary>
public class PollyRateLimiter : IRateLimiter
{
    private readonly ILogger<PollyRateLimiter> _logger;
    private readonly RateLimiterOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private readonly ResiliencePipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="PollyRateLimiter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The rate limiter options.</param>
    public PollyRateLimiter(
        ILogger<PollyRateLimiter> logger,
        IOptions<RateLimiterOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _semaphore = new SemaphoreSlim(_options.PermitLimit, _options.PermitLimit);

        // Build pipeline with timeout to prevent deadlocks
        _pipeline = new ResiliencePipelineBuilder()
            .AddTimeout(TimeSpan.FromSeconds(30))
            .Build();
    }

    /// <inheritdoc/>
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        var acquired = await _semaphore.WaitAsync(0, cancellationToken);
        
        if (!acquired)
        {
            _logger.LogWarning("Rate limit exceeded - permit limit: {PermitLimit}", _options.PermitLimit);
            return Result<T>.Failure(Error.TooManyRequests(
                "RateLimiter.LimitExceeded",
                "Too many requests, please try again later"));
        }

        try
        {
            return await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogWarning(ex, "Rate limiter operation timeout");
            return Result<T>.Failure(Error.Internal(
                "RateLimiter.Timeout",
                "Operation timed out"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limiter execution failed");
            return Result<T>.Failure(Error.Internal(
                "RateLimiter.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        var acquired = await _semaphore.WaitAsync(0, cancellationToken);
        
        if (!acquired)
        {
            _logger.LogWarning("Rate limit exceeded - permit limit: {PermitLimit}", _options.PermitLimit);
            return Result.Failure(Error.TooManyRequests(
                "RateLimiter.LimitExceeded",
                "Too many requests, please try again later"));
        }

        try
        {
            return await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogWarning(ex, "Rate limiter operation timeout");
            return Result.Failure(Error.Internal(
                "RateLimiter.Timeout",
                "Operation timed out"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limiter execution failed");
            return Result.Failure(Error.Internal(
                "RateLimiter.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsAllowedAsync(CancellationToken cancellationToken = default)
    {
        var currentCount = _semaphore.CurrentCount;
        return Task.FromResult(currentCount > 0);
    }
}
