namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Configuration options for circuit breaker.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the failure threshold (0.0 to 1.0).
    /// </summary>
    public double FailureThreshold { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the minimum throughput before circuit breaker activates.
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Gets or sets the sampling duration for measuring failures.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the break duration when circuit is open.
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}
