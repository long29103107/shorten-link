using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Domain;
using ShortenLink.Core.Repositories;
using ShortenLink.Infrastructure.Persistence;

namespace ShortenLink.Infrastructure.Repositories;

public sealed class EfCoreShortLinkRepository : IShortLinkRepository
{
    private readonly ShortLinkDbContext dbContext;

    public EfCoreShortLinkRepository(ShortLinkDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ShortLink?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.ShortLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(link => link.Code == code, cancellationToken)
            .ConfigureAwait(false);

        return record?.ToDomain();
    }

    public Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        dbContext.ShortLinks.AnyAsync(link => link.Code == code, cancellationToken);

    public async Task AddAsync(
        ShortLink shortLink,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shortLink);

        dbContext.ShortLinks.Add(ShortLinkRecord.FromDomain(shortLink));
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        ShortLink shortLink,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shortLink);

        var record = await dbContext.ShortLinks
            .FirstOrDefaultAsync(link => link.Code == shortLink.Code, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            dbContext.ShortLinks.Add(ShortLinkRecord.FromDomain(shortLink));
        }
        else
        {
            record.UpdateFromDomain(shortLink);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
