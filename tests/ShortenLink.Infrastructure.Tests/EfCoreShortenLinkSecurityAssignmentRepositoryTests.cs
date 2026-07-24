using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Security;
using ShortenLink.Infrastructure.Persistence;
using ShortenLink.Infrastructure.Repositories;
using Xunit;

namespace ShortenLink.Infrastructure.Tests;

public sealed class EfCoreShortenLinkSecurityAssignmentRepositoryTests
{
    [Fact]
    public async Task AddOrUpdateAsync_PersistsAndFindsAssignment()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        var createdAt = new DateTimeOffset(2026, 7, 16, 9, 0, 0, TimeSpan.Zero);

        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            HashCredential("owner-key"),
            "Local Owner",
            new[] { "Owner" },
            new[] { "short_links.import" },
            true,
            createdAt));

        var stored = await repository.FindByCredentialKeyHashAsync(HashCredential("owner-key"));

        Assert.NotNull(stored);
        Assert.Equal("Local Owner", stored.Name);
        Assert.True(stored.IsEnabled);
        Assert.Equal(createdAt, stored.CreatedAt);
        Assert.Equal(new[] { "Owner" }, stored.Roles);
        Assert.Equal(new[] { "short_links.import" }, stored.Permissions);
    }

    [Fact]
    public async Task AddOrUpdateAsync_UpdatesExistingAssignment()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        var credentialHash = HashCredential("editor-key");

        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            credentialHash,
            "Editor",
            new[] { "Editor" },
            Array.Empty<string>(),
            true,
            DateTimeOffset.UtcNow));
        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            credentialHash,
            "Disabled Editor",
            new[] { "Viewer" },
            new[] { "analytics.read" },
            false,
            DateTimeOffset.UtcNow));

        var stored = await repository.FindByCredentialKeyHashAsync(credentialHash);

        Assert.NotNull(stored);
        Assert.Equal("Disabled Editor", stored.Name);
        Assert.False(stored.IsEnabled);
        Assert.Equal(new[] { "Viewer" }, stored.Roles);
        Assert.Equal(new[] { "analytics.read" }, stored.Permissions);
    }

    [Fact]
    public async Task ListAsync_ReturnsAssignmentsOrderedByName()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();

        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            HashCredential("viewer-key"),
            "Viewer",
            new[] { "Viewer" },
            Array.Empty<string>(),
            true,
            DateTimeOffset.UtcNow));
        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            HashCredential("admin-key"),
            "Admin",
            new[] { "Admin" },
            Array.Empty<string>(),
            true,
            DateTimeOffset.UtcNow));

        var assignments = await repository.ListAsync();

        Assert.Collection(
            assignments,
            assignment => Assert.Equal("Admin", assignment.Name),
            assignment => Assert.Equal("Viewer", assignment.Name));
    }

    [Fact]
    public async Task DisableAsync_DisablesExistingAssignment()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        var credentialHash = HashCredential("disable-key");
        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            credentialHash,
            "Disable Me",
            new[] { "Owner" },
            Array.Empty<string>(),
            true,
            DateTimeOffset.UtcNow));

        var disabled = await repository.DisableAsync(credentialHash);
        var stored = await repository.FindByCredentialKeyHashAsync(credentialHash);

        Assert.True(disabled);
        Assert.NotNull(stored);
        Assert.False(stored.IsEnabled);
    }

    [Fact]
    public async Task DisableAsync_ReturnsFalseForMissingAssignment()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();

        var disabled = await repository.DisableAsync(HashCredential("missing-disable-key"));

        Assert.False(disabled);
    }

    [Fact]
    public async Task FindByCredentialKeyHashAsync_ReturnsNullForUnknownCredential()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();

        var stored = await repository.FindByCredentialKeyHashAsync(HashCredential("missing-key"));

        Assert.Null(stored);
    }

    [Fact]
    public async Task Schema_HasExpectedIndexes()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var indexes = await database.GetIndexNamesAsync();

        Assert.Contains("IX_shorten_link_security_assignments_IsEnabled", indexes);
        Assert.Contains("IX_shorten_link_security_assignments_CreatedAt", indexes);
    }

    [Fact]
    public async Task EnsureCreated_CreatesUsableSecurityAssignmentsSchema()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();

        var repository = new EfCoreShortenLinkSecurityAssignmentRepository(context);
        var credentialHash = HashCredential("backfill-key");
        await repository.AddOrUpdateAsync(new ShortenLinkSecurityAssignment(
            credentialHash,
            "Bootstrap Admin",
            new[] { ShortenLinkSystemRoles.Admin },
            new[] { "audit_logs.read" },
            true,
            new DateTimeOffset(2026, 7, 16, 16, 0, 0, TimeSpan.Zero)));

        var stored = await repository.FindByCredentialKeyHashAsync(credentialHash);
        var indexes = await database.GetIndexNamesAsync();

        Assert.NotNull(stored);
        Assert.Equal("Bootstrap Admin", stored.Name);
        Assert.Contains("IX_shorten_link_security_assignments_IsEnabled", indexes);
        Assert.Contains("IX_shorten_link_security_assignments_CreatedAt", indexes);
    }

    [Fact]
    public void Model_WithPostgresProvider_KeepsExpectedIndexes()
    {
        var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=shorten_link_tests;Username=postgres;Password=postgres")
            .Options;

        using var context = new ShortLinkDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(ShortenLinkSecurityAssignmentPersistenceEntity));

        Assert.NotNull(entityType);

        var indexNames = entityType!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("IX_shorten_link_security_assignments_IsEnabled", indexNames);
        Assert.Contains("IX_shorten_link_security_assignments_CreatedAt", indexNames);
    }

    private static string HashCredential(string apiKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(hash).ToLowerInvariant();
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

        public EfCoreShortenLinkSecurityAssignmentRepository CreateRepository() =>
            new(CreateContext());

        public async Task<HashSet<string>> GetIndexNamesAsync()
        {
            var indexes = new HashSet<string>(StringComparer.Ordinal);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'shorten_link_security_assignments'";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                indexes.Add(reader.GetString(0));
            }

            return indexes;
        }

        public async ValueTask DisposeAsync()
        {
            await connection.DisposeAsync();
        }
    }
}
