namespace ShortenLink.Core.Contracts.Results;

public sealed record ShortLinkClickSummary(
    string ShortCode,
    long ClickCount,
    DateTimeOffset? LastClickedAtUtc);
