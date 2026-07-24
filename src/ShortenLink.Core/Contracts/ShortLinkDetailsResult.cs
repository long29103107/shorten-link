using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Contracts;

public sealed record ShortLinkDetailsResult(
    bool Succeeded,
    ShortLink? ShortLink,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ShortLinkDetailsResult Success(ShortLink shortLink) =>
        new(true, shortLink, null, null);

    public static ShortLinkDetailsResult Failure(string errorCode, string errorMessage) =>
        new(false, null, errorCode, errorMessage);
}
