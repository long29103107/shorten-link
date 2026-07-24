using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Security;
using ShortenLink.Infrastructure.Persistence;

namespace ShortenLink.Infrastructure.Repositories;

public sealed class EfCoreShortenLinkSecurityAssignmentRepository : IShortenLinkSecurityAssignmentRepository
{
    private readonly ShortLinkDbContext dbContext;

    public EfCoreShortenLinkSecurityAssignmentRepository(ShortLinkDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<ShortenLinkSecurityAssignment>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var records = await dbContext.SecurityAssignments
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records
            .OrderBy(assignment => assignment.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(assignment => assignment.CredentialKeyHash, StringComparer.Ordinal)
            .Select(record => record.ToDomain())
            .ToList();
    }

    public async Task<ShortenLinkSecurityAssignment?> FindByCredentialKeyHashAsync(
        string credentialKeyHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialKeyHash);

        var record = await dbContext.SecurityAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                assignment => assignment.CredentialKeyHash == credentialKeyHash,
                cancellationToken)
            .ConfigureAwait(false);

        return record?.ToDomain();
    }

    public async Task AddOrUpdateAsync(
        ShortenLinkSecurityAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        var record = await dbContext.SecurityAssignments
            .FirstOrDefaultAsync(
                candidate => candidate.CredentialKeyHash == assignment.CredentialKeyHash,
                cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
        {
            dbContext.SecurityAssignments.Add(ShortenLinkSecurityAssignmentPersistenceEntity.FromDomain(assignment));
        }
        else
        {
            record.UpdateFromDomain(assignment);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> DisableAsync(
        string credentialKeyHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialKeyHash);

        var record = await dbContext.SecurityAssignments
            .FirstOrDefaultAsync(
                assignment => assignment.CredentialKeyHash == credentialKeyHash,
                cancellationToken)
            .ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.IsEnabled = false;
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
