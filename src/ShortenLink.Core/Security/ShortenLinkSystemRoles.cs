namespace ShortenLink.Core.Security;

public static class ShortenLinkSystemRoles
{
    public const string Admin = "Admin";
    public const string User = "User";

    public const string Owner = Admin;

    public const string Editor = User;

    public const string Viewer = User;

    public static IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionBundles { get; }
        = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Admin] = ShortenLinkPermissionCatalog.All,
            [User] = new HashSet<string>(StringComparer.Ordinal)
            {
                ShortenLinkPermissionCatalog.ShortLinksRead,
                ShortenLinkPermissionCatalog.ShortLinksCreate,
                ShortenLinkPermissionCatalog.ShortLinksUpdate,
                ShortenLinkPermissionCatalog.ShortLinksStatus,
                ShortenLinkPermissionCatalog.ShortLinksDelete,
                ShortenLinkPermissionCatalog.ShortLinksImport,
                ShortenLinkPermissionCatalog.AnalyticsRead,
                ShortenLinkPermissionCatalog.AuditLogsRead
            }
        };
}
