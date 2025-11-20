using Nalam360.Platform.Core.Results;

namespace Nalam360.Platform.Messaging;

/// <summary>
/// Event bus interface for publishing domain events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to the message bus.
    /// </summary>
    Task<Result> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : class;

    /// <summary>
    /// Publishes multiple events to the message bus.
    /// </summary>
    Task<Result> PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken cancellationToken = default)
        where TEvent : class;
}

/// <summary>
/// Integration event marker interface.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for integration events.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
