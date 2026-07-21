using ShortenLink.Core.Domain;

namespace ShortenLink.Core.Security;

public sealed class ShortenLinkSecurityAssignmentEntity : BaseEntity
{
    public ShortenLinkSecurityAssignmentEntity(
        string credentialKeyHash,
        string name,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        bool isEnabled,
        DateTimeOffset createdAt)
        : base(createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialKeyHash);

        CredentialKeyHash = credentialKeyHash;
        Name = name;
        Roles = roles;
        Permissions = permissions;
        IsEnabled = isEnabled;
    }

    public string CredentialKeyHash { get; }

    public string Name { get; }

    public IReadOnlyList<string> Roles { get; }

    public IReadOnlyList<string> Permissions { get; }

    public bool IsEnabled { get; }

}
