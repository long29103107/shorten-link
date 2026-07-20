using ShortenLink.Core.Security;

namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortenLinkUserApiKeyRecord
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string KeyHash { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static ShortenLinkUserApiKeyRecord FromDomain(ShortenLinkUserApiKey apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey);

        return new ShortenLinkUserApiKeyRecord
        {
            Id = apiKey.Id,
            UserId = apiKey.UserId,
            DisplayName = apiKey.DisplayName,
            KeyHash = apiKey.KeyHash,
            IsEnabled = apiKey.IsEnabled,
            CreatedAt = apiKey.CreatedAt
        };
    }

    public ShortenLinkUserApiKey ToDomain() =>
        new(
            Id,
            UserId,
            DisplayName,
            KeyHash,
            IsEnabled,
            CreatedAt);

    public void UpdateFromDomain(ShortenLinkUserApiKey apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey);

        UserId = apiKey.UserId;
        DisplayName = apiKey.DisplayName;
        KeyHash = apiKey.KeyHash;
        IsEnabled = apiKey.IsEnabled;
        CreatedAt = apiKey.CreatedAt;
    }
}
