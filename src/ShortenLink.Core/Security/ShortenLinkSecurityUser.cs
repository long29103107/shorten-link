namespace ShortenLink.Core.Security;

public sealed class ShortenLinkSecurityUser
{
    public ShortenLinkSecurityUser(
        string id,
        string username,
        string displayName,
        string passwordHash,
        IReadOnlyList<string> roleIds,
        bool isEnabled,
        bool isHidden,
        bool isBootstrap,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentNullException.ThrowIfNull(roleIds);

        Id = id;
        Username = username.Trim();
        DisplayName = displayName.Trim();
        PasswordHash = passwordHash;
        RoleIds = NormalizeDistinct(roleIds);
        IsEnabled = isEnabled;
        IsHidden = isHidden;
        IsBootstrap = isBootstrap;
        CreatedAt = createdAt;
    }

    public string Id { get; }

    public string Username { get; }

    public string DisplayName { get; }

    public string PasswordHash { get; }

    public IReadOnlyList<string> RoleIds { get; }

    public bool IsEnabled { get; }

    public bool IsHidden { get; }

    public bool IsBootstrap { get; }

    public DateTimeOffset CreatedAt { get; }

    private static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string> values) =>
        values
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
