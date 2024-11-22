namespace EShop.Domain.Abstractions;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; set; }

    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    public bool Equals(Entity? other)
        => other is not null && Id.Equals(other.Id);
    public override int GetHashCode() 
        => HashCode.Combine(Id, base.GetHashCode());
    public override bool Equals(object? obj)
        => obj is Entity entity && Equals(entity);
}