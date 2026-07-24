namespace ShortenLink.Api.Endpoints;

internal static class SecurityRoleEndpoints
{
    public static IEndpointRouteBuilder MapSecurityRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/security/roles")
            .WithTags("Security Roles");

        group.MapGet("/", ShortenLinkEndpointHandlers.ListSecurityRolesAsync)
            .WithName("ListSecurityRoles");
        group.MapPut("/custom", ShortenLinkEndpointHandlers.UpsertCustomSecurityRoleAsync)
            .WithName("UpsertCustomSecurityRole");
        group.MapPut("/{id}/permission-overrides", ShortenLinkEndpointHandlers.ReplaceSecurityRolePermissionOverridesAsync)
            .WithName("ReplaceSecurityRolePermissionOverrides");
        group.MapDelete("/custom/{id}", ShortenLinkEndpointHandlers.DeleteCustomSecurityRoleAsync)
            .WithName("DeleteCustomSecurityRole");

        return endpoints;
    }
}
