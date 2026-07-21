using Microsoft.AspNetCore.Http.HttpResults;
using ShortenLink.Core.Services;

namespace ShortenLink.Api.Endpoints;

internal static class MockDataEndpoints
{
    public static IEndpointRouteBuilder MapMockDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/api/mock/seed-short-links", SeedShortLinksAsync)
            .WithName("SeedMockShortLinks")
            .WithTags("Mock Data");

        return endpoints;
    }

    private static async Task<Ok<MockSeedShortLinksResponse>> SeedShortLinksAsync(
        IShortLinkService shortLinkService,
        int? count,
        CancellationToken cancellationToken)
    {
        var requestedCount = Math.Clamp(count ?? 200, 1, 500);
        var createdCodes = new List<string>(requestedCount);
        var failedCount = 0;

        for (var index = 1; index <= requestedCount; index++)
        {
            var result = await shortLinkService.CreateAsync(
                new CreateShortLinkRequest(CreateMockUrl(index), DateTimeOffset.UtcNow.AddDays(30)),
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
    }

    internal static string CreateMockUrl(int index)
    {
        var normalizedIndex = Math.Max(index, 1);
        string[] domains =
        [
            "https://example.com",
            "https://github.com",
            "https://learn.microsoft.com",
            "https://www.episoden.com",
            "https://docs.example.dev"
        ];
        var domain = domains[(normalizedIndex - 1) % domains.Length];
        return $"{domain}/mock/short-link/{normalizedIndex:000}?source=seed";
    }

    private sealed record MockSeedShortLinksResponse(
        int RequestedCount,
        int CreatedCount,
        int FailedCount,
        IReadOnlyList<string> Codes);
}
