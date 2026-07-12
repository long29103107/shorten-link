namespace ShortenLink.Core.Domain;

public sealed class ShortLink
{
    public ShortLink(
        string code,
        Uri originalUrl,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt = null,
        bool isActive = true)
    {
        ShortLinkAliasValidator.ValidateCodeOrThrow(code);
        ArgumentNullException.ThrowIfNull(originalUrl);

        if (!ShortLinkUrlValidator.IsValid(originalUrl.AbsoluteUri))
        {
            throw new ArgumentException("Original URL must be an absolute HTTP or HTTPS URL.", nameof(originalUrl));
        }

        Code = code;
        OriginalUrl = originalUrl;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        IsActive = isActive;
    }

    public string Code { get; }

    public Uri OriginalUrl { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public bool IsActive { get; private set; }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt is not null && ExpiresAt <= now;

    public bool CanResolve(DateTimeOffset now) => IsActive && !IsExpired(now);

    public void Deactivate() => IsActive = false;
}
