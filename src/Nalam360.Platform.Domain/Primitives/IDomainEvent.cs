namespace Nalam360.Platform.Domain.Primitives;

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base class for domain events
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
