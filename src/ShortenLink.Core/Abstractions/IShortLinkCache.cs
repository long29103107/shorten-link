using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Abstractions;

public interface IShortLinkCache
{
    Task<ShortLink?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task SetAsync(ShortLink shortLink, CancellationToken cancellationToken = default);

    Task RemoveAsync(string code, CancellationToken cancellationToken = default);
}
