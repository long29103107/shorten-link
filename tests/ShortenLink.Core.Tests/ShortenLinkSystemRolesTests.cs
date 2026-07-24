using ShortenLink.Core.Security;
using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class ShortenLinkSystemRolesTests
{
    [Fact]
    public void PermissionBundles_ExposeOnlyAdminAndUserRoles()
    {
        Assert.Equal(
            new[] { ShortenLinkSystemRoles.Admin, ShortenLinkSystemRoles.User },
            ShortenLinkSystemRoles.PermissionBundles.Keys.OrderBy(value => value));
        Assert.Contains(
            ShortenLinkPermissionCatalog.AuditLogsRead,
            ShortenLinkSystemRoles.PermissionBundles[ShortenLinkSystemRoles.User]);
        Assert.Contains(
            ShortenLinkPermissionCatalog.ShortLinksStatus,
            ShortenLinkSystemRoles.PermissionBundles[ShortenLinkSystemRoles.User]);
        Assert.Contains(
            ShortenLinkPermissionCatalog.ShortLinksImport,
            ShortenLinkSystemRoles.PermissionBundles[ShortenLinkSystemRoles.User]);
        Assert.Contains(
            ShortenLinkPermissionCatalog.ShortLinksDelete,
            ShortenLinkSystemRoles.PermissionBundles[ShortenLinkSystemRoles.User]);
    }
}
