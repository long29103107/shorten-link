namespace ShortenLink.Core.Services;

public sealed record RecordShortLinkClickRequest(
    string ShortCode,
    DateTimeOffset ClickedAtUtc,
    string? RemoteIpAddress,
    string? UserAgent,
    string? Referrer);
