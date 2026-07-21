namespace ShortenLink.Core.Security;

public sealed class ShortenLinkUserApiKey
{
    public ShortenLinkUserApiKey(
        string id,
        string userId,
        string displayName,
        string keyHash,
        bool isEnabled,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyHash);

        Id = id;
        UserId = userId;
        DisplayName = displayName.Trim();
        KeyHash = keyHash;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

    public string Id { get; }

    public string UserId { get; }

    public string DisplayName { get; }

    public string KeyHash { get; }

    public bool IsEnabled { get; }

    public DateTimeOffset CreatedAt { get; }

}
