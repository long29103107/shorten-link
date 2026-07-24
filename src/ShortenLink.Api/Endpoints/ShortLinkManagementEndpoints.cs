using Microsoft.Extensions.Options;
using ShortenLink.AspNetCore;

namespace ShortenLink.Api.Endpoints;

internal static class ShortLinkManagementEndpoints
{
    public static IEndpointRouteBuilder MapShortLinkManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/short-links")
            .WithTags("Short Links");

        group.MapGet("/", ShortenLinkEndpointHandlers.ListShortLinksAsync)
            .WithName("ListShortLinks");

        var createEndpoint = group.MapPost("/", ShortenLinkEndpointHandlers.CreateShortLinkAsync)
            .WithName("CreateShortLink");

        group.MapGet("/{code}", ShortenLinkEndpointHandlers.GetShortLinkDetailsAsync)
            .WithName("GetShortLinkDetails");
        group.MapGet("/{code}/analytics", ShortenLinkEndpointHandlers.GetShortLinkAnalyticsAsync)
            .WithName("GetShortLinkAnalytics");
        group.MapGet("/{code}/shares", ShortenLinkEndpointHandlers.ListShortLinkSharesAsync)
            .WithName("ListShortLinkShares");
        group.MapPut("/{code}/shares", ShortenLinkEndpointHandlers.UpsertShortLinkShareAsync)
            .WithName("UpsertShortLinkShare");
        group.MapDelete("/{code}/shares/{userId}", ShortenLinkEndpointHandlers.DeleteShortLinkShareAsync)
            .WithName("DeleteShortLinkShare");
        group.MapPut("/{code}", ShortenLinkEndpointHandlers.UpdateShortLinkAsync)
            .WithName("UpdateShortLink");
        group.MapPost("/{code}/deactivate", ShortenLinkEndpointHandlers.DeactivateShortLinkAsync)
            .WithName("DeactivateShortLink");
        group.MapPost("/{code}/activate", ShortenLinkEndpointHandlers.ActivateShortLinkAsync)
            .WithName("ActivateShortLink");
        group.MapDelete("/{code}", ShortenLinkEndpointHandlers.DeleteShortLinkAsync)
            .WithName("DeleteShortLink");

        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value;
        if (options.RateLimiting.Enabled)
        {
            createEndpoint.RequireRateLimiting(ShortenLinkRateLimitingPolicyNames.Create);
        }

        return endpoints;
    }
}
