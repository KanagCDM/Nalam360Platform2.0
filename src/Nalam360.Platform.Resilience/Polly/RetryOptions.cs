namespace Nalam360.Platform.Resilience.Polly;

/// <summary>
/// Configuration options for retry policy.
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay between retries.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets whether to use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to add jitter to retry delays.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}
