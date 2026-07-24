using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShortenLink.Core.Security;
using ShortenLink.Infrastructure.Persistence;
using ShortenLink.Infrastructure.Repositories;
using Xunit;

namespace ShortenLink.Infrastructure.Tests;

public sealed class EfCoreShortLinkShareRepositoryTests
{
    [Fact]
    public async Task AddOrUpdateAsync_PersistsAndUpdatesPerUserAccess()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ShortLinkDbContext>()
            .UseSqlite(connection)
            .Options;
        await using (var setup = new ShortLinkDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
        }

        var repository = new EfCoreShortLinkShareRepository(new ShortLinkDbContext(options));
        var createdAt = new DateTimeOffset(2026, 7, 24, 3, 0, 0, TimeSpan.Zero);
        await repository.AddOrUpdateAsync(
            new ShortLinkShare("code123", "user-2", ShortLinkShareAccess.View, "user-1", createdAt));
        await repository.AddOrUpdateAsync(
            new ShortLinkShare("code123", "user-2", ShortLinkShareAccess.Edit, "user-1", createdAt.AddMinutes(1)));

        var stored = await repository.FindAsync("code123", "user-2");
        var access = await repository.ListSharedAccessAsync("user-2");

        Assert.NotNull(stored);
        Assert.Equal(ShortLinkShareAccess.Edit, stored.Access);
        Assert.Equal(ShortLinkShareAccess.Edit, access["code123"]);
    }
}
