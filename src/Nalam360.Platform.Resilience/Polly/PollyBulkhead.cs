using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nalam360.Platform.Core.Results;
using Polly;
using Polly.Bulkhead;

namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Polly-based bulkhead implementation.
/// </summary>
public class PollyBulkhead : IBulkhead
{
    private readonly ILogger<PollyBulkhead> _logger;
    private readonly ResiliencePipeline _pipeline;
    private readonly BulkheadOptions _options;
    private int _availableCapacity;

    public PollyBulkhead(
        ILogger<PollyBulkhead> logger,
        IOptions<BulkheadOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _availableCapacity = _options.MaxParallelization;

        _pipeline = new ResiliencePipelineBuilder()
            .AddConcurrencyLimiter(_options.MaxParallelization, _options.MaxQueuingActions)
            .Build();
    }

    /// <inheritdoc/>
    public int AvailableCapacity => _availableCapacity;

    /// <inheritdoc/>
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Interlocked.Decrement(ref _availableCapacity);
            
            var result = await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
            
            return result;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogWarning("Bulkhead limit exceeded");
            return Result<T>.Failure(Error.Unavailable(
                "Bulkhead.LimitExceeded",
                "Service capacity exceeded, please try again later"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulkhead execution failed");
            return Result<T>.Failure(Error.Internal(
                "Bulkhead.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
        finally
        {
            Interlocked.Increment(ref _availableCapacity);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Interlocked.Decrement(ref _availableCapacity);
            
            var result = await _pipeline.ExecuteAsync(async ct => await action(ct), cancellationToken);
            
            return result;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogWarning("Bulkhead limit exceeded");
            return Result.Failure(Error.Unavailable(
                "Bulkhead.LimitExceeded",
                "Service capacity exceeded, please try again later"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulkhead execution failed");
            return Result.Failure(Error.Internal(
                "Bulkhead.ExecutionFailed",
                $"Operation failed: {ex.Message}"));
        }
        finally
        {
            Interlocked.Increment(ref _availableCapacity);
        }
    }
}
