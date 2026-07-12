using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Repositories;

public interface IShortLinkClickRepository
{
    Task AddAsync(ShortLinkClick shortLinkClick, CancellationToken cancellationToken = default);
}
