namespace Nalam360.Platform.Domain.Primitives;

/// <summary>
/// Base class for all entities with strongly-typed identifiers
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
    }

    protected Entity()
    {
        // For EF Core
        Id = default!;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        
        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}

/// <summary>
/// Base class for entities with GUID identifiers
/// </summary>
public abstract class Entity : Entity<Guid>
{
    protected Entity(Guid id) : base(id) { }
    protected Entity() : base() { }
}
