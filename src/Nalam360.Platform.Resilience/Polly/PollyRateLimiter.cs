using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using Polly;
using Polly.RateLimiting;
using System.Threading.RateLimiting;

namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Polly-based rate limiter implementation.
/// </summary>
public class PollyRateLimiter : IRateLimiter
{
    private readonly ILogger<PollyRateLimiter> _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly RateLimiterOptions _options;

    public PollyRateLimiter(
        ILogger<PollyRateLimiter> logger,
        IOptions<RateLimiterOptions> options)
    {
        _logger = logger;
        _options = options.Value;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = args =>
                {
                    return new ConcurrencyLimiter(new ConcurrencyLimiterOptions
                    {
                        PermitLimit = _options.PermitLimit,
                        QueueLimit = _options.QueueLimit
                    }).AcquireAsync(1, args.Context.CancellationToken);
                },
                OnRejected = args =>
                {
                    _logger.LogWarning("Rate limit exceeded, request rejected");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <inheritdoc/>
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
        }
        catch (RateLimiterRejectedException ex)
        {
            _logger.LogWarning("Rate limit exceeded");
            return Result<T>.Failure(Error.TooManyRequests(
                "RateLimiter.LimitExceeded",
                "Too many requests, please try again later"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limiter execution failed");
            return Result<T>.Failure(Error.Internal(
                "RateLimiter.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
        }
        catch (RateLimiterRejectedException ex)
        {
            _logger.LogWarning("Rate limit exceeded");
            return Result.Failure(Error.TooManyRequests(
                "RateLimiter.LimitExceeded",
                "Too many requests, please try again later"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limiter execution failed");
            return Result.Failure(Error.Internal(
                "RateLimiter.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsAllowedAsync(CancellationToken cancellationToken = default)
    {
        // Simplified check - in production you'd track actual permits
        return Task.FromResult(true);
    }
}
