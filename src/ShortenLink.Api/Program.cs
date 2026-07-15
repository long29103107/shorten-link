using ShortenLink.AspNetCore;
using ShortenLink.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddShortenLink(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapPost("/api/mock/seed-short-links", async (
        IShortLinkService shortLinkService,
        int? count,
        CancellationToken cancellationToken) =>
    {
        var requestedCount = Math.Clamp(count ?? 200, 1, 500);
        var createdCodes = new List<string>(requestedCount);
        var failedCount = 0;

        for (var index = 1; index <= requestedCount; index++)
        {
            var result = await shortLinkService.CreateAsync(
                new CreateShortLinkRequest(Program.CreateMockUrl(index)),
                cancellationToken).ConfigureAwait(false);

            if (result.Succeeded && result.ShortLink is not null)
            {
                createdCodes.Add(result.ShortLink.Code);
            }
            else
            {
                failedCount++;
            }
        }

        return TypedResults.Ok(new MockSeedShortLinksResponse(
            requestedCount,
            createdCodes.Count,
            failedCount,
            createdCodes));
    })
    .WithName("SeedMockShortLinks")
    .WithTags("Mock Data");
}

app.UseShortenLinkRateLimiting();

app.MapShortenLinkEndpoints();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    app = "ShortenLink.Api"
}))
.WithName("Health");

app.Run();

public partial class Program
{
    internal static string CreateMockUrl(int index)
    {
        var normalizedIndex = Math.Max(index, 1);
        var domains = new[]
        {
            "https://example.com",
            "https://github.com",
            "https://learn.microsoft.com",
            "https://www.episoden.com",
            "https://docs.example.dev"
        };
        var domain = domains[(normalizedIndex - 1) % domains.Length];

        return $"{domain}/mock/short-link/{normalizedIndex:000}?source=seed";
    }
}

internal sealed record MockSeedShortLinksResponse(
    int RequestedCount,
    int CreatedCount,
    int FailedCount,
    IReadOnlyList<string> Codes);
