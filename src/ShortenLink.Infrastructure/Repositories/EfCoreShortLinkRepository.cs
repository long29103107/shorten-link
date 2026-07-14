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

    public async Task<IReadOnlyList<ShortLink>> ListRecentAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 500);
        var records = await dbContext.ShortLinks
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records
            .OrderByDescending(link => link.CreatedAt)
            .ThenBy(link => link.Code, StringComparer.Ordinal)
            .Take(safeLimit)
            .Select(record => record.ToDomain())
            .ToList();
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

    public async Task DeleteAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var record = await dbContext.ShortLinks
            .FirstOrDefaultAsync(link => link.Code == code, cancellationToken)
            .ConfigureAwait(false);

        if (record is not null)
        {
            dbContext.ShortLinks.Remove(record);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
