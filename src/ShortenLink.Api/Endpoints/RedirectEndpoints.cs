using Microsoft.Extensions.Options;
using ShortenLink.AspNetCore;

namespace ShortenLink.Api.Endpoints;

internal static class RedirectEndpoints
{
    public static IEndpointRouteBuilder MapRedirectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var redirectEndpoint = endpoints.MapGet("/{code}", ShortenLinkEndpointHandlers.RedirectShortLinkAsync)
            .WithName("RedirectShortLink");

        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<ShortenLinkOptions>>().Value;
        if (options.RateLimiting.Enabled)
        {
            redirectEndpoint.RequireRateLimiting(ShortenLinkRateLimitingPolicyNames.Redirect);
        }

        return endpoints;
    }
}
