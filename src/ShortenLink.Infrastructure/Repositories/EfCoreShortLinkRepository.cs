using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Domain;
using ShortenLink.Core.Repositories;
using ShortenLink.Core.Services;
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
        DateTimeOffset? beforeCreatedAt = null,
        string? beforeCode = null,
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
            .Where(link => IsAfterCursor(link, beforeCreatedAt, beforeCode))
            .Take(safeLimit)
            .Select(record => record.ToDomain())
            .ToList();
    }

    private static bool IsAfterCursor(
        ShortLinkRecord link,
        DateTimeOffset? beforeCreatedAt,
        string? beforeCode)
    {
        if (beforeCreatedAt is null)
        {
            return true;
        }

        if (link.CreatedAt < beforeCreatedAt)
        {
            return true;
        }

        return link.CreatedAt == beforeCreatedAt
            && !string.IsNullOrWhiteSpace(beforeCode)
            && string.Compare(link.Code, beforeCode, StringComparison.Ordinal) > 0;
    }

    public async Task<IReadOnlyList<ShortLink>> ListRecentPageAsync(
        int skip,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var safeSkip = Math.Max(skip, 0);
        var safeLimit = Math.Clamp(limit, 1, 500);
        var records = await dbContext.ShortLinks
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records
            .OrderByDescending(link => link.CreatedAt)
            .ThenBy(link => link.Code, StringComparer.Ordinal)
            .Skip(safeSkip)
            .Take(safeLimit)
            .Select(record => record.ToDomain())
            .ToList();
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        dbContext.ShortLinks.CountAsync(cancellationToken);

    public async Task<ShortLinkListPage> ListPageAsync(
        int skip,
        int limit,
        ShortLinkListQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var safeSkip = Math.Max(skip, 0);
        var safeLimit = Math.Clamp(limit, 1, 500);
        var records = await dbContext.ShortLinks
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var filtered = records
            .Where(record => MatchesSearch(record, query.Search))
            .Where(record => MatchesStatus(record, query))
            .ToList();
        var ordered = ApplySort(filtered, query)
            .Skip(safeSkip)
            .Take(safeLimit)
            .Select(record => record.ToDomain())
            .ToList();

        return new ShortLinkListPage(ordered, filtered.Count);
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

    private static bool MatchesSearch(ShortLinkRecord record, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return record.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
            || record.OriginalUrl.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesStatus(ShortLinkRecord record, ShortLinkListQuery query) =>
        query.Status switch
        {
            ShortLinkListStatus.Active => record.IsActive && !IsExpired(record, query.Now),
            ShortLinkListStatus.Inactive => !record.IsActive,
            ShortLinkListStatus.Expired => record.IsActive && IsExpired(record, query.Now),
            ShortLinkListStatus.ExpiringSoon => record.IsActive
                && !IsExpired(record, query.Now)
                && record.ExpiresAt is not null
                && record.ExpiresAt <= query.ExpiringSoonBefore,
            _ => true
        };

    private static IEnumerable<ShortLinkRecord> ApplySort(
        IEnumerable<ShortLinkRecord> records,
        ShortLinkListQuery query)
    {
        return query.SortBy switch
        {
            ShortLinkListSortBy.Expiry => ApplyDirection(
                records,
                query.SortDirection,
                record => record.ExpiresAt ?? DateTimeOffset.MaxValue),
            ShortLinkListSortBy.Destination => ApplyDirection(
                records,
                query.SortDirection,
                record => record.OriginalUrl),
            ShortLinkListSortBy.Code => ApplyDirection(
                records,
                query.SortDirection,
                record => record.Code),
            ShortLinkListSortBy.Status => ApplyDirection(
                records,
                query.SortDirection,
                record => GetStatusRank(record, query.Now)),
            _ => ApplyDirection(
                records,
                query.SortDirection,
                record => record.CreatedAt)
        };
    }

    private static IEnumerable<ShortLinkRecord> ApplyDirection<TKey>(
        IEnumerable<ShortLinkRecord> records,
        ShortLinkSortDirection direction,
        Func<ShortLinkRecord, TKey> keySelector)
    {
        return direction == ShortLinkSortDirection.Asc
            ? records.OrderBy(keySelector).ThenBy(record => record.Code, StringComparer.Ordinal)
            : records.OrderByDescending(keySelector).ThenBy(record => record.Code, StringComparer.Ordinal);
    }

    private static bool IsExpired(ShortLinkRecord record, DateTimeOffset now) =>
        record.ExpiresAt is not null && record.ExpiresAt <= now;

    private static int GetStatusRank(ShortLinkRecord record, DateTimeOffset now)
    {
        if (!record.IsActive)
        {
            return 2;
        }

        return IsExpired(record, now) ? 1 : 0;
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
