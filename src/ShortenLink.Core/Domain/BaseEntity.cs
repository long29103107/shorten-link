namespace ShortenLink.Core.Domain;

public abstract class BaseEntity<TKey>
    where TKey : notnull
{
    protected BaseEntity()
        : this(DateTimeOffset.UtcNow)
    {
    }

    protected BaseEntity(
        DateTimeOffset createdAt,
        TKey? id = default,
        Guid? createdBy = null,
        Guid? updatedBy = null,
        DateTimeOffset? updatedAt = null)
    {
        if (typeof(TKey) != typeof(Guid))
        {
            throw new NotSupportedException($"{nameof(BaseEntity<TKey>)} currently supports Guid keys only.");
        }

        Id = EqualityComparer<TKey>.Default.Equals(id!, default!)
            ? (TKey)(object)Guid.CreateVersion7()
            : id!;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public TKey Id { get; protected set; }

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
