namespace ShortenLink.Core.Contracts;

public sealed record ShortLinkClickSummary(
    string ShortCode,
    long ClickCount,
    DateTimeOffset? LastClickedAtUtc);
