using Microsoft.EntityFrameworkCore;
using Nalam360.Platform.Core.Time;
using Nalam360.Platform.Domain.Events;
using Nalam360.Platform.Domain.Primitives;

namespace Nalam360.Platform.Data.EntityFramework;

/// <summary>
/// Base DbContext with domain event dispatch and audit support.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ITimeProvider _timeProvider;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    protected BaseDbContext(
        DbContextOptions options,
        ITimeProvider timeProvider,
        IDomainEventDispatcher? domainEventDispatcher = null)
        : base(options)
    {
        _timeProvider = timeProvider;
        _domainEventDispatcher = domainEventDispatcher;
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates audit fields (CreatedAt, ModifiedAt) on entities.
    /// </summary>
    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IAuditable &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var auditable = (IAuditable)entry.Entity;
            var now = _timeProvider.UtcNow;

            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAt = now;
            }

            auditable.ModifiedAt = now;
        }
    }

    /// <summary>
    /// Dispatches domain events from aggregate roots.
    /// </summary>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_domainEventDispatcher is null)
            return;

        var aggregateRoots = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(ar => ar.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(ar => ar.DomainEvents)
            .ToList();

        aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }
    }
}

/// <summary>
/// Interface for auditable entities.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the entity was last modified.
    /// </summary>
    DateTime? ModifiedAt { get; set; }
}
