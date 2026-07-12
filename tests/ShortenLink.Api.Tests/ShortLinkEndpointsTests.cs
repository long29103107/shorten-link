using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShortenLink.Api;
using ShortenLink.AspNetCore;
using ShortenLink.Core.Services;
using ShortenLink.Infrastructure.Persistence;
using Xunit;

namespace ShortenLink.Api.Tests;

public sealed class ShortLinkEndpointsTests
{
    [Fact]
    public async Task PostCreate_ReturnsCreatedShortLink()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/docs",
            customAlias = "docs_1",
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkCreatedResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("docs_1", payload.Code);
        Assert.Equal("https://sho.rt/docs_1", payload.ShortUrl);
        Assert.Equal("https://example.com/docs", payload.OriginalUrl);
        Assert.Equal(new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero), payload.CreatedAtUtc);
    }

    [Fact]
    public async Task PostCreate_ReturnsConflictForDuplicateAlias()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/one",
            customAlias = "taken"
        });

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/two",
            customAlias = "taken"
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("duplicate_alias", payload.ErrorCode);
    }

    [Fact]
    public async Task PostCreate_ReturnsBadRequestForInvalidUrl()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "ftp://example.com/file"
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("invalid_url", payload.ErrorCode);
    }

    [Fact]
    public async Task GetDetails_ReturnsStoredShortLink()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/details",
            customAlias = "detail01"
        });

        using var response = await client.GetAsync("/api/short-links/detail01");
        var payload = await response.Content.ReadFromJsonAsync<ShortLinkDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("detail01", payload.Code);
        Assert.Equal("https://example.com/details", payload.OriginalUrl);
        Assert.True(payload.IsActive);
    }

    [Fact]
    public async Task Delete_DeactivatesShortLinkAndRedirectReturnsGone()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/remove",
            customAlias = "remove1"
        });

        using var deleteResponse = await client.DeleteAsync("/api/short-links/remove1");
        using var redirectResponse = await client.GetAsync("/remove1");
        var payload = await redirectResponse.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Gone, redirectResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("inactive", payload.ErrorCode);
    }

    [Fact]
    public async Task Delete_InvalidatesCachedRedirect_WhenMemoryCacheEnabled()
    {
        await using var factory = new ShortLinkApiFactory(
            enableFrontendFallback: false,
            cacheEnabled: true,
            cacheProvider: "Memory");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/cached-remove",
            customAlias = "cached1"
        });

        using var firstRedirectResponse = await client.GetAsync("/cached1");
        using var deleteResponse = await client.DeleteAsync("/api/short-links/cached1");
        using var secondRedirectResponse = await client.GetAsync("/cached1");
        var payload = await secondRedirectResponse.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.Redirect, firstRedirectResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Gone, secondRedirectResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("inactive", payload.ErrorCode);
    }

    [Fact]
    public async Task Redirect_ReturnsOriginalUrlForActiveShortLink()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/redirect",
            customAlias = "jump01"
        });

        using var response = await client.GetAsync("/jump01");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("https://example.com/redirect", response.Headers.Location?.AbsoluteUri);
    }

    [Fact]
    public async Task Redirect_RecordsClickAnalytics_WhenEnabled()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false, analyticsEnabled: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/redirect",
            customAlias = "track01"
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, "/track01");
        request.Headers.Referrer = new Uri("https://referrer.example/source");
        request.Headers.UserAgent.ParseAdd("shorten-link-tests/1.0");

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(await WaitForConditionAsync(async () =>
        {
            var clicks = await factory.GetRecordedClicksAsync();
            return clicks.Count == 1;
        }));

        var click = Assert.Single(await factory.GetRecordedClicksAsync());
        Assert.Equal("track01", click.ShortCode);
        Assert.Equal("shorten-link-tests/1.0", click.UserAgent);
        Assert.Equal("https://referrer.example/source", click.Referrer);
    }

    [Fact]
    public async Task Redirect_DoesNotRecordClickAnalytics_WhenDisabled()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false, analyticsEnabled: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/redirect",
            customAlias = "track00"
        });

        using var response = await client.GetAsync("/track00");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await Task.Delay(150);
        Assert.Empty(await factory.GetRecordedClicksAsync());
    }

    [Fact]
    public async Task RateLimiting_DisabledByDefault_DoesNotThrottleCreateRequests()
    {
        await using var factory = new ShortLinkApiFactory(
            enableFrontendFallback: false,
            rateLimitingEnabled: false,
            createPermitLimit: 1);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var firstResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/one",
            customAlias = "open01"
        });
        using var secondResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/two",
            customAlias = "open02"
        });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
    }

    [Fact]
    public async Task RateLimiting_ReturnsTooManyRequestsForCreate_WhenLimitExceeded()
    {
        await using var factory = new ShortLinkApiFactory(
            enableFrontendFallback: false,
            rateLimitingEnabled: true,
            createPermitLimit: 1,
            redirectPermitLimit: 10);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var firstResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/one",
            customAlias = "limit01"
        });
        using var secondResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/two",
            customAlias = "limit02"
        });

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
    }

    [Fact]
    public async Task RateLimiting_ReturnsTooManyRequestsForRedirect_BeforeSecondAnalyticsRecord()
    {
        await using var factory = new ShortLinkApiFactory(
            enableFrontendFallback: false,
            analyticsEnabled: true,
            rateLimitingEnabled: true,
            createPermitLimit: 10,
            redirectPermitLimit: 1);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/redirect",
            customAlias = "limitrd"
        });

        using var firstResponse = await client.GetAsync("/limitrd");
        using var secondResponse = await client.GetAsync("/limitrd");

        Assert.Equal(HttpStatusCode.Redirect, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, secondResponse.StatusCode);
        Assert.True(await WaitForConditionAsync(async () =>
        {
            var clicks = await factory.GetRecordedClicksAsync();
            return clicks.Count == 1;
        }));
        Assert.Single(await factory.GetRecordedClicksAsync());
    }

    [Fact]
    public async Task UnknownCode_RedirectsToFrontendFallbackWhenEnabled()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: true);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/missing");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/not-found", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task UnknownCode_RedirectsToAbsoluteFrontendFallbackWhenConfigured()
    {
        await using var factory = new ShortLinkApiFactory(
            enableFrontendFallback: true,
            frontendFallbackPath: "http://localhost:5173/not-found");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/missing");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost:5173/not-found", response.Headers.Location?.AbsoluteUri);
    }

    [Fact]
    public async Task UnknownCode_ReturnsJson404WhenFrontendFallbackDisabled()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/missing");
        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("not_found", payload.ErrorCode);
    }

    [Fact]
    public void AddShortenLink_UsesSqliteProviderByDefault()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Database:UsePostgres"] = "false",
            ["ShortenLink:Database:SqliteConnectionString"] = "Data Source=provider-sqlite.db"
        });

        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value;
        var dbContext = scope.ServiceProvider.GetRequiredService<ShortLinkDbContext>();

        Assert.False(options.Database.UsePostgres);
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", dbContext.Database.ProviderName);
    }

    [Fact]
    public void AddShortenLink_UsesPostgresProviderWhenEnabled()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Database:UsePostgres"] = "true",
            ["ShortenLink:Database:PostgresConnectionString"] = "Host=localhost;Port=5432;Database=shorten_link_tests;Username=postgres;Password=postgres"
        });

        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value;
        var dbContext = scope.ServiceProvider.GetRequiredService<ShortLinkDbContext>();

        Assert.True(options.Database.UsePostgres);
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", dbContext.Database.ProviderName);
    }

    [Fact]
    public void AddShortenLink_RejectsMissingPostgresConnectionStringWhenEnabled()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Database:UsePostgres"] = "true",
            ["ShortenLink:Database:PostgresConnectionString"] = ""
        });

        using var scope = services.CreateScope();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            _ = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value);

        Assert.Contains("PostgresConnectionString", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddShortenLink_UsesDisabledCacheByDefault()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>());

        var cache = services.GetRequiredService<IShortLinkCache>();

        Assert.IsType<DisabledShortLinkCache>(cache);
    }

    [Fact]
    public void AddShortenLink_UsesMemoryCacheProviderWhenEnabled()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Cache:Enabled"] = "true",
            ["ShortenLink:Cache:Provider"] = "Memory"
        });

        var distributedCache = services.GetRequiredService<IDistributedCache>();
        var shortLinkCache = services.GetRequiredService<IShortLinkCache>();

        Assert.Contains("Memory", distributedCache.GetType().Name, StringComparison.Ordinal);
        Assert.Equal("DistributedShortLinkCache", shortLinkCache.GetType().Name);
    }

    [Fact]
    public void AddShortenLink_UsesRedisCacheProviderWhenEnabled()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Cache:Enabled"] = "true",
            ["ShortenLink:Cache:Provider"] = "Redis",
            ["ShortenLink:Cache:RedisConnectionString"] = "localhost:6379"
        });

        var distributedCache = services.GetRequiredService<IDistributedCache>();
        var shortLinkCache = services.GetRequiredService<IShortLinkCache>();

        Assert.Contains("Redis", distributedCache.GetType().Name, StringComparison.Ordinal);
        Assert.Equal("DistributedShortLinkCache", shortLinkCache.GetType().Name);
    }

    [Fact]
    public void AddShortenLink_RejectsInvalidCacheProvider()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Cache:Enabled"] = "true",
            ["ShortenLink:Cache:Provider"] = "Disk"
        });

        using var scope = services.CreateScope();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            _ = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value);

        Assert.Contains("Cache:Provider", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddShortenLink_RejectsMissingRedisConnectionStringWhenEnabled()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:Cache:Enabled"] = "true",
            ["ShortenLink:Cache:Provider"] = "Redis",
            ["ShortenLink:Cache:RedisConnectionString"] = ""
        });

        using var scope = services.CreateScope();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            _ = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value);

        Assert.Contains("RedisConnectionString", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AddShortenLink_BindsRateLimitingOptions()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:RateLimiting:Enabled"] = "true",
            ["ShortenLink:RateLimiting:Create:PermitLimit"] = "3",
            ["ShortenLink:RateLimiting:Create:WindowSeconds"] = "11",
            ["ShortenLink:RateLimiting:Redirect:PermitLimit"] = "7",
            ["ShortenLink:RateLimiting:Redirect:WindowSeconds"] = "13"
        });

        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value;

        Assert.True(options.RateLimiting.Enabled);
        Assert.Equal(3, options.RateLimiting.Create.PermitLimit);
        Assert.Equal(11, options.RateLimiting.Create.WindowSeconds);
        Assert.Equal(7, options.RateLimiting.Redirect.PermitLimit);
        Assert.Equal(13, options.RateLimiting.Redirect.WindowSeconds);
    }

    [Fact]
    public void AddShortenLink_RejectsInvalidRateLimitOptions()
    {
        using var services = BuildServiceProvider(new Dictionary<string, string?>
        {
            ["ShortenLink:RateLimiting:Enabled"] = "true",
            ["ShortenLink:RateLimiting:Create:PermitLimit"] = "0"
        });

        using var scope = services.CreateScope();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            _ = scope.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value);

        Assert.Contains("RateLimiting", exception.Message, StringComparison.Ordinal);
    }

    private sealed class ShortLinkApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly string databaseDirectory = Path.Combine(Path.GetTempPath(), $"shorten-link-api-tests-{Guid.NewGuid():N}");
        private readonly bool enableFrontendFallback;
        private readonly string frontendFallbackPath;
        private readonly bool analyticsEnabled;
        private readonly bool cacheEnabled;
        private readonly string cacheProvider;
        private readonly bool rateLimitingEnabled;
        private readonly int createPermitLimit;
        private readonly int redirectPermitLimit;

        public ShortLinkApiFactory(
            bool enableFrontendFallback,
            string frontendFallbackPath = "/not-found",
            bool analyticsEnabled = false,
            bool cacheEnabled = false,
            string cacheProvider = "Memory",
            bool rateLimitingEnabled = false,
            int createPermitLimit = 60,
            int redirectPermitLimit = 120)
        {
            this.enableFrontendFallback = enableFrontendFallback;
            this.frontendFallbackPath = frontendFallbackPath;
            this.analyticsEnabled = analyticsEnabled;
            this.cacheEnabled = cacheEnabled;
            this.cacheProvider = cacheProvider;
            this.rateLimitingEnabled = rateLimitingEnabled;
            this.createPermitLimit = createPermitLimit;
            this.redirectPermitLimit = redirectPermitLimit;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Directory.CreateDirectory(databaseDirectory);

            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ShortenLink:BaseUrl"] = "https://sho.rt",
                    ["ShortenLink:Database:UsePostgres"] = "false",
                    ["ShortenLink:Database:SqliteConnectionString"] = $"Data Source={Path.Combine(databaseDirectory, "app.db")}",
                    ["ShortenLink:Redirect:EnableFrontendFallback"] = enableFrontendFallback.ToString(),
                    ["ShortenLink:Redirect:FrontendFallbackPath"] = frontendFallbackPath,
                    ["ShortenLink:Analytics:Enabled"] = analyticsEnabled.ToString(),
                    ["ShortenLink:Analytics:UseAsyncWorker"] = "true",
                    ["ShortenLink:Analytics:QueueCapacity"] = "32",
                    ["ShortenLink:Cache:Enabled"] = cacheEnabled.ToString(),
                    ["ShortenLink:Cache:Provider"] = cacheProvider,
                    ["ShortenLink:Cache:RedisConnectionString"] = "localhost:6379",
                    ["ShortenLink:Cache:EntryTtlSeconds"] = "300",
                    ["ShortenLink:RateLimiting:Enabled"] = rateLimitingEnabled.ToString(),
                    ["ShortenLink:RateLimiting:Create:PermitLimit"] = createPermitLimit.ToString(),
                    ["ShortenLink:RateLimiting:Create:WindowSeconds"] = "60",
                    ["ShortenLink:RateLimiting:Create:QueueLimit"] = "0",
                    ["ShortenLink:RateLimiting:Redirect:PermitLimit"] = redirectPermitLimit.ToString(),
                    ["ShortenLink:RateLimiting:Redirect:WindowSeconds"] = "60",
                    ["ShortenLink:RateLimiting:Redirect:QueueLimit"] = "0"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<TimeProvider>(
                    new FixedTimeProvider(new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero)));
            });
        }

        public async Task<List<ShortLinkClickRecord>> GetRecordedClicksAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ShortLinkDbContext>();

            return await dbContext.ShortLinkClicks
                .AsNoTracking()
                .OrderBy(click => click.Id)
                .ToListAsync();
        }

        public new ValueTask DisposeAsync()
        {
            base.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private static ServiceProvider BuildServiceProvider(IDictionary<string, string?> overrides)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ShortenLink:BaseUrl"] = "https://sho.rt",
                ["ShortenLink:Database:UsePostgres"] = "false",
                ["ShortenLink:Database:SqliteConnectionString"] = "Data Source=shorten-link-provider-tests.db",
                ["ShortenLink:Redirect:EnableFrontendFallback"] = "true",
                ["ShortenLink:Redirect:FrontendFallbackPath"] = "/not-found",
                ["ShortenLink:Analytics:Enabled"] = "false",
                ["ShortenLink:Analytics:UseAsyncWorker"] = "true",
                ["ShortenLink:Analytics:QueueCapacity"] = "32",
                ["ShortenLink:Cache:Enabled"] = "false",
                ["ShortenLink:Cache:Provider"] = "Memory",
                ["ShortenLink:Cache:RedisConnectionString"] = "localhost:6379",
                ["ShortenLink:Cache:EntryTtlSeconds"] = "300",
                ["ShortenLink:RateLimiting:Enabled"] = "false",
                ["ShortenLink:RateLimiting:Create:PermitLimit"] = "60",
                ["ShortenLink:RateLimiting:Create:WindowSeconds"] = "60",
                ["ShortenLink:RateLimiting:Create:QueueLimit"] = "0",
                ["ShortenLink:RateLimiting:Redirect:PermitLimit"] = "120",
                ["ShortenLink:RateLimiting:Redirect:WindowSeconds"] = "60",
                ["ShortenLink:RateLimiting:Redirect:QueueLimit"] = "0"
            })
            .AddInMemoryCollection(overrides)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddShortenLink(configuration);

        return services.BuildServiceProvider();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            this.utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed record ShortLinkCreatedResponse(
        string Code,
        string ShortUrl,
        string OriginalUrl,
        DateTimeOffset CreatedAtUtc);

    private sealed record ShortLinkDetailsResponse(
        string Code,
        string OriginalUrl,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? ExpiredAtUtc,
        bool IsActive);

    private sealed record ShortLinkErrorResponse(string ErrorCode, string Message);

    private static async Task<bool> WaitForConditionAsync(Func<Task<bool>> condition)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(50);
        }

        return false;
    }
}
