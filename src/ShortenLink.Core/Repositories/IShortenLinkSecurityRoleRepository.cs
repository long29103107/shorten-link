using ShortenLink.Core.Security;

namespace ShortenLink.Core.Repositories;

public interface IShortenLinkSecurityRoleRepository
{
    Task<IReadOnlyList<ShortenLinkCustomRole>> ListCustomRolesAsync(
        CancellationToken cancellationToken = default);

    Task<ShortenLinkCustomRole?> FindCustomRoleAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task AddOrUpdateCustomRoleAsync(
        ShortenLinkCustomRole role,
        CancellationToken cancellationToken = default);

    Task<bool> DisableCustomRoleAsync(
        string id,
        CancellationToken cancellationToken = default);
}
