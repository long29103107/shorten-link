using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Security;

public sealed class ShortenLinkCustomRoleEntity : BaseEntity
{
    public ShortenLinkCustomRoleEntity(
        string id,
        string name,
        IReadOnlyList<string> permissions,
        bool isEnabled,
        DateTimeOffset createdAt)
        : base(createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(permissions);

        var normalizedPermissions = NormalizeDistinct(permissions);
        foreach (var permission in normalizedPermissions)
        {
            if (!ShortenLinkPermissionCatalog.All.Contains(permission))
            {
                throw new ArgumentException($"Unknown permission '{permission}'.", nameof(permissions));
            }
        }

        RoleKey = id;
        Name = name.Trim();
        Permissions = normalizedPermissions;
        IsEnabled = isEnabled;
    }

    public string RoleKey { get; }

    public string Name { get; }

    public IReadOnlyList<string> Permissions { get; }

    public bool IsEnabled { get; }

    private static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string> values) =>
        values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToList();

}
