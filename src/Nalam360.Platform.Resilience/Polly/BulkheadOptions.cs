namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Configuration options for bulkhead isolation.
/// </summary>
public class BulkheadOptions
{
    /// <summary>
    /// Gets or sets the maximum number of parallel executions.
    /// </summary>
    public int MaxParallelization { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of queued actions.
    /// </summary>
    public int MaxQueuingActions { get; set; } = 20;
}
