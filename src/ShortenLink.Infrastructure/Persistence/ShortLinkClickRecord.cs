using ShortenLink.Core.Domain;

namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortLinkClickRecord
{
    public long Id { get; set; }

    public string ShortCode { get; set; } = string.Empty;

    public DateTimeOffset ClickedAtUtc { get; set; }

    public string? RemoteIpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? Referrer { get; set; }

    public static ShortLinkClickRecord FromDomain(ShortLinkClick shortLinkClick)
    {
        ArgumentNullException.ThrowIfNull(shortLinkClick);

        return new ShortLinkClickRecord
        {
            ShortCode = shortLinkClick.ShortCode,
            ClickedAtUtc = shortLinkClick.ClickedAtUtc,
            RemoteIpAddress = shortLinkClick.RemoteIpAddress,
            UserAgent = shortLinkClick.UserAgent,
            Referrer = shortLinkClick.Referrer
        };
    }

    public ShortLinkClick ToDomain() =>
        new(ShortCode, ClickedAtUtc, RemoteIpAddress, UserAgent, Referrer);
}
