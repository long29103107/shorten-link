namespace ShortenLink.Api.Endpoints;

internal static class SecurityUserEndpoints
{
    public static IEndpointRouteBuilder MapSecurityUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/security/users")
            .WithTags("Security Users");

        group.MapGet("/", ShortenLinkEndpointHandlers.ListSecurityUsersAsync)
            .WithName("ListSecurityUsers");
        group.MapPut("/", ShortenLinkEndpointHandlers.UpsertSecurityUserAsync)
            .WithName("UpsertSecurityUser");
        group.MapPost("/{id}/disable", ShortenLinkEndpointHandlers.DisableSecurityUserAsync)
            .WithName("DisableSecurityUser");

        return endpoints;
    }
}
