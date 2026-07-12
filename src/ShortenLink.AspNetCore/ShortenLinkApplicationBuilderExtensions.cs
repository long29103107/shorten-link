using Microsoft.AspNetCore.Builder;

namespace ShortenLink.AspNetCore;

public static class ShortenLinkApplicationBuilderExtensions
{
    public static IApplicationBuilder UseShortenLinkRateLimiting(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseRateLimiter();
    }
}
