namespace ShortenLink.Api.Endpoints;

internal static class SecuritySessionEndpoints
{
    public static IEndpointRouteBuilder MapSecuritySessionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/security")
            .WithTags("Security");

        group.MapPost("/login", ShortenLinkEndpointHandlers.LoginSecurityUserAsync)
            .WithName("LoginSecurityUser");
        group.MapPost("/refresh", ShortenLinkEndpointHandlers.RefreshSecurityUserAsync)
            .WithName("RefreshSecurityUser");
        group.MapGet("/me", ShortenLinkEndpointHandlers.GetCurrentSecurityUserAsync)
            .WithName("GetCurrentSecurityUser");

        return endpoints;
    }
}
