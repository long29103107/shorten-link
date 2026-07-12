using ShortenLink.Core.Domain;
using ShortenLink.Core.Repositories;
using ShortenLink.Infrastructure.Persistence;

namespace ShortenLink.Infrastructure.Repositories;

public sealed class EfCoreShortLinkClickRepository : IShortLinkClickRepository
{
    private readonly ShortLinkDbContext dbContext;

    public EfCoreShortLinkClickRepository(ShortLinkDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(
        ShortLinkClick shortLinkClick,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shortLinkClick);

        dbContext.ShortLinkClicks.Add(ShortLinkClickRecord.FromDomain(shortLinkClick));
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
