using ShortenLink.Core.Domain;
using ShortenLink.Core.Generation;
using ShortenLink.Core.Repositories;
using ShortenLink.Core.Services;
using Xunit;

namespace ShortenLink.Core.Tests;

public sealed class ShortLinkServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsInvalidUrl()
    {
        var service = CreateService();

        var result = await service.CreateAsync(new CreateShortLinkRequest("ftp://example.com/file"));

        Assert.False(result.Succeeded);
        Assert.Equal(ShortLinkErrorCodes.InvalidUrl, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_RejectsInvalidAlias()
    {
        var service = CreateService();

        var result = await service.CreateAsync(new CreateShortLinkRequest("https://example.com", "bad alias"));

        Assert.False(result.Succeeded);
        Assert.Equal(ShortLinkErrorCodes.InvalidAlias, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateCustomAlias()
    {
        var repository = new InMemoryShortLinkRepository();
        await repository.AddAsync(new ShortLink("taken", new Uri("https://example.com"), DateTimeOffset.UtcNow));
        var service = CreateService(repository);

        var result = await service.CreateAsync(new CreateShortLinkRequest("https://openai.com", "taken"));

        Assert.False(result.Succeeded);
        Assert.Equal(ShortLinkErrorCodes.DuplicateAlias, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_UsesCustomAliasWhenValid()
    {
        var repository = new InMemoryShortLinkRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(new CreateShortLinkRequest("https://example.com/path", "docs_1"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ShortLink);
        Assert.Equal("docs_1", result.ShortLink.Code);
        Assert.True(await repository.ExistsByCodeAsync("docs_1"));
    }

    [Fact]
    public async Task CreateAsync_GeneratesUniqueDefaultCode()
    {
        var repository = new InMemoryShortLinkRepository();
        await repository.AddAsync(new ShortLink("taken01", new Uri("https://example.com"), DateTimeOffset.UtcNow));
        var service = CreateService(repository, new SequenceCodeGenerator("taken01", "fresh01"));

        var result = await service.CreateAsync(new CreateShortLinkRequest("https://openai.com"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ShortLink);
        Assert.Equal("fresh01", result.ShortLink.Code);
    }

    [Fact]
    public async Task ResolveAsync_RejectsExpiredLink()
    {
        var now = new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero);
        var repository = new InMemoryShortLinkRepository();
        await repository.AddAsync(new ShortLink("expired", new Uri("https://example.com"), now.AddDays(-2), now.AddDays(-1)));
        var service = CreateService(repository, timeProvider: new FixedTimeProvider(now));

        var result = await service.ResolveAsync("expired");

        Assert.False(result.Succeeded);
        Assert.Equal(ShortLinkErrorCodes.Expired, result.ErrorCode);
    }

    [Fact]
    public async Task DeactivateAsync_MarksLinkInactive()
    {
        var repository = new InMemoryShortLinkRepository();
        await repository.AddAsync(new ShortLink("docs", new Uri("https://example.com"), DateTimeOffset.UtcNow));
        var service = CreateService(repository);

        var result = await service.DeactivateAsync("docs");
        var resolveResult = await service.ResolveAsync("docs");

        Assert.True(result.Succeeded);
        Assert.False(resolveResult.Succeeded);
        Assert.Equal(ShortLinkErrorCodes.Inactive, resolveResult.ErrorCode);
    }

    [Fact]
    public async Task ResolveAsync_UsesCacheBeforeRepository()
    {
        var repository = new InMemoryShortLinkRepository();
        var cache = new InMemoryShortLinkCache();
        await cache.SetAsync(new ShortLink("cached1", new Uri("https://example.com/cached"), DateTimeOffset.UtcNow));
        var service = CreateService(repository, cache: cache);

        var result = await service.ResolveAsync("cached1");

        Assert.True(result.Succeeded);
        Assert.Equal("https://example.com/cached", result.ShortLink?.OriginalUrl.AbsoluteUri.TrimEnd('/'));
        Assert.Equal(0, repository.FindByCodeCallCount);
    }

    [Fact]
    public async Task ResolveAsync_CachesSuccessfulRepositoryLookup()
    {
        var repository = new InMemoryShortLinkRepository();
        var cache = new InMemoryShortLinkCache();
        await repository.AddAsync(new ShortLink("cacheme", new Uri("https://example.com/db"), DateTimeOffset.UtcNow));
        var service = CreateService(repository, cache: cache);

        var first = await service.ResolveAsync("cacheme");
        var second = await service.ResolveAsync("cacheme");

        Assert.True(first.Succeeded);
        Assert.True(second.Succeeded);
        Assert.Equal(1, repository.FindByCodeCallCount);
        Assert.NotNull(await cache.FindByCodeAsync("cacheme"));
    }

    [Fact]
    public async Task DeactivateAsync_RemovesCachedLink()
    {
        var repository = new InMemoryShortLinkRepository();
        var cache = new InMemoryShortLinkCache();
        var shortLink = new ShortLink("remove1", new Uri("https://example.com/remove"), DateTimeOffset.UtcNow);
        await repository.AddAsync(shortLink);
        await cache.SetAsync(shortLink);
        var service = CreateService(repository, cache: cache);

        var result = await service.DeactivateAsync("remove1");

        Assert.True(result.Succeeded);
        Assert.Null(await cache.FindByCodeAsync("remove1"));
    }

    private static ShortLinkService CreateService(
        InMemoryShortLinkRepository? repository = null,
        IShortCodeGenerator? generator = null,
        IShortLinkCache? cache = null,
        TimeProvider? timeProvider = null)
    {
        return new ShortLinkService(
            repository ?? new InMemoryShortLinkRepository(),
            generator ?? new SequenceCodeGenerator("abc1234"),
            cache,
            timeProvider);
    }

    private sealed class InMemoryShortLinkRepository : IShortLinkRepository
    {
        private readonly Dictionary<string, ShortLink> links = new(StringComparer.Ordinal);

        public int FindByCodeCallCount { get; private set; }

        public Task<ShortLink?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            FindByCodeCallCount++;
            links.TryGetValue(code, out var shortLink);
            return Task.FromResult(shortLink);
        }

        public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(links.ContainsKey(code));

        public Task AddAsync(ShortLink shortLink, CancellationToken cancellationToken = default)
        {
            links.Add(shortLink.Code, shortLink);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ShortLink shortLink, CancellationToken cancellationToken = default)
        {
            links[shortLink.Code] = shortLink;
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryShortLinkCache : IShortLinkCache
    {
        private readonly Dictionary<string, ShortLink> links = new(StringComparer.Ordinal);

        public Task<ShortLink?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            links.TryGetValue(code, out var shortLink);
            return Task.FromResult(shortLink);
        }

        public Task SetAsync(ShortLink shortLink, CancellationToken cancellationToken = default)
        {
            links[shortLink.Code] = shortLink;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string code, CancellationToken cancellationToken = default)
        {
            links.Remove(code);
            return Task.CompletedTask;
        }
    }

    private sealed class SequenceCodeGenerator : IShortCodeGenerator
    {
        private readonly Queue<string> codes;

        public SequenceCodeGenerator(params string[] codes)
        {
            this.codes = new Queue<string>(codes);
        }

        public string Generate(int length = Base62ShortCodeGenerator.DefaultCodeLength) =>
            codes.Count > 0 ? codes.Dequeue() : new string('a', length);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset now;

        public FixedTimeProvider(DateTimeOffset now)
        {
            this.now = now;
        }

        public override DateTimeOffset GetUtcNow() => now;
    }
}
