namespace ShortenLink.Core.Security;

public static class ShortenLinkPermissionCatalog
{
    public const string ShortLinksRead = "short_links.read";
    public const string ShortLinksCreate = "short_links.create";
    public const string ShortLinksUpdate = "short_links.update";
    public const string ShortLinksStatus = "short_links.status";
    public const string ShortLinksDelete = "short_links.delete";
    public const string ShortLinksImport = "short_links.import";
    public const string AnalyticsRead = "analytics.read";
    public const string AuditLogsRead = "audit_logs.read";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        ShortLinksRead,
        ShortLinksCreate,
        ShortLinksUpdate,
        ShortLinksStatus,
        ShortLinksDelete,
        ShortLinksImport,
        AnalyticsRead,
        AuditLogsRead
    };
}
