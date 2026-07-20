using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Security;
using ShortenLink.Infrastructure.Persistence;
using ShortenLink.Infrastructure.Repositories;
using Xunit;

namespace ShortenLink.Infrastructure.Tests;

public sealed class EfCoreShortenLinkSecurityIdentityRepositoryTests
{
    [Fact]
    public async Task CustomRoleRepository_PersistsSupportedPermissionBundle()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var repository = new EfCoreShortenLinkSecurityRoleRepository(context);
        var createdAt = new DateTimeOffset(2026, 7, 17, 8, 0, 0, TimeSpan.Zero);

        await repository.AddOrUpdateCustomRoleAsync(new ShortenLinkCustomRole(
            "support",
            "Support",
            new[] { ShortenLinkPermissionCatalog.ShortLinksRead, ShortenLinkPermissionCatalog.AnalyticsRead },
            isEnabled: true,
            createdAt));

        var stored = await repository.FindCustomRoleAsync("support");
        var roles = await repository.ListCustomRolesAsync();

        Assert.NotNull(stored);
        Assert.Equal("Support", stored.Name);
        Assert.True(stored.IsEnabled);
        Assert.Equal(createdAt, stored.CreatedAt);
        Assert.Equal(
            new[] { ShortenLinkPermissionCatalog.AnalyticsRead, ShortenLinkPermissionCatalog.ShortLinksRead },
            stored.Permissions);
        Assert.Single(roles);
    }

    [Fact]
    public async Task CustomRoleRepository_DisablesExistingRole()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var repository = new EfCoreShortenLinkSecurityRoleRepository(context);
        await repository.AddOrUpdateCustomRoleAsync(new ShortenLinkCustomRole(
            "support",
            "Support",
            new[] { ShortenLinkPermissionCatalog.ShortLinksRead },
            isEnabled: true,
            DateTimeOffset.UtcNow));

        var disabled = await repository.DisableCustomRoleAsync("support");
        var stored = await repository.FindCustomRoleAsync("support");

        Assert.True(disabled);
        Assert.NotNull(stored);
        Assert.False(stored.IsEnabled);
    }

    [Fact]
    public async Task SecurityUserRepository_EnsuresHiddenBootstrapAdmin()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var repository = new EfCoreShortenLinkSecurityUserRepository(context);
        var passwordHash = ShortenLinkSecurityCredentialHasher.HashPassword("admin");

        var admin = await repository.EnsureBootstrapAdminAsync(
            passwordHash,
            new DateTimeOffset(2026, 7, 17, 8, 15, 0, TimeSpan.Zero));
        var hiddenUsers = await repository.ListAsync(includeHidden: true);
        var visibleUsers = await repository.ListAsync();

        Assert.Equal(EfCoreShortenLinkSecurityUserRepository.BootstrapAdminUsername, admin.Username);
        Assert.True(admin.IsBootstrap);
        Assert.True(admin.IsHidden);
        Assert.True(admin.IsEnabled);
        Assert.Equal(new[] { ShortenLinkSystemRoles.Owner }, admin.RoleIds);
        Assert.DoesNotContain("admin", admin.PasswordHash, StringComparison.Ordinal);
        Assert.Single(hiddenUsers);
        Assert.Empty(visibleUsers);
    }

    [Fact]
    public async Task SecurityUserRepository_PersistsNormalUserWithAssignedRoles()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var repository = new EfCoreShortenLinkSecurityUserRepository(context);
        var createdAt = new DateTimeOffset(2026, 7, 17, 9, 0, 0, TimeSpan.Zero);

        await repository.AddOrUpdateAsync(new ShortenLinkSecurityUser(
            "user-1",
            "editor",
            "Editor User",
            ShortenLinkSecurityCredentialHasher.HashPassword("editor-password"),
            new[] { ShortenLinkSystemRoles.Editor, "support" },
            isEnabled: true,
            isHidden: false,
            isBootstrap: false,
            createdAt));

        var stored = await repository.FindByUsernameAsync("editor");
        var visibleUsers = await repository.ListAsync();

        Assert.NotNull(stored);
        Assert.Equal("Editor User", stored.DisplayName);
        Assert.Equal(new[] { ShortenLinkSystemRoles.Editor, "support" }, stored.RoleIds);
        Assert.True(stored.IsEnabled);
        Assert.False(stored.IsHidden);
        Assert.False(stored.IsBootstrap);
        Assert.Single(visibleUsers);
    }

    [Fact]
    public async Task SecurityUserRepository_DisablesNormalUserButNotBootstrap()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var repository = new EfCoreShortenLinkSecurityUserRepository(context);
        await repository.EnsureBootstrapAdminAsync(
            ShortenLinkSecurityCredentialHasher.HashPassword("admin"),
            DateTimeOffset.UtcNow);
        await repository.AddOrUpdateAsync(new ShortenLinkSecurityUser(
            "user-1",
            "editor",
            "Editor User",
            ShortenLinkSecurityCredentialHasher.HashPassword("editor-password"),
            new[] { ShortenLinkSystemRoles.Editor },
            isEnabled: true,
            isHidden: false,
            isBootstrap: false,
            DateTimeOffset.UtcNow));

        var disabledUser = await repository.DisableAsync("user-1");
        var disabledBootstrap = await repository.DisableAsync(EfCoreShortenLinkSecurityUserRepository.BootstrapAdminUserId);
        var stored = await repository.FindByIdAsync("user-1");
        var bootstrap = await repository.FindByIdAsync(EfCoreShortenLinkSecurityUserRepository.BootstrapAdminUserId);

        Assert.True(disabledUser);
        Assert.False(disabledBootstrap);
        Assert.NotNull(stored);
        Assert.False(stored.IsEnabled);
        Assert.NotNull(bootstrap);
        Assert.True(bootstrap.IsEnabled);
    }

    [Fact]
    public async Task UserApiKeyRepository_PersistsHashOnlyCredential()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var repository = new EfCoreShortenLinkUserApiKeyRepository(context);
        var keyHash = ShortenLinkSecurityCredentialHasher.HashApiKey("raw-api-key");
        var createdAt = new DateTimeOffset(2026, 7, 17, 9, 30, 0, TimeSpan.Zero);

        await repository.AddOrUpdateAsync(new ShortenLinkUserApiKey(
            "key-1",
            "user-1",
            "Local automation",
            keyHash,
            isEnabled: true,
            createdAt));

        var stored = await repository.FindByKeyHashAsync(keyHash);
        var storedById = await repository.FindByIdAsync("key-1");
        var keys = await repository.ListByUserIdAsync("user-1");
        var disabled = await repository.DisableAsync("key-1");
        var disabledKey = await repository.FindByKeyHashAsync(keyHash);

        Assert.NotNull(stored);
        Assert.NotNull(storedById);
        Assert.Equal(stored.KeyHash, storedById.KeyHash);
        Assert.Equal("user-1", stored.UserId);
        Assert.Equal("Local automation", stored.DisplayName);
        Assert.Equal(keyHash, stored.KeyHash);
        Assert.DoesNotContain("raw-api-key", stored.KeyHash, StringComparison.Ordinal);
        Assert.Single(keys);
        Assert.True(disabled);
        Assert.NotNull(disabledKey);
        Assert.False(disabledKey.IsEnabled);
    }

    [Fact]
    public async Task SchemaInitializer_CreatesSecurityIdentityTablesWhenDatabaseAlreadyExists()
    {
        await using var database = await SqliteTestDatabase.CreateWithLegacySchemaAsync();
        await using var context = database.CreateContext();
        await context.Database.EnsureCreatedAsync();

        await context.EnsureSecurityIdentitySchemaAsync();

        var tables = await database.GetTableNamesAsync();

        Assert.Contains("shorten_link_security_custom_roles", tables);
        Assert.Contains("shorten_link_security_users", tables);
        Assert.Contains("shorten_link_security_user_api_keys", tables);
    }

    [Fact]
    public void Model_WithPostgresProvider_KeepsSecurityIdentityIndexes()
    {
        var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=shorten_link_tests;Username=postgres;Password=postgres")
            .Options;

        using var context = new ShortLinkDbContext(options);

        Assert.Contains(
            "IX_shorten_link_security_users_Username",
            GetIndexNames<ShortenLinkSecurityUserRecord>(context));
        Assert.Contains(
            "IX_shorten_link_security_user_api_keys_KeyHash",
            GetIndexNames<ShortenLinkUserApiKeyRecord>(context));
        Assert.Contains(
            "IX_shorten_link_security_custom_roles_Name",
            GetIndexNames<ShortenLinkCustomRoleRecord>(context));
    }

    private static HashSet<string> GetIndexNames<TRecord>(ShortLinkDbContext context)
    {
        var entityType = context.Model.FindEntityType(typeof(TRecord));
        Assert.NotNull(entityType);

        return entityType!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private sealed class SqliteTestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private SqliteTestDatabase(SqliteConnection connection)
        {
            this.connection = connection;
        }

        public static async Task<SqliteTestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var database = new SqliteTestDatabase(connection);
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();

            return database;
        }

        public static async Task<SqliteTestDatabase> CreateWithLegacySchemaAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE short_links (
                    Code TEXT NOT NULL PRIMARY KEY,
                    OriginalUrl TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ExpiresAt TEXT NULL,
                    IsActive INTEGER NOT NULL
                );
                """;
            await command.ExecuteNonQueryAsync();

            return new SqliteTestDatabase(connection);
        }

        public ShortLinkDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
                .UseSqlite(connection)
                .Options;

            return new ShortLinkDbContext(options);
        }

        public async Task<HashSet<string>> GetTableNamesAsync()
        {
            var tables = new HashSet<string>(StringComparer.Ordinal);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        public async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
        }
    }
}
