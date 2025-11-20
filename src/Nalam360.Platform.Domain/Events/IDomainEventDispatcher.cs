namespace Nalam360.Platform.Domain.Events;

/// <summary>
/// Dispatches domain events to their handlers
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : class;

    Task DispatchAsync(IEnumerable<object> domainEvents, CancellationToken cancellationToken = default);
}
