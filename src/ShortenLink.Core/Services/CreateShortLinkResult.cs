using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Services;

public sealed record CreateShortLinkResult(
    bool Succeeded,
    ShortLink? ShortLink,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static CreateShortLinkResult Success(ShortLink shortLink) =>
        new(true, shortLink, null, null);

    public static CreateShortLinkResult Failure(string errorCode, string errorMessage) =>
        new(false, null, errorCode, errorMessage);
}
