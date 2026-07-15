using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ShortenLink.AspNetCore;

public interface IShortenLinkAuthorizationService
{
    ShortenLinkAuthorizationResult Authorize(HttpContext httpContext, string permission);
}

public sealed record ShortenLinkAuthorizationResult(
    bool Succeeded,
    bool IsAuthenticated,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ShortenLinkAuthorizationResult Success() =>
        new(true, true, null, null);

    public static ShortenLinkAuthorizationResult Unauthorized() =>
        new(false, false, "unauthorized", "A valid admin API key is required.");

    public static ShortenLinkAuthorizationResult Forbidden() =>
        new(false, true, "forbidden", "The admin API key does not include the required permission.");
}

public sealed class ShortenLinkAuthorizationService(
    IOptions<ShortenLinkOptions> options) : IShortenLinkAuthorizationService
{
    public ShortenLinkAuthorizationResult Authorize(HttpContext httpContext, string permission)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        var security = options.Value.Security;
        if (!security.Enabled)
        {
            return ShortenLinkAuthorizationResult.Success();
        }

        if (!httpContext.Request.Headers.TryGetValue(security.HeaderName, out var keyValues))
        {
            return ShortenLinkAuthorizationResult.Unauthorized();
        }

        var apiKey = keyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ShortenLinkAuthorizationResult.Unauthorized();
        }

        var principal = security.ApiKeys.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(candidate.Key)
            && string.Equals(candidate.Key, apiKey, StringComparison.Ordinal));
        if (principal is null)
        {
            return ShortenLinkAuthorizationResult.Unauthorized();
        }

        var permissions = GetEffectivePermissions(principal);
        return permissions.Contains(permission)
            ? ShortenLinkAuthorizationResult.Success()
            : ShortenLinkAuthorizationResult.Forbidden();
    }

    private static HashSet<string> GetEffectivePermissions(ShortenLinkApiKeyOptions apiKey)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var permission in apiKey.Permissions.Where(static permission => !string.IsNullOrWhiteSpace(permission)))
        {
            permissions.Add(permission);
        }

        foreach (var role in apiKey.Roles.Where(static role => !string.IsNullOrWhiteSpace(role)))
        {
            if (!ShortenLinkRoles.PermissionBundles.TryGetValue(role, out var rolePermissions))
            {
                continue;
            }

            foreach (var permission in rolePermissions)
            {
                permissions.Add(permission);
            }
        }

        return permissions;
    }
}
