namespace ShortenLink.Core.Security;

public static class ShortenLinkSystemRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string Viewer = "Viewer";

    public static IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionBundles { get; }
        = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Owner] = ShortenLinkPermissionCatalog.All,
            [Admin] = ShortenLinkPermissionCatalog.All,
            [Editor] = new HashSet<string>(StringComparer.Ordinal)
            {
                ShortenLinkPermissionCatalog.ShortLinksRead,
                ShortenLinkPermissionCatalog.ShortLinksCreate,
                ShortenLinkPermissionCatalog.ShortLinksUpdate,
                ShortenLinkPermissionCatalog.ShortLinksActivate,
                ShortenLinkPermissionCatalog.ShortLinksDeactivate
            },
            [Viewer] = new HashSet<string>(StringComparer.Ordinal)
            {
                ShortenLinkPermissionCatalog.ShortLinksRead,
                ShortenLinkPermissionCatalog.AnalyticsRead
            }
        };
}
