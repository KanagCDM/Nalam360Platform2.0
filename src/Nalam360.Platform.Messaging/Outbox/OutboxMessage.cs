namespace Nalam360.Platform.Messaging.Outbox;

/// <summary>
/// Represents an outbox message for transactional messaging pattern.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Gets or sets the message identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the event type name.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized event payload.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the message was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the message was processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the number of processing attempts.
    /// </summary>
    public int Attempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets the last error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
