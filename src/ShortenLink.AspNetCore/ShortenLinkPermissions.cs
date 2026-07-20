using ShortenLink.Core.Security;

namespace ShortenLink.AspNetCore;

public static class ShortenLinkPermissions
{
    public const string ShortLinksRead = ShortenLinkPermissionCatalog.ShortLinksRead;
    public const string ShortLinksCreate = ShortenLinkPermissionCatalog.ShortLinksCreate;
    public const string ShortLinksUpdate = ShortenLinkPermissionCatalog.ShortLinksUpdate;
    public const string ShortLinksActivate = ShortenLinkPermissionCatalog.ShortLinksActivate;
    public const string ShortLinksDeactivate = ShortenLinkPermissionCatalog.ShortLinksDeactivate;
    public const string ShortLinksDelete = ShortenLinkPermissionCatalog.ShortLinksDelete;
    public const string ShortLinksExport = ShortenLinkPermissionCatalog.ShortLinksExport;
    public const string AnalyticsRead = ShortenLinkPermissionCatalog.AnalyticsRead;
    public const string AuditLogsRead = ShortenLinkPermissionCatalog.AuditLogsRead;
    public const string SecurityAssignmentsManage = ShortenLinkPermissionCatalog.SecurityAssignmentsManage;

    public static IReadOnlySet<string> All => ShortenLinkPermissionCatalog.All;
}

public static class ShortenLinkRoles
{
    public const string Owner = ShortenLinkSystemRoles.Owner;
    public const string Admin = ShortenLinkSystemRoles.Admin;
    public const string Editor = ShortenLinkSystemRoles.Editor;
    public const string Viewer = ShortenLinkSystemRoles.Viewer;

    public static IReadOnlyDictionary<string, IReadOnlySet<string>> PermissionBundles =>
        ShortenLinkSystemRoles.PermissionBundles;
}
