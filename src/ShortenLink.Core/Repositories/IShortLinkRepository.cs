using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Repositories;

public interface IShortLinkRepository
{
    Task<IReadOnlyList<ShortLink>> ListRecentAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<ShortLink?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(ShortLink shortLink, CancellationToken cancellationToken = default);

    Task UpdateAsync(ShortLink shortLink, CancellationToken cancellationToken = default);

    Task DeleteAsync(string code, CancellationToken cancellationToken = default);
}
