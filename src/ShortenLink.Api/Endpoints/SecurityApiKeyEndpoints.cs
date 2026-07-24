namespace ShortenLink.Api.Endpoints;

internal static class SecurityApiKeyEndpoints
{
    public static IEndpointRouteBuilder MapSecurityApiKeyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup("/api/security/api-keys")
            .WithTags("Security API Keys");

        group.MapGet("/", ShortenLinkEndpointHandlers.ListCurrentUserApiKeysAsync)
            .WithName("ListCurrentUserApiKeys");
        group.MapPost("/", ShortenLinkEndpointHandlers.CreateCurrentUserApiKeyAsync)
            .WithName("CreateCurrentUserApiKey");
        group.MapPut("/{id}", ShortenLinkEndpointHandlers.RenameCurrentUserApiKeyAsync)
            .WithName("RenameCurrentUserApiKey");
        group.MapPost("/{id}/disable", ShortenLinkEndpointHandlers.DisableCurrentUserApiKeyAsync)
            .WithName("DisableCurrentUserApiKey");

        return endpoints;
    }
}
