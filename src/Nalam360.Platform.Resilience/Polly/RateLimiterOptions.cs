namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Configuration options for rate limiter.
/// </summary>
public class RateLimiterOptions
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent permits.
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the queue limit for waiting requests.
    /// </summary>
    public int QueueLimit { get; set; } = 50;

    /// <summary>
    /// Gets or sets the window duration for token bucket.
    /// </summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(1);
}
