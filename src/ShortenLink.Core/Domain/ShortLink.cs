namespace ShortenLink.Core.Domain;

public sealed class ShortLinkEntity : BaseEntity
{
    public ShortLinkEntity(
        string code,
        Uri originalUrl,
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt = null,
        bool isActive = true)
        : base(createdAt)
    {
        ShortCodeValidator.ValidateCodeOrThrow(code);
        ArgumentNullException.ThrowIfNull(originalUrl);

        if (!ShortLinkUrlValidator.IsValid(originalUrl.AbsoluteUri))
        {
            throw new ArgumentException("Original URL must be an absolute HTTP or HTTPS URL.", nameof(originalUrl));
        }

        Code = code;
        OriginalUrl = originalUrl;
        ExpiresAt = expiresAt;
        IsActive = isActive;
    }

    public string Code { get; }

    public Uri OriginalUrl { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public bool IsActive { get; private set; }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt is not null && ExpiresAt <= now;

    public bool CanResolve(DateTimeOffset now) => IsActive && !IsExpired(now);

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
