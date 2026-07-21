namespace ShortenLink.Core.Security;

public sealed class ShortenLinkCustomRole
{
    public ShortenLinkCustomRole(
        string id,
        string name,
        IReadOnlyList<string> permissions,
        bool isEnabled,
        DateTimeOffset createdAt)
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

        Id = id;
        Name = name.Trim();
        Permissions = normalizedPermissions;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

    public string Id { get; }

    public string Name { get; }

    public IReadOnlyList<string> Permissions { get; }

    public bool IsEnabled { get; }

    public DateTimeOffset CreatedAt { get; }

    private static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string> values) =>
        values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToList();

}
