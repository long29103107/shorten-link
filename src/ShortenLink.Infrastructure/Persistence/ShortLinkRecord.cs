using ShortenLink.Core.Domain;

namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortLinkRecord
{
    public string Code { get; set; } = string.Empty;

    public string OriginalUrl { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsActive { get; set; }

    public static ShortLinkRecord FromDomain(ShortLink shortLink)
    {
        ArgumentNullException.ThrowIfNull(shortLink);

        return new ShortLinkRecord
        {
            Code = shortLink.Code,
            OriginalUrl = shortLink.OriginalUrl.AbsoluteUri,
            CreatedAt = shortLink.CreatedAt,
            ExpiresAt = shortLink.ExpiresAt,
            IsActive = shortLink.IsActive
        };
    }

    public ShortLink ToDomain() =>
        new(Code, new Uri(OriginalUrl), CreatedAt, ExpiresAt, IsActive);

    public void UpdateFromDomain(ShortLink shortLink)
    {
        ArgumentNullException.ThrowIfNull(shortLink);

        OriginalUrl = shortLink.OriginalUrl.AbsoluteUri;
        CreatedAt = shortLink.CreatedAt;
        ExpiresAt = shortLink.ExpiresAt;
        IsActive = shortLink.IsActive;
    }
}
