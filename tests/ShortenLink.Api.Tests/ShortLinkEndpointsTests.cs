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
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkCreatedResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.Code));
        Assert.Equal(7, payload.Code.Length);
        Assert.Equal($"https://sho.rt/{payload.Code}", payload.ShortUrl);
        Assert.Equal("https://example.com/docs", payload.OriginalUrl);
        Assert.Equal(new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero), payload.CreatedAtUtc);
    }

    [Fact]
    public async Task PostCreate_GeneratesRandomCodesForRepeatedCreates()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var firstResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/one",
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });
        using var secondResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/two",
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 1, 0, 0, TimeSpan.Zero)
        });

        var firstPayload = await firstResponse.Content.ReadFromJsonAsync<ShortLinkCreatedResponse>();
        var secondPayload = await secondResponse.Content.ReadFromJsonAsync<ShortLinkCreatedResponse>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.NotNull(firstPayload);
        Assert.NotNull(secondPayload);
        Assert.Equal(7, firstPayload.Code.Length);
        Assert.Equal(7, secondPayload.Code.Length);
        Assert.NotEqual(firstPayload.Code, secondPayload.Code);
    }

    [Fact]
    public async Task GetList_ReturnsRecentShortLinksForAdmin()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var first = await CreateShortLinkAsync(client, "https://example.com/one");
        var second = await CreateShortLinkAsync(client, "https://example.com/two");

        using var response = await client.GetAsync("/api/short-links?limit=10");
        var payload = await response.Content.ReadFromJsonAsync<ShortLinkAdminListResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(2, payload.Items.Count);
        Assert.Null(payload.NextCursor);

        var firstItem = Assert.Single(payload.Items, item => item.Code == first.Code);
        Assert.Equal(first.ShortUrl, firstItem.ShortUrl);
        Assert.Equal("https://example.com/one", firstItem.OriginalUrl);
        Assert.True(firstItem.IsActive);

        var secondItem = Assert.Single(payload.Items, item => item.Code == second.Code);
        Assert.Equal(second.ShortUrl, secondItem.ShortUrl);
        Assert.Equal("https://example.com/two", secondItem.OriginalUrl);
        Assert.True(secondItem.IsActive);
    }

    [Fact]
    public async Task GetList_ReturnsCursorForNextPage()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        await CreateShortLinkAsync(client, "https://example.com/one");
        await CreateShortLinkAsync(client, "https://example.com/two");
        await CreateShortLinkAsync(client, "https://example.com/three");

        using var firstResponse = await client.GetAsync("/api/short-links?limit=2");
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<ShortLinkAdminListResponse>();

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.Items.Count);
        Assert.False(string.IsNullOrWhiteSpace(firstPage.NextCursor));

        using var secondResponse = await client.GetAsync($"/api/short-links?limit=2&cursor={Uri.EscapeDataString(firstPage.NextCursor)}");
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<ShortLinkAdminListResponse>();

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.NotNull(secondPage);
        Assert.Single(secondPage.Items);
        Assert.Empty(firstPage.Items.Select(item => item.Code).Intersect(secondPage.Items.Select(item => item.Code)));
        Assert.Equal(
            new[]
            {
                "https://example.com/one",
                "https://example.com/three",
                "https://example.com/two"
            },
            firstPage.Items.Concat(secondPage.Items)
                .Select(item => item.OriginalUrl)
                .OrderBy(url => url, StringComparer.Ordinal)
                .ToArray());
        Assert.Null(secondPage.NextCursor);
    }

    [Fact]
    public async Task PostMockSeedShortLinks_CreatesRequestedMockLinks()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var seedResponse = await client.PostAsync("/api/mock/seed-short-links?count=12", null);
        var seedPayload = await seedResponse.Content.ReadFromJsonAsync<MockSeedShortLinksResponse>();

        Assert.Equal(HttpStatusCode.OK, seedResponse.StatusCode);
        Assert.NotNull(seedPayload);
        Assert.Equal(12, seedPayload.RequestedCount);
        Assert.Equal(12, seedPayload.CreatedCount);
        Assert.Equal(0, seedPayload.FailedCount);
        Assert.Equal(12, seedPayload.Codes.Count);

        using var listResponse = await client.GetAsync("/api/short-links?limit=20");
        var listPayload = await listResponse.Content.ReadFromJsonAsync<ShortLinkAdminListResponse>();

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(listPayload);
        Assert.Equal(12, listPayload.Items.Count);
    }

    [Fact]
    public async Task PostCreate_ReturnsBadRequestForInvalidUrl()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "ftp://example.com/file",
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("invalid_url", payload.ErrorCode);
    }

    [Fact]
    public async Task PostCreate_ReturnsBadRequestWhenDestinationUrlIsMissing()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("invalid_url", payload.ErrorCode);
    }

    [Fact]
    public async Task PostCreate_ReturnsBadRequestWhenExpiryIsMissing()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/docs"
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("invalid_expiration", payload.ErrorCode);
    }

    [Fact]
    public async Task GetDetails_ReturnsStoredShortLink()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();

        var created = await CreateShortLinkAsync(client, "https://example.com/details");

        using var response = await client.GetAsync($"/api/short-links/{created.Code}");
        var payload = await response.Content.ReadFromJsonAsync<ShortLinkDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(created.Code, payload.Code);
        Assert.Equal("https://example.com/details", payload.OriginalUrl);
        Assert.True(payload.IsActive);
    }

    [Fact]
    public async Task PostDeactivate_DeactivatesShortLinkAndRedirectReturnsGone()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var created = await CreateShortLinkAsync(client, "https://example.com/remove");

        using var deleteResponse = await client.PostAsync($"/api/short-links/{created.Code}/deactivate", null);
        using var redirectResponse = await client.GetAsync($"/{created.Code}");
        var payload = await redirectResponse.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Gone, redirectResponse.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("inactive", payload.ErrorCode);
    }

    [Fact]
    public async Task PutUpdate_ChangesDestinationForRedirect()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var created = await CreateShortLinkAsync(client, "https://example.com/old");

        using var updateResponse = await client.PutAsJsonAsync($"/api/short-links/{created.Code}", new
        {
            originalUrl = "https://example.com/new",
            expiredAtUtc = new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero)
        });
        using var redirectResponse = await client.GetAsync($"/{created.Code}");

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, redirectResponse.StatusCode);
        Assert.Equal("https://example.com/new", redirectResponse.Headers.Location?.AbsoluteUri);
    }

    [Fact]
    public async Task PutUpdate_ReturnsBadRequestWhenDestinationUrlIsMissing()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();
        var created = await CreateShortLinkAsync(client, "https://example.com/old");

        using var response = await client.PutAsJsonAsync($"/api/short-links/{created.Code}", new
        {
            expiredAtUtc = new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero)
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("invalid_url", payload.ErrorCode);
    }

    [Fact]
    public async Task PutUpdate_ReturnsBadRequestWhenExpiryIsMissing()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient();
        var created = await CreateShortLinkAsync(client, "https://example.com/old");

        using var response = await client.PutAsJsonAsync($"/api/short-links/{created.Code}", new
        {
            originalUrl = "https://example.com/new"
        });

        var payload = await response.Content.ReadFromJsonAsync<ShortLinkErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("invalid_expiration", payload.ErrorCode);
    }

    [Fact]
    public async Task Delete_RemovesShortLink()
    {
        await using var factory = new ShortLinkApiFactory(enableFrontendFallback: false);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var created = await CreateShortLinkAsync(client, "https://example.com/delete");

        using var deleteResponse = await client.DeleteAsync($"/api/short-links/{created.Code}");
        using var detailsResponse = await client.GetAsync($"/api/short-links/{created.Code}");

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, detailsResponse.StatusCode);
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

        var created = await CreateShortLinkAsync(client, "https://example.com/cached-remove");

        using var firstRedirectResponse = await client.GetAsync($"/{created.Code}");
        using var deleteResponse = await client.PostAsync($"/api/short-links/{created.Code}/deactivate", null);
        using var secondRedirectResponse = await client.GetAsync($"/{created.Code}");
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

        var created = await CreateShortLinkAsync(client, "https://example.com/redirect");

        using var response = await client.GetAsync($"/{created.Code}");

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

        var created = await CreateShortLinkAsync(client, "https://example.com/redirect");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/{created.Code}");
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
        Assert.Equal(created.Code, click.ShortCode);
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

        var created = await CreateShortLinkAsync(client, "https://example.com/redirect");

        using var response = await client.GetAsync($"/{created.Code}");

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
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });
        using var secondResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/two",
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 1, 0, 0, TimeSpan.Zero)
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
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero)
        });
        using var secondResponse = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl = "https://example.com/two",
            expiredAtUtc = new DateTimeOffset(2026, 7, 20, 1, 0, 0, TimeSpan.Zero)
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

        var created = await CreateShortLinkAsync(client, "https://example.com/redirect");

        using var firstResponse = await client.GetAsync($"/{created.Code}");
        using var secondResponse = await client.GetAsync($"/{created.Code}");

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

    private static async Task<ShortLinkCreatedResponse> CreateShortLinkAsync(
        HttpClient client,
        string originalUrl,
        DateTimeOffset? expiredAtUtc = null)
    {
        expiredAtUtc ??= new DateTimeOffset(2026, 7, 20, 0, 0, 0, TimeSpan.Zero);

        using var response = await client.PostAsJsonAsync("/api/short-links", new
        {
            originalUrl,
            expiredAtUtc
        });
        var payload = await response.Content.ReadFromJsonAsync<ShortLinkCreatedResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);

        return payload;
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

    private sealed record ShortLinkAdminListItemResponse(
        string Code,
        string ShortUrl,
        string OriginalUrl,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? ExpiredAtUtc,
        bool IsActive);

    private sealed record ShortLinkAdminListResponse(
        IReadOnlyList<ShortLinkAdminListItemResponse> Items,
        string? NextCursor);

    private sealed record MockSeedShortLinksResponse(
        int RequestedCount,
        int CreatedCount,
        int FailedCount,
        IReadOnlyList<string> Codes);

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
