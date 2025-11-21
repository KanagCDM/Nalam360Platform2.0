using System.Collections.Concurrent;

namespace Nalam360Enterprise.UI.Core.Notifications;

/// <summary>
/// Default implementation of INotificationQueue
/// </summary>
public class NotificationQueue : INotificationQueue
{
    private readonly ConcurrentDictionary<Guid, NotificationMessage> _notifications = new();
    private readonly object _lock = new();

    /// <inheritdoc/>
    public event Action<NotificationMessage>? OnNotificationAdded;

    /// <inheritdoc/>
    public event Action<Guid>? OnNotificationRemoved;

    /// <inheritdoc/>
    public Guid Add(string message, NotificationType type = NotificationType.Info, int? duration = 5000)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or whitespace", nameof(message));
        }

        var notification = new NotificationMessage
        {
            Id = Guid.NewGuid(),
            Message = message,
            Type = type,
            Duration = duration,
            CreatedAt = DateTime.UtcNow
        };

        _notifications[notification.Id] = notification;
        OnNotificationAdded?.Invoke(notification);

        // Auto-remove after duration if specified
        if (duration.HasValue && duration.Value > 0)
        {
            Task.Delay(duration.Value).ContinueWith(_ => Remove(notification.Id));
        }

        return notification.Id;
    }

    /// <inheritdoc/>
    public Guid Success(string message, int? duration = 5000)
    {
        return Add(message, NotificationType.Success, duration);
    }

    /// <inheritdoc/>
    public Guid Error(string message, int? duration = 8000)
    {
        return Add(message, NotificationType.Error, duration);
    }

    /// <inheritdoc/>
    public Guid Warning(string message, int? duration = 6000)
    {
        return Add(message, NotificationType.Warning, duration);
    }

    /// <inheritdoc/>
    public Guid Info(string message, int? duration = 5000)
    {
        return Add(message, NotificationType.Info, duration);
    }

    /// <inheritdoc/>
    public void Remove(Guid id)
    {
        if (_notifications.TryRemove(id, out _))
        {
            OnNotificationRemoved?.Invoke(id);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        var ids = _notifications.Keys.ToList();
        _notifications.Clear();

        foreach (var id in ids)
        {
            OnNotificationRemoved?.Invoke(id);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<NotificationMessage> GetAll()
    {
        return _notifications.Values.OrderByDescending(n => n.CreatedAt).ToList();
    }
}
