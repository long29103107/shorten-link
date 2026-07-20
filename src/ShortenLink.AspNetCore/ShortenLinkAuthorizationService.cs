using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ShortenLink.Core.Repositories;
using ShortenLink.Core.Security;

namespace ShortenLink.AspNetCore;

public interface IShortenLinkAuthorizationService
{
    Task<ShortenLinkAuthorizationResult> AuthorizeAsync(
        HttpContext httpContext,
        string permission,
        CancellationToken cancellationToken = default);
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
        new(false, false, "unauthorized", "A valid admin credential is required.");

    public static ShortenLinkAuthorizationResult Forbidden() =>
        new(false, true, "forbidden", "The admin credential does not include the required permission.");
}

public sealed class ShortenLinkAuthorizationService(
    IOptions<ShortenLinkOptions> options,
    IShortenLinkSecurityAssignmentRepository securityAssignmentRepository,
    IShortenLinkUserApiKeyRepository userApiKeyRepository,
    IShortenLinkSecurityUserRepository userRepository,
    IShortenLinkUserSessionService userSessionService) : IShortenLinkAuthorizationService
{
    public async Task<ShortenLinkAuthorizationResult> AuthorizeAsync(
        HttpContext httpContext,
        string permission,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        var security = options.Value.Security;
        if (!security.Enabled)
        {
            return ShortenLinkAuthorizationResult.Success();
        }

        if (HasBearerToken(httpContext))
        {
            var session = await userSessionService
                .GetCurrentUserAsync(httpContext, cancellationToken)
                .ConfigureAwait(false);
            if (!session.Succeeded || session.Principal is null)
            {
                return ShortenLinkAuthorizationResult.Unauthorized();
            }

            return session.Principal.Permissions.Contains(permission, StringComparer.Ordinal)
                ? ShortenLinkAuthorizationResult.Success()
                : ShortenLinkAuthorizationResult.Forbidden();
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

        var apiKeyHash = ShortenLinkSecurityCredentialHasher.HashApiKey(apiKey);
        var userApiKey = await userApiKeyRepository
            .FindByKeyHashAsync(apiKeyHash, cancellationToken)
            .ConfigureAwait(false);
        if (userApiKey is not null)
        {
            if (!userApiKey.IsEnabled)
            {
                return ShortenLinkAuthorizationResult.Unauthorized();
            }

            var owner = await userRepository
                .FindByIdAsync(userApiKey.UserId, cancellationToken)
                .ConfigureAwait(false);
            if (owner is null || !owner.IsEnabled)
            {
                return ShortenLinkAuthorizationResult.Unauthorized();
            }

            var userPrincipal = await userSessionService
                .CreatePrincipalAsync(owner, userApiKey.CreatedAt, cancellationToken)
                .ConfigureAwait(false);

            return userPrincipal.Permissions.Contains(permission, StringComparer.Ordinal)
                ? ShortenLinkAuthorizationResult.Success()
                : ShortenLinkAuthorizationResult.Forbidden();
        }

        var persistedAssignment = await securityAssignmentRepository
            .FindByCredentialKeyHashAsync(apiKeyHash, cancellationToken)
            .ConfigureAwait(false);
        if (persistedAssignment is not null)
        {
            if (!persistedAssignment.IsEnabled)
            {
                return ShortenLinkAuthorizationResult.Unauthorized();
            }

            var persistedPermissions = GetEffectivePermissions(
                persistedAssignment.Roles,
                persistedAssignment.Permissions);

            return persistedPermissions.Contains(permission)
                ? ShortenLinkAuthorizationResult.Success()
                : ShortenLinkAuthorizationResult.Forbidden();
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
        => GetEffectivePermissions(apiKey.Roles, apiKey.Permissions);

    private static HashSet<string> GetEffectivePermissions(
        IEnumerable<string> roles,
        IEnumerable<string> explicitPermissions)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var permission in explicitPermissions.Where(static permission => !string.IsNullOrWhiteSpace(permission)))
        {
            permissions.Add(permission);
        }

        foreach (var role in roles.Where(static role => !string.IsNullOrWhiteSpace(role)))
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

    private static bool HasBearerToken(HttpContext httpContext)
    {
        var authorization = httpContext.Request.Headers.Authorization.FirstOrDefault();
        return !string.IsNullOrWhiteSpace(authorization)
            && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }
}
