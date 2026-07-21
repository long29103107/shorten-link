namespace ShortenLink.Core.Domain;

public abstract class BaseEntity
{
    protected BaseEntity(
        DateTimeOffset createdAt,
        Guid? id = null,
        Guid? createdBy = null,
        Guid? updatedBy = null,
        DateTimeOffset? updatedAt = null)
    {
        Id = id ?? Guid.CreateVersion7();
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; protected set; }

    public Guid? CreatedBy { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public Guid? UpdatedBy { get; protected set; }

    public DateTimeOffset? UpdatedAt { get; protected set; }

    public void MarkUpdated(Guid? updatedBy, DateTimeOffset updatedAt)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }
}
