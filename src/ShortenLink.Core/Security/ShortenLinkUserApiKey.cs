using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Security;

public sealed class ShortenLinkUserApiKeyEntity : BaseEntity<Guid>
{
    public ShortenLinkUserApiKeyEntity(
        string id,
        string userId,
        string displayName,
        string keyHash,
        bool isEnabled,
        DateTimeOffset createdAt,
        Guid? technicalId = null)
        : base(createdAt, technicalId ?? Guid.CreateVersion7())
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyHash);

        ApiKeyKey = id;
        UserId = userId;
        DisplayName = displayName.Trim();
        KeyHash = keyHash;
        IsEnabled = isEnabled;
    }

    public string ApiKeyKey { get; }

    public string UserId { get; }

    public string DisplayName { get; }

    public string KeyHash { get; }

    public bool IsEnabled { get; }

}
