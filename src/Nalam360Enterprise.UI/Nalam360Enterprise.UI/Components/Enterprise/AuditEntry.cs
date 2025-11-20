namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Represents a single audit log entry
    /// </summary>
    public class AuditEntry
    {
        /// <summary>
        /// Unique identifier for the audit entry
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Action performed (e.g., Create, Update, Delete, View)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Resource that was acted upon (e.g., User, Order, Patient)
        /// </summary>
        public string Resource { get; set; } = string.Empty;

        /// <summary>
        /// When the action occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User ID who performed the action
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// User display name
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// IP address of the user
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent string (browser/client info)
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Additional data specific to this audit event
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new();

        /// <summary>
        /// Whether the action was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if action failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Duration of the action in milliseconds
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Previous value before the change (for updates)
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// New value after the change (for updates)
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// Severity level of the audit event
        /// </summary>
        public AuditSeverity Severity { get; set; } = AuditSeverity.Information;

        /// <summary>
        /// Category for grouping related audit events
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Tags for filtering and searching
        /// </summary>
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Severity level for audit events
    /// </summary>
    public enum AuditSeverity
    {
        /// <summary>
        /// Informational event
        /// </summary>
        Information,

        /// <summary>
        /// Warning event
        /// </summary>
        Warning,

        /// <summary>
        /// Error event
        /// </summary>
        Error,

        /// <summary>
        /// Critical event
        /// </summary>
        Critical
    }
}
