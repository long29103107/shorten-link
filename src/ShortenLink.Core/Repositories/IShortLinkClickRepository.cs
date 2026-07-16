using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Repositories;

public interface IShortLinkClickRepository
{
    Task AddAsync(ShortLinkClick shortLinkClick, CancellationToken cancellationToken = default);

    Task<ShortLinkClickSummary> GetSummaryAsync(
        string shortCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShortLinkClick>> ListRecentAsync(
        string shortCode,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed record ShortLinkClickSummary(
    string ShortCode,
    long ClickCount,
    DateTimeOffset? LastClickedAtUtc);
