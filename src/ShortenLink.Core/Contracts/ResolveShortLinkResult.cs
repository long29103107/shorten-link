using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Contracts;

public sealed record ResolveShortLinkResult(
    bool Succeeded,
    ShortLink? ShortLink,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ResolveShortLinkResult Success(ShortLink shortLink) =>
        new(true, shortLink, null, null);

    public static ResolveShortLinkResult Failure(string errorCode, string errorMessage) =>
        new(false, null, errorCode, errorMessage);
}
