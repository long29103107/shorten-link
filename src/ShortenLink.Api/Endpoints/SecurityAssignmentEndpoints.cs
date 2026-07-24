namespace ShortenLink.Api.Endpoints;

internal static class SecurityAssignmentEndpoints
{
    public static IEndpointRouteBuilder MapSecurityAssignmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/security/assignments")
            .WithTags("Security Assignments");

        group.MapGet("/", ShortenLinkEndpointHandlers.ListSecurityAssignmentsAsync)
            .WithName("ListSecurityAssignments");
        group.MapPut("/", ShortenLinkEndpointHandlers.UpsertSecurityAssignmentAsync)
            .WithName("UpsertSecurityAssignment");
        group.MapPost("/{credentialKeyHash}/disable", ShortenLinkEndpointHandlers.DisableSecurityAssignmentAsync)
            .WithName("DisableSecurityAssignment");

        return endpoints;
    }
}
