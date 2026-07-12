using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Services;

public interface IShortLinkCache
{
    Task<ShortLink?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task SetAsync(ShortLink shortLink, CancellationToken cancellationToken = default);

    Task RemoveAsync(string code, CancellationToken cancellationToken = default);
}
