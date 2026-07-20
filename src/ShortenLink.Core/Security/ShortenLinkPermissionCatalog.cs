namespace ShortenLink.Core.Security;

public static class ShortenLinkPermissionCatalog
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
    public const string SecurityAssignmentsManage = "security.assignments.manage";

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
        AuditLogsRead,
        SecurityAssignmentsManage
    };
}
