using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Domain;
using ShortenLink.Infrastructure.Persistence;
using ShortenLink.Infrastructure.Repositories;
using Xunit;

namespace ShortenLink.Infrastructure.Tests;

public sealed class EfCoreShortLinkRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsAndFindsShortLink()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        var createdAt = new DateTimeOffset(2026, 7, 11, 9, 30, 0, TimeSpan.Zero);
        var expiresAt = createdAt.AddDays(7);
        var shortLink = new ShortLink("abc1234", new Uri("https://example.com/docs"), createdAt, expiresAt);

        await repository.AddAsync(shortLink);

        var stored = await repository.FindByCodeAsync("abc1234");

        Assert.NotNull(stored);
        Assert.Equal("abc1234", stored.Code);
        Assert.Equal("https://example.com/docs", stored.OriginalUrl.AbsoluteUri.TrimEnd('/'));
        Assert.Equal(createdAt, stored.CreatedAt);
        Assert.Equal(expiresAt, stored.ExpiresAt);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task ExistsByCodeAsync_ReturnsTrueOnlyWhenCodeExists()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        await repository.AddAsync(new ShortLink("exists", new Uri("https://example.com"), DateTimeOffset.UtcNow));

        Assert.True(await repository.ExistsByCodeAsync("exists"));
        Assert.False(await repository.ExistsByCodeAsync("missing"));
    }

    [Fact]
    public async Task UpdateAsync_PersistsDeactivatedState()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        var shortLink = new ShortLink("active1", new Uri("https://example.com"), DateTimeOffset.UtcNow);
        await repository.AddAsync(shortLink);

        shortLink.Deactivate();
        await repository.UpdateAsync(shortLink);

        var stored = await repository.FindByCodeAsync("active1");

        Assert.NotNull(stored);
        Assert.False(stored.IsActive);
    }

    [Fact]
    public async Task AddAsync_EnforcesUniqueCode()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        await repository.AddAsync(new ShortLink("dupe", new Uri("https://example.com/one"), DateTimeOffset.UtcNow));
        var secondRepository = database.CreateRepository();

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            secondRepository.AddAsync(new ShortLink("dupe", new Uri("https://example.com/two"), DateTimeOffset.UtcNow)));
    }

    [Fact]
    public async Task Schema_HasExpectedIndexes()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var indexes = await database.GetIndexNamesAsync();

        Assert.Contains("IX_short_links_Code", indexes);
        Assert.Contains("IX_short_links_CreatedAt", indexes);
        Assert.Contains("IX_short_links_ExpiresAt", indexes);
        Assert.Contains("IX_short_links_IsActive", indexes);
    }

    [Fact]
    public void Model_WithPostgresProvider_KeepsExpectedIndexes()
    {
        var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=shorten_link_tests;Username=postgres;Password=postgres")
            .Options;

        using var context = new ShortLinkDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(ShortLinkRecord));

        Assert.NotNull(entityType);

        var indexNames = entityType!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("IX_short_links_Code", indexNames);
        Assert.Contains("IX_short_links_CreatedAt", indexNames);
        Assert.Contains("IX_short_links_ExpiresAt", indexNames);
        Assert.Contains("IX_short_links_IsActive", indexNames);
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

        public ShortLinkDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
                .UseSqlite(connection)
                .Options;

            return new ShortLinkDbContext(options);
        }

        public EfCoreShortLinkRepository CreateRepository() =>
            new(CreateContext());

        public async Task<HashSet<string>> GetIndexNamesAsync()
        {
            var indexes = new HashSet<string>(StringComparer.Ordinal);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'short_links'";

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
