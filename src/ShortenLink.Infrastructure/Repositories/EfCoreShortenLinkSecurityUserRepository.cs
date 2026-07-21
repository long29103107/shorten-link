using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Repositories;
using ShortenLink.Core.Security;
using ShortenLink.Infrastructure.Persistence;

namespace ShortenLink.Infrastructure.Repositories;

public sealed class EfCoreShortenLinkSecurityUserRepository : IShortenLinkSecurityUserRepository
{
    public const string BootstrapAdminUserId = "bootstrap-admin";
    public const string BootstrapAdminUsername = "admin";

    private readonly ShortLinkDbContext dbContext;

    public EfCoreShortenLinkSecurityUserRepository(ShortLinkDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<ShortenLinkSecurityUser>> ListAsync(
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.SecurityUsers.AsNoTracking();
        if (!includeHidden)
        {
            query = query.Where(user => !user.IsHidden);
        }

        var records = await query
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records
            .OrderBy(user => user.Username, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.Id, StringComparer.Ordinal)
            .Select(user => user.ToDomain())
            .ToList();
    }

    public async Task<ShortenLinkSecurityUser?> FindByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var record = await dbContext.SecurityUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return record?.ToDomain();
    }

    public async Task<ShortenLinkSecurityUser?> FindByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        var normalizedUsername = username.Trim();

        var record = await dbContext.SecurityUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Username == normalizedUsername, cancellationToken)
            .ConfigureAwait(false);

        return record?.ToDomain();
    }

    public async Task AddOrUpdateAsync(
        ShortenLinkSecurityUser user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var record = await dbContext.SecurityUsers
            .FirstOrDefaultAsync(candidate => candidate.Id == user.UserKey, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            dbContext.SecurityUsers.Add(ShortenLinkSecurityUserRecord.FromDomain(user));
        }
        else
        {
            record.UpdateFromDomain(user);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ShortenLinkSecurityUser> EnsureBootstrapAdminAsync(
        string passwordHash,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var existing = await dbContext.SecurityUsers
            .FirstOrDefaultAsync(user => user.Username == BootstrapAdminUsername, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            var user = new ShortenLinkSecurityUser(
                BootstrapAdminUserId,
                BootstrapAdminUsername,
                "Bootstrap Admin",
                passwordHash,
                new[] { ShortenLinkSystemRoles.Owner },
                isEnabled: true,
                isHidden: true,
                isBootstrap: true,
                createdAt);

            dbContext.SecurityUsers.Add(ShortenLinkSecurityUserRecord.FromDomain(user));
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return user;
        }

        existing.PasswordHash = string.IsNullOrWhiteSpace(existing.PasswordHash)
            ? passwordHash
            : existing.PasswordHash;
        existing.DisplayName = string.IsNullOrWhiteSpace(existing.DisplayName)
            ? "Bootstrap Admin"
            : existing.DisplayName;
        existing.RoleIdsJson = "[\"Owner\"]";
        existing.IsEnabled = true;
        existing.IsHidden = true;
        existing.IsBootstrap = true;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing.ToDomain();
    }

    public async Task<bool> DisableAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var record = await dbContext.SecurityUsers
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (record is null || record.IsBootstrap)
        {
            return false;
        }

        record.IsEnabled = false;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
