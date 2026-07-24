namespace ShortenLink.Core.Contracts.Results;

public sealed record DeactivateShortLinkResult(
    bool Succeeded,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static DeactivateShortLinkResult Success() => new(true, null, null);

    public static DeactivateShortLinkResult Failure(string errorCode, string errorMessage) =>
        new(false, errorCode, errorMessage);
}
