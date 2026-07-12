using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Services;

public sealed class DisabledShortLinkCache : IShortLinkCache
{
    public Task<ShortLink?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult<ShortLink?>(null);

    public Task SetAsync(ShortLink shortLink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shortLink);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string code, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
