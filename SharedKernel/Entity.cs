using Domain.Abstractions;

namespace SharedKernel;

public abstract class Entity<TEntityId>
{
    public TEntityId Id { get; init; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    protected Entity()
    {
    }

    protected Entity(TEntityId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }
}

public abstract class Entity : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        return [.. _domainEvents];
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

public abstract class EventEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> GetDomainEvents()
    {
        return [.. _domainEvents];
    }

    protected EventEntity(Guid id)
    {
        Id = id;
    }

    protected EventEntity()
    {
    }

    public Guid Id { get; init; }



    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}


