using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Security;

public sealed class ShortenLinkUserApiKeyEntity : BaseEntity
{
    public ShortenLinkUserApiKeyEntity(
        string id,
        string userId,
        string displayName,
        string keyHash,
        bool isEnabled,
        DateTimeOffset createdAt)
        : base(createdAt)
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
