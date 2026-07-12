namespace ShortenLink.Core.Services;

public sealed record CreateShortLinkRequest(
    string OriginalUrl,
    string? CustomAlias = null,
    DateTimeOffset? ExpiresAt = null);
