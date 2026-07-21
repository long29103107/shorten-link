using System.Text.Json;
using ShortenLink.Core.Security;

namespace ShortenLink.Infrastructure.Persistence;

public sealed class ShortenLinkCustomRoleRecord
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PermissionsJson { get; set; } = "[]";

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static ShortenLinkCustomRoleRecord FromDomain(ShortenLinkCustomRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return new ShortenLinkCustomRoleRecord
        {
            Id = role.RoleKey,
            Name = role.Name,
            PermissionsJson = JsonSerializer.Serialize(role.Permissions, SerializerOptions),
            IsEnabled = role.IsEnabled,
            CreatedAt = role.CreatedAt
        };
    }

    public ShortenLinkCustomRole ToDomain() =>
        new(
            Id,
            Name,
            DeserializeList(PermissionsJson),
            IsEnabled,
            CreatedAt);

    public void UpdateFromDomain(ShortenLinkCustomRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Name = role.Name;
        PermissionsJson = JsonSerializer.Serialize(role.Permissions, SerializerOptions);
        IsEnabled = role.IsEnabled;
        CreatedAt = role.CreatedAt;
    }

    private static IReadOnlyList<string> DeserializeList(string value) =>
        JsonSerializer.Deserialize<List<string>>(value, SerializerOptions) ?? new List<string>();
}
