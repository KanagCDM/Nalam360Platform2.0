namespace Nalam360Enterprise.UI.Components.Enterprise
{
    /// <summary>
    /// Notification type for categorizing and displaying notifications
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Informational notification
        /// </summary>
        Info,

        /// <summary>
        /// Success notification
        /// </summary>
        Success,

        /// <summary>
        /// Warning notification
        /// </summary>
        Warning,

        /// <summary>
        /// Error notification
        /// </summary>
        Error
    }

    /// <summary>
    /// AI-calculated priority level for notifications
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Low priority - can be reviewed later
        /// </summary>
        Low,

        /// <summary>
        /// Medium priority - should be reviewed soon
        /// </summary>
        Medium,

        /// <summary>
        /// High priority - requires immediate attention
        /// </summary>
        High
    }
}
