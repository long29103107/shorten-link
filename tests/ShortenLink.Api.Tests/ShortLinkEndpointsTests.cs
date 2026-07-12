using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShortenLink.Api;
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

    private sealed class ShortLinkApiFactory : WebApplicationFactory<Program>, IAsyncDisposable
    {
        private readonly string databaseDirectory = Path.Combine(Path.GetTempPath(), $"shorten-link-api-tests-{Guid.NewGuid():N}");
        private readonly bool enableFrontendFallback;

        public ShortLinkApiFactory(bool enableFrontendFallback)
        {
            this.enableFrontendFallback = enableFrontendFallback;
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
                    ["ShortenLink:Redirect:FrontendFallbackPath"] = "/not-found"
                });
            });
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<TimeProvider>(
                    new FixedTimeProvider(new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero)));
            });
        }

        public new ValueTask DisposeAsync()
        {
            base.Dispose();
            return ValueTask.CompletedTask;
        }
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
}
