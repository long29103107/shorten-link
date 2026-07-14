namespace ShortenLink.Core.Services;

public sealed record UpdateShortLinkRequest(
    string OriginalUrl,
    DateTimeOffset? ExpiresAt = null);
