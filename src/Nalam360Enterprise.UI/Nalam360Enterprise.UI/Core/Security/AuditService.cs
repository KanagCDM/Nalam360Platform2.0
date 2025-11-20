namespace Nalam360Enterprise.UI.Core.Security;

/// <summary>
/// Metadata for audit trail tracking
/// </summary>
public class AuditMetadata
{
    /// <summary>
    /// User who performed the action
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username who performed the action
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Timestamp when the action occurred
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Action performed
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Component or resource affected
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Additional metadata as key-value pairs
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();

    /// <summary>
    /// IP address of the user
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Session identifier
    /// </summary>
    public string? SessionId { get; set; }
}

/// <summary>
/// Event arguments for audit events
/// </summary>
public class AuditEventArgs : EventArgs
{
    public AuditMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Service for handling audit logging
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit event
    /// </summary>
    Task LogAsync(AuditMetadata metadata);

    /// <summary>
    /// Event raised when an audit event occurs
    /// </summary>
    event EventHandler<AuditEventArgs>? AuditEventOccurred;
}

/// <summary>
/// Default implementation of IAuditService
/// </summary>
public class AuditService : IAuditService
{
    public event EventHandler<AuditEventArgs>? AuditEventOccurred;

    public Task LogAsync(AuditMetadata metadata)
    {
        // Raise event for subscribers
        AuditEventOccurred?.Invoke(this, new AuditEventArgs { Metadata = metadata });

        // In a real implementation, this would write to a persistent store
        // For now, we just raise the event so consumers can handle it
        return Task.CompletedTask;
    }
}
