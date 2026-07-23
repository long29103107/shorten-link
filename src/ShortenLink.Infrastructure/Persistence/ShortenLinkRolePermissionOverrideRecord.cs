namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortenLinkRolePermissionOverrideRecord
{
    public string RoleId { get; set; } = string.Empty;

    public string Permission { get; set; } = string.Empty;

    public bool IsAllowed { get; set; }
}
