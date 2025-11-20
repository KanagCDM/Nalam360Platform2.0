using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using Polly;
using Polly.CircuitBreaker;

namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Polly-based circuit breaker implementation.
/// </summary>
public class PollyCircuitBreaker : ICircuitBreaker
{
    private readonly ILogger<PollyCircuitBreaker> _logger;
    private readonly ResiliencePipeline _pipeline;
    private CircuitState _state = CircuitState.Closed;

    public PollyCircuitBreaker(
        ILogger<PollyCircuitBreaker> logger,
        IOptions<CircuitBreakerOptions> options)
    {
        _logger = logger;
        var opts = options.Value;

        _pipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = opts.FailureThreshold,
                MinimumThroughput = opts.MinimumThroughput,
                SamplingDuration = opts.SamplingDuration,
                BreakDuration = opts.BreakDuration,
                OnOpened = args =>
                {
                    _state = CircuitState.Open;
                    _logger.LogWarning("Circuit breaker opened");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _state = CircuitState.Closed;
                    _logger.LogInformation("Circuit breaker closed");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _state = CircuitState.HalfOpen;
                    _logger.LogInformation("Circuit breaker half-opened");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <inheritdoc/>
    public CircuitState State => _state;

    /// <inheritdoc/>
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("Circuit breaker is open, request blocked");
            return Result<T>.Failure(Error.Unavailable(
                "CircuitBreaker.Open",
                "Service temporarily unavailable due to circuit breaker"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Circuit breaker execution failed");
            return Result<T>.Failure(Error.Internal(
                "CircuitBreaker.ExecutionFailed",
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
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("Circuit breaker is open, request blocked");
            return Result.Failure(Error.Unavailable(
                "CircuitBreaker.Open",
                "Service temporarily unavailable due to circuit breaker"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Circuit breaker execution failed");
            return Result.Failure(Error.Internal(
                "CircuitBreaker.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
    }
}
