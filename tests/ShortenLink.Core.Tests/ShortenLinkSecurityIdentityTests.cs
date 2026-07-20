using ShortenLink.Core.Security;
using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class ShortenLinkSecurityIdentityTests
{
    [Fact]
    public void SystemRoles_AreBuiltInPermissionBundles()
    {
        var roles = ShortenLinkSystemRoles.PermissionBundles;

        Assert.Contains(ShortenLinkSystemRoles.Owner, roles.Keys);
        Assert.Contains(ShortenLinkSystemRoles.Admin, roles.Keys);
        Assert.Contains(ShortenLinkSystemRoles.Editor, roles.Keys);
        Assert.Contains(ShortenLinkSystemRoles.Viewer, roles.Keys);
        Assert.Contains(ShortenLinkPermissionCatalog.ShortLinksRead, roles[ShortenLinkSystemRoles.Viewer]);
        Assert.Contains(ShortenLinkPermissionCatalog.AnalyticsRead, roles[ShortenLinkSystemRoles.Viewer]);
        Assert.DoesNotContain(ShortenLinkPermissionCatalog.ShortLinksDelete, roles[ShortenLinkSystemRoles.Viewer]);
    }

    [Fact]
    public void CustomRole_RejectsUnknownPermissions()
    {
        var exception = Assert.Throws<ArgumentException>(() => new ShortenLinkCustomRole(
            "support",
            "Support",
            new[] { "security.magic" },
            isEnabled: true,
            DateTimeOffset.UtcNow));

        Assert.Contains("Unknown permission", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CustomRole_NormalizesSupportedPermissions()
    {
        var role = new ShortenLinkCustomRole(
            "support",
            " Support ",
            new[]
            {
                ShortenLinkPermissionCatalog.AnalyticsRead,
                ShortenLinkPermissionCatalog.ShortLinksRead,
                ShortenLinkPermissionCatalog.ShortLinksRead
            },
            isEnabled: true,
            DateTimeOffset.UtcNow);

        Assert.Equal("Support", role.Name);
        Assert.Equal(
            new[] { ShortenLinkPermissionCatalog.AnalyticsRead, ShortenLinkPermissionCatalog.ShortLinksRead },
            role.Permissions);
    }

    [Fact]
    public void CredentialHasher_DoesNotReturnRawSecrets()
    {
        var passwordHash = ShortenLinkSecurityCredentialHasher.HashPassword("admin");
        var apiKeyHash = ShortenLinkSecurityCredentialHasher.HashApiKey("local-api-key");

        Assert.DoesNotContain("admin", passwordHash, StringComparison.Ordinal);
        Assert.NotEqual("local-api-key", apiKeyHash);
        Assert.Equal(64, apiKeyHash.Length);
    }

    [Fact]
    public void CredentialHasher_VerifiesPasswordHash()
    {
        var passwordHash = ShortenLinkSecurityCredentialHasher.HashPassword("correct-password");

        Assert.True(ShortenLinkSecurityCredentialHasher.VerifyPassword("correct-password", passwordHash));
        Assert.False(ShortenLinkSecurityCredentialHasher.VerifyPassword("wrong-password", passwordHash));
        Assert.False(ShortenLinkSecurityCredentialHasher.VerifyPassword("correct-password", "not-a-hash"));
    }
}
