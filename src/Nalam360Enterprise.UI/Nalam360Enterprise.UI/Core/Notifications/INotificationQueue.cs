namespace Nalam360Enterprise.UI.Core.Notifications;

/// <summary>
/// Manages a queue of notifications for display to users
/// </summary>
public interface INotificationQueue
{
    /// <summary>
    /// Event triggered when a notification is added to the queue
    /// </summary>
    event Action<NotificationMessage>? OnNotificationAdded;

    /// <summary>
    /// Event triggered when a notification is removed from the queue
    /// </summary>
    event Action<Guid>? OnNotificationRemoved;

    /// <summary>
    /// Adds a notification to the queue
    /// </summary>
    /// <param name="message">The notification message</param>
    /// <param name="type">The notification type</param>
    /// <param name="duration">Duration in milliseconds (null for persistent)</param>
    /// <returns>The notification ID</returns>
    Guid Add(string message, NotificationType type = NotificationType.Info, int? duration = 5000);

    /// <summary>
    /// Adds a success notification
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns>The notification ID</returns>
    Guid Success(string message, int? duration = 5000);

    /// <summary>
    /// Adds an error notification
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns>The notification ID</returns>
    Guid Error(string message, int? duration = 8000);

    /// <summary>
    /// Adds a warning notification
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns>The notification ID</returns>
    Guid Warning(string message, int? duration = 6000);

    /// <summary>
    /// Adds an info notification
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="duration">Duration in milliseconds</param>
    /// <returns>The notification ID</returns>
    Guid Info(string message, int? duration = 5000);

    /// <summary>
    /// Removes a notification from the queue
    /// </summary>
    /// <param name="id">The notification ID</param>
    void Remove(Guid id);

    /// <summary>
    /// Removes all notifications from the queue
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets all current notifications
    /// </summary>
    /// <returns>List of notifications</returns>
    IReadOnlyList<NotificationMessage> GetAll();
}

/// <summary>
/// Notification types
/// </summary>
public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>
/// Represents a notification message
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The notification message
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The notification type
    /// </summary>
    public NotificationType Type { get; init; }

    /// <summary>
    /// Duration in milliseconds (null for persistent)
    /// </summary>
    public int? Duration { get; init; }

    /// <summary>
    /// When the notification was created
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
