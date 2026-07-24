namespace ShortenLink.Core.Security;

public enum ShortLinkShareAccess
{
    View = 1,
    Edit = 2
}

public sealed record ShortLinkShare(
    string ShortCode,
    string UserId,
    ShortLinkShareAccess Access,
    string CreatedByUserId,
    DateTimeOffset CreatedAt);
