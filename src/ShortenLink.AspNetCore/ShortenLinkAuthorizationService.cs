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
    IShortenLinkSecurityRoleRepository roleRepository,
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

            var persistedPermissions = await GetEffectivePermissionsAsync(
                persistedAssignment.Roles,
                persistedAssignment.Permissions,
                cancellationToken).ConfigureAwait(false);

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

        var permissions = await GetEffectivePermissionsAsync(
            principal.Roles,
            principal.Permissions,
            cancellationToken).ConfigureAwait(false);
        return permissions.Contains(permission)
            ? ShortenLinkAuthorizationResult.Success()
            : ShortenLinkAuthorizationResult.Forbidden();
    }

    private async Task<HashSet<string>> GetEffectivePermissionsAsync(
        IEnumerable<string> roles,
        IEnumerable<string> explicitPermissions,
        CancellationToken cancellationToken)
    {
        var permissions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var permission in explicitPermissions.Where(static permission => !string.IsNullOrWhiteSpace(permission)))
        {
            permissions.Add(permission);
        }

        foreach (var role in roles.Where(static role => !string.IsNullOrWhiteSpace(role)))
        {
            var rolePermissions = new HashSet<string>(StringComparer.Ordinal);
            if (ShortenLinkRoles.PermissionBundles.TryGetValue(role, out var systemPermissions))
            {
                rolePermissions.UnionWith(systemPermissions);
            }
            else
            {
                var customRole = await roleRepository
                    .FindCustomRoleAsync(role, cancellationToken)
                    .ConfigureAwait(false);
                if (customRole is not { IsEnabled: true }) continue;
                rolePermissions.UnionWith(customRole.Permissions);
            }

            var overrides = await roleRepository
                .ListPermissionOverridesAsync(role, cancellationToken)
                .ConfigureAwait(false);
            foreach (var item in overrides)
            {
                if (item.IsAllowed) rolePermissions.Add(item.Permission);
                else rolePermissions.Remove(item.Permission);
            }

            permissions.UnionWith(rolePermissions);
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
