namespace Nalam360.Platform.Domain.Primitives;

/// <summary>
/// Base class for aggregate roots
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() : base() { }

    /// <summary>
    /// Adds a domain event to be dispatched
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Removes a specific domain event
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }
}

/// <summary>
/// Base class for aggregate roots with GUID identifiers
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() : base() { }
}

/// <summary>
/// Marker interface for aggregate roots
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
