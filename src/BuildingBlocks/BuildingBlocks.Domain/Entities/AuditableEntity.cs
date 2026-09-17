namespace BuildingBlocks.Domain.Entities;

using BuildingBlocks.Domain.Contracts;

public abstract class AuditableEntity<TId> : Entity<TId>, IAuditableEntity
    where TId : notnull
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    protected AuditableEntity() { }
    protected AuditableEntity(TId id) : base(id) { }
}

public abstract class AuditableEntity : AuditableEntity<Guid>
{
    protected AuditableEntity() : base(Guid.NewGuid()) { }
    protected AuditableEntity(Guid id) : base(id) { }
}

