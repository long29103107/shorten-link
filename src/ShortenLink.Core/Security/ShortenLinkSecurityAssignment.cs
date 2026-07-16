namespace ShortenLink.Core.Security;

public sealed class ShortenLinkSecurityAssignment
{
    public ShortenLinkSecurityAssignment(
        string credentialKeyHash,
        string name,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        bool isEnabled,
        DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialKeyHash);

        CredentialKeyHash = credentialKeyHash;
        Name = name;
        Roles = roles;
        Permissions = permissions;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
    }

    public string CredentialKeyHash { get; }

    public string Name { get; }

    public IReadOnlyList<string> Roles { get; }

    public IReadOnlyList<string> Permissions { get; }

    public bool IsEnabled { get; }

    public DateTimeOffset CreatedAt { get; }
}
