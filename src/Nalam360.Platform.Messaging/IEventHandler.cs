namespace Nalam360.Platform.Messaging;

/// <summary>
/// Event handler interface for consuming events.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : class
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
