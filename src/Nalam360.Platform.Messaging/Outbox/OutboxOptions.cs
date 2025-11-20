namespace Nalam360.Platform.Messaging.Outbox;

/// <summary>
/// Configuration options for the outbox pattern.
/// </summary>
public class OutboxOptions
{
    /// <summary>
    /// Gets or sets the interval between processing attempts.
    /// </summary>
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the batch size for processing messages.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to automatically cleanup old messages.
    /// </summary>
    public bool CleanupEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of days after which to cleanup messages.
    /// </summary>
    public int CleanupAfterDays { get; set; } = 30;
}
