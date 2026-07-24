using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Contracts.Results;

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
