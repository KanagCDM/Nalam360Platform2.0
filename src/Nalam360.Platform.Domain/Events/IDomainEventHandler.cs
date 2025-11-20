namespace Nalam360.Platform.Domain.Events;

/// <summary>
/// Handles domain events
/// </summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : class
{
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}
