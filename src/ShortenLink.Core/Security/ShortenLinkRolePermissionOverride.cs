namespace ShortenLink.Core.Security;

public sealed record ShortenLinkRolePermissionOverride(string Permission, bool IsAllowed)
{
    public static ShortenLinkRolePermissionOverride Create(string permission, bool isAllowed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        var normalized = permission.Trim();
        if (!ShortenLinkPermissionCatalog.All.Contains(normalized))
        {
            throw new ArgumentException($"Unknown permission '{normalized}'.", nameof(permission));
        }

        return new ShortenLinkRolePermissionOverride(normalized, isAllowed);
    }
}
