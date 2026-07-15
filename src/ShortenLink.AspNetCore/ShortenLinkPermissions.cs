namespace ShortenLink.AspNetCore;

public static class ShortenLinkPermissions
{
    public const string ShortLinksRead = "short_links.read";
    public const string ShortLinksCreate = "short_links.create";
    public const string ShortLinksUpdate = "short_links.update";
    public const string ShortLinksActivate = "short_links.activate";
    public const string ShortLinksDeactivate = "short_links.deactivate";
    public const string ShortLinksDelete = "short_links.delete";
    public const string ShortLinksExport = "short_links.export";
    public const string AnalyticsRead = "analytics.read";
    public const string AuditLogsRead = "audit_logs.read";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        ShortLinksRead,
        ShortLinksCreate,
        ShortLinksUpdate,
        ShortLinksActivate,
        ShortLinksDeactivate,
        ShortLinksDelete,
        ShortLinksExport,
        AnalyticsRead,
        AuditLogsRead
    };
}

public static class ShortenLinkRoles
{
    public const string Owner = "Owner";
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string Viewer = "Viewer";

    public static IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionBundles { get; }
        = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Owner] = ShortenLinkPermissions.All,
            [Admin] = new HashSet<string>(StringComparer.Ordinal)
            {
                ShortenLinkPermissions.ShortLinksRead,
                ShortenLinkPermissions.ShortLinksCreate,
                ShortenLinkPermissions.ShortLinksUpdate,
                ShortenLinkPermissions.ShortLinksActivate,
                ShortenLinkPermissions.ShortLinksDeactivate,
                ShortenLinkPermissions.ShortLinksDelete,
                ShortenLinkPermissions.ShortLinksExport,
                ShortenLinkPermissions.AnalyticsRead,
                ShortenLinkPermissions.AuditLogsRead
            },
            [Editor] = new HashSet<string>(StringComparer.Ordinal)
            {
                ShortenLinkPermissions.ShortLinksRead,
                ShortenLinkPermissions.ShortLinksCreate,
                ShortenLinkPermissions.ShortLinksUpdate,
                ShortenLinkPermissions.ShortLinksActivate,
                ShortenLinkPermissions.ShortLinksDeactivate
            },
            [Viewer] = new HashSet<string>(StringComparer.Ordinal)
            {
                ShortenLinkPermissions.ShortLinksRead,
                ShortenLinkPermissions.AnalyticsRead
            }
        };
}
