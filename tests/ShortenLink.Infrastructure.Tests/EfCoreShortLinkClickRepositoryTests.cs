using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Domain;
using ShortenLink.Infrastructure.Persistence;
using ShortenLink.Infrastructure.Repositories;
using Xunit;

namespace ShortenLink.Infrastructure.Tests;

public sealed class EfCoreShortLinkClickRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsShortLinkClick()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var repository = database.CreateRepository();
        var clickedAtUtc = new DateTimeOffset(2026, 7, 12, 8, 30, 0, TimeSpan.Zero);

        await repository.AddAsync(new ShortLinkClick(
            "jump01",
            clickedAtUtc,
            "127.0.0.1",
            "integration-test-agent",
            "https://example.com/from"));

        var stored = await database.GetClicksAsync();
        var click = Assert.Single(stored);
        Assert.Equal("jump01", click.ShortCode);
        Assert.Equal(clickedAtUtc, click.ClickedAtUtc);
        Assert.Equal("127.0.0.1", click.RemoteIpAddress);
        Assert.Equal("integration-test-agent", click.UserAgent);
        Assert.Equal("https://example.com/from", click.Referrer);
    }

    [Fact]
    public async Task Schema_HasExpectedIndexes()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        var indexes = await database.GetIndexNamesAsync();

        Assert.Contains("IX_short_link_clicks_ShortCode", indexes);
        Assert.Contains("IX_short_link_clicks_ClickedAtUtc", indexes);
        Assert.Contains("IX_short_link_clicks_ShortCode_ClickedAtUtc", indexes);
    }

    [Fact]
    public void Model_WithPostgresProvider_KeepsExpectedIndexes()
    {
        var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=shorten_link_tests;Username=postgres;Password=postgres")
            .Options;

        using var context = new ShortLinkDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(ShortLinkClickRecord));

        Assert.NotNull(entityType);

        var indexNames = entityType!
            .GetIndexes()
            .Select(index => index.GetDatabaseName())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("IX_short_link_clicks_ShortCode", indexNames);
        Assert.Contains("IX_short_link_clicks_ClickedAtUtc", indexNames);
        Assert.Contains("IX_short_link_clicks_ShortCode_ClickedAtUtc", indexNames);
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

        public EfCoreShortLinkClickRepository CreateRepository() =>
            new(CreateContext());

        public async Task<List<ShortLinkClickRecord>> GetClicksAsync()
        {
            await using var context = CreateContext();
            return await context.ShortLinkClicks
                .AsNoTracking()
                .OrderBy(click => click.Id)
                .ToListAsync();
        }

        public async Task<HashSet<string>> GetIndexNamesAsync()
        {
            var indexes = new HashSet<string>(StringComparer.Ordinal);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'short_link_clicks'";

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
