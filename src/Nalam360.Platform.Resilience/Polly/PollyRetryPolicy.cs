using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using Polly;
using Polly.Retry;

namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Polly-based retry policy implementation.
/// </summary>
public class PollyRetryPolicy : IRetryPolicy
{
    private readonly ILogger<PollyRetryPolicy> _logger;
    private readonly ResiliencePipeline _pipeline;

    public PollyRetryPolicy(
        ILogger<PollyRetryPolicy> logger,
        IOptions<RetryOptions> options)
    {
        _logger = logger;
        var opts = options.Value;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = opts.MaxRetryAttempts,
                Delay = opts.InitialDelay,
                BackoffType = opts.UseExponentialBackoff 
                    ? DelayBackoffType.Exponential 
                    : DelayBackoffType.Constant,
                UseJitter = opts.UseJitter,
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        "Retry attempt {Attempt} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry policy execution failed");
            return Result<T>.Failure(Error.Internal(
                "Retry.ExecutionFailed",
                $"Operation failed after retries: {ex.Message}"));
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Retry policy execution failed");
            return Result.Failure(Error.Internal(
                "Retry.ExecutionFailed",
                $"Operation failed after retries: {ex.Message}"));
        }
    }
}
