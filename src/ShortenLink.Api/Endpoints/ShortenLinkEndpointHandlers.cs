using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShortenLink.AspNetCore;
using ShortenLink.Core;
using ShortenLink.Core.Domain;
using ShortenLink.Core.Security;
using ShortenLink.Core.Services;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ShortenLinkPermissions = ShortenLink.AspNetCore.ShortenLinkPermissions;

namespace ShortenLink.Api.Endpoints;

internal static class ShortenLinkEndpointHandlers
{
    internal static async Task<Results<Ok<SecurityLoginResponse>, JsonHttpResult<ShortLinkErrorResponse>>> LoginSecurityUserAsync(
        SecurityLoginRequest request,
        IShortenLinkUserSessionService userSessionService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = string.IsNullOrWhiteSpace(request.Email)
            ? request.Username
            : request.Email;
        var missingLoginFields = new List<(string Field, string Message)>();
        if (string.IsNullOrWhiteSpace(email))
        {
            missingLoginFields.Add(("email", "Email is required."));
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            missingLoginFields.Add(("password", "Password is required."));
        }
        if (missingLoginFields.Count > 0)
        {
            return CreateErrorResponse(
                "invalid_login",
                "Email or password is invalid.",
                CreateFieldErrors(missingLoginFields));
        }

        var login = await userSessionService
            .LoginAsync(email!, request.Password, cancellationToken)
            .ConfigureAwait(false);
        if (!login.Succeeded
            || login.Principal is null
            || string.IsNullOrWhiteSpace(login.Token)
            || string.IsNullOrWhiteSpace(login.RefreshToken))
        {
            return CreateErrorResponse(login.ErrorCode, login.ErrorMessage);
        }

        return TypedResults.Ok(new SecurityLoginResponse(
            login.Token,
            login.Token,
            login.RefreshToken!,
            SecurityCurrentUserResponse.FromPrincipal(login.Principal)));
    }

    internal static async Task<Results<Ok<SecurityLoginResponse>, JsonHttpResult<ShortLinkErrorResponse>>> RefreshSecurityUserAsync(
        SecurityRefreshRequest request,
        IShortenLinkUserSessionService userSessionService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var refreshed = await userSessionService.RefreshAsync(request.RefreshToken, cancellationToken).ConfigureAwait(false);
        if (!refreshed.Succeeded
            || refreshed.Principal is null
            || string.IsNullOrWhiteSpace(refreshed.Token)
            || string.IsNullOrWhiteSpace(refreshed.RefreshToken))
        {
            return CreateErrorResponse(refreshed.ErrorCode, refreshed.ErrorMessage);
        }

        return TypedResults.Ok(new SecurityLoginResponse(
            refreshed.Token,
            refreshed.Token,
            refreshed.RefreshToken,
            SecurityCurrentUserResponse.FromPrincipal(refreshed.Principal)));
    }

    internal static async Task<Results<Ok<SecurityCurrentUserResponse>, JsonHttpResult<ShortLinkErrorResponse>>> GetCurrentSecurityUserAsync(
        IShortenLinkUserSessionService userSessionService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var session = await userSessionService
            .GetCurrentUserAsync(httpContext, cancellationToken)
            .ConfigureAwait(false);
        if (!session.Succeeded || session.Principal is null)
        {
            return CreateErrorResponse(session.ErrorCode, session.ErrorMessage);
        }

        return TypedResults.Ok(SecurityCurrentUserResponse.FromPrincipal(session.Principal));
    }

    internal static async Task<Results<Ok<SecurityUserApiKeysListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListCurrentUserApiKeysAsync(
        IShortenLinkUserSessionService userSessionService,
        IShortenLinkUserApiKeyRepository apiKeyRepository,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var session = await userSessionService
            .GetCurrentUserAsync(httpContext, cancellationToken)
            .ConfigureAwait(false);
        if (!session.Succeeded || session.Principal is null)
        {
            return CreateErrorResponse(session.ErrorCode, session.ErrorMessage);
        }

        var apiKeys = await apiKeyRepository
            .ListByUserIdAsync(session.Principal.UserId, cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(new SecurityUserApiKeysListResponse(
            apiKeys.Select(SecurityUserApiKeyResponse.FromDomain).ToList()));
    }

    internal static async Task<Results<Ok<SecurityUserApiKeyCreatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> CreateCurrentUserApiKeyAsync(
        SecurityUserApiKeyCreateRequest request,
        IShortenLinkUserSessionService userSessionService,
        IShortenLinkUserApiKeyRepository apiKeyRepository,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await userSessionService
            .GetCurrentUserAsync(httpContext, cancellationToken)
            .ConfigureAwait(false);
        if (!session.Succeeded || session.Principal is null)
        {
            return CreateErrorResponse(session.ErrorCode, session.ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return CreateFieldErrorResponse("invalid_api_key", "API key display name is required.", "displayName");
        }

        var rawApiKey = CreateRawUserApiKey();
        var apiKey = new ShortenLinkUserApiKey(
            Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture),
            session.Principal.UserId,
            request.DisplayName.Trim(),
            ShortenLinkSecurityCredentialHasher.HashApiKey(rawApiKey),
            isEnabled: true,
            timeProvider.GetUtcNow());

        await apiKeyRepository.AddOrUpdateAsync(apiKey, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(new SecurityUserApiKeyCreatedResponse(
            SecurityUserApiKeyResponse.FromDomain(apiKey),
            rawApiKey));
    }

    internal static async Task<Results<Ok<SecurityUserApiKeyResponse>, JsonHttpResult<ShortLinkErrorResponse>>> RenameCurrentUserApiKeyAsync(
        string id,
        SecurityUserApiKeyRenameRequest request,
        IShortenLinkUserSessionService userSessionService,
        IShortenLinkUserApiKeyRepository apiKeyRepository,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await userSessionService
            .GetCurrentUserAsync(httpContext, cancellationToken)
            .ConfigureAwait(false);
        if (!session.Succeeded || session.Principal is null)
        {
            return CreateErrorResponse(session.ErrorCode, session.ErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return CreateFieldErrorResponse("invalid_api_key", "API key display name is required.", "displayName");
        }

        var apiKey = await apiKeyRepository.FindByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (apiKey is null || !apiKey.UserId.Equals(session.Principal.UserId, StringComparison.Ordinal))
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "API key was not found.");
        }

        var renamed = new ShortenLinkUserApiKey(
            apiKey.ApiKeyKey,
            apiKey.UserId,
            request.DisplayName.Trim(),
            apiKey.KeyHash,
            apiKey.IsEnabled,
            apiKey.CreatedAt);

        await apiKeyRepository.AddOrUpdateAsync(renamed, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(SecurityUserApiKeyResponse.FromDomain(renamed));
    }

    internal static async Task<Results<Ok<SecurityUserApiKeyDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableCurrentUserApiKeyAsync(
        string id,
        IShortenLinkUserSessionService userSessionService,
        IShortenLinkUserApiKeyRepository apiKeyRepository,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var session = await userSessionService
            .GetCurrentUserAsync(httpContext, cancellationToken)
            .ConfigureAwait(false);
        if (!session.Succeeded || session.Principal is null)
        {
            return CreateErrorResponse(session.ErrorCode, session.ErrorMessage);
        }

        var apiKey = await apiKeyRepository.FindByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (apiKey is null || !apiKey.UserId.Equals(session.Principal.UserId, StringComparison.Ordinal))
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "API key was not found.");
        }

        var disabled = await apiKeyRepository.DisableAsync(id, cancellationToken).ConfigureAwait(false);
        if (!disabled)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "API key was not found.");
        }

        return TypedResults.Ok(new SecurityUserApiKeyDisabledResponse(id, false));
    }

    internal static async Task<Results<Ok<SecurityRolesListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListSecurityRolesAsync(
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var systemRoles = new List<SecurityRoleResponse>();
        foreach (var role in ShortenLinkSystemRoles.PermissionBundles.OrderBy(role => role.Key, StringComparer.OrdinalIgnoreCase))
        {
            var overrides = await roleRepository.ListPermissionOverridesAsync(role.Key, cancellationToken).ConfigureAwait(false);
            systemRoles.Add(SecurityRoleResponse.System(role.Key, role.Value, overrides));
        }
        var customRoles = await roleRepository.ListCustomRolesAsync(cancellationToken).ConfigureAwait(false);
        var customRoleResponses = new List<SecurityRoleResponse>();
        foreach (var role in customRoles)
        {
            var overrides = await roleRepository.ListPermissionOverridesAsync(role.RoleKey, cancellationToken).ConfigureAwait(false);
            customRoleResponses.Add(SecurityRoleResponse.Custom(role, overrides));
        }

        return TypedResults.Ok(new SecurityRolesListResponse(
            systemRoles,
            customRoleResponses));
    }

    internal static async Task<Results<Ok<SecurityRoleResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpsertCustomSecurityRoleAsync(
        SecurityCustomRoleUpsertRequest request,
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var validation = ValidateCustomRoleRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        var role = new ShortenLinkCustomRole(
            request.Id.Trim(),
            request.Name.Trim(),
            NormalizeDistinct(request.Permissions),
            request.IsEnabled ?? true,
            timeProvider.GetUtcNow());

        await roleRepository.AddOrUpdateCustomRoleAsync(role, cancellationToken).ConfigureAwait(false);
        var overrides = await roleRepository.ListPermissionOverridesAsync(role.RoleKey, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(SecurityRoleResponse.Custom(role, overrides));
    }

    internal static async Task<Results<Ok<SecurityRoleResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ReplaceSecurityRolePermissionOverridesAsync(
        string id,
        SecurityRolePermissionOverridesRequest request,
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var roleId = id.Trim();
        var isSystem = ShortenLinkSystemRoles.PermissionBundles.TryGetValue(roleId, out var systemDefaults);
        var customRole = isSystem ? null : await roleRepository.FindCustomRoleAsync(roleId, cancellationToken).ConfigureAwait(false);
        if (!isSystem && customRole is null)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Security role was not found.");
        }

        var normalized = new List<ShortenLinkRolePermissionOverride>();
        foreach (var item in request.Overrides ?? [])
        {
            if (!ShortenLinkPermissions.All.Contains(item.Permission))
            {
                return CreateFieldErrorResponse("invalid_permission", $"Unknown permission '{item.Permission}'.", "overrides");
            }

            if (normalized.Any(existing => existing.Permission == item.Permission))
            {
                return CreateFieldErrorResponse("duplicate_permission", $"Permission '{item.Permission}' has more than one override.", "overrides");
            }

            normalized.Add(ShortenLinkRolePermissionOverride.Create(item.Permission, item.IsAllowed));
        }

        await roleRepository.ReplacePermissionOverridesAsync(roleId, normalized, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(isSystem
            ? SecurityRoleResponse.System(roleId, systemDefaults!, normalized)
            : SecurityRoleResponse.Custom(customRole!, normalized));
    }

    internal static async Task<Results<Ok<SecurityRoleDeletedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DeleteCustomSecurityRoleAsync(
        string id,
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        if (ShortenLinkSystemRoles.PermissionBundles.ContainsKey(id))
        {
            return CreateErrorResponse("system_role_immutable", "System roles cannot be deleted.");
        }

        var role = await roleRepository.FindCustomRoleAsync(id, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Custom role was not found.");
        }

        var users = await userRepository.ListAsync(includeHidden: true, cancellationToken).ConfigureAwait(false);
        var assignedUserCount = users.Count(user =>
            user.RoleIds.Contains(id, StringComparer.OrdinalIgnoreCase));
        if (assignedUserCount > 0)
        {
            return CreateErrorResponse(
                "role_in_use",
                $"Role is assigned to {assignedUserCount} user(s). Remove or replace the role on those users before deleting it.");
        }

        var deleted = await roleRepository.DeleteCustomRoleAsync(id, cancellationToken).ConfigureAwait(false);
        if (!deleted)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Custom role was not found.");
        }

        return TypedResults.Ok(new SecurityRoleDeletedResponse(id));
    }

    internal static async Task<Results<Ok<SecurityUsersListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListSecurityUsersAsync(
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var users = await userRepository.ListAsync(includeHidden: false, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(new SecurityUsersListResponse(
            users.Select(SecurityUserResponse.FromDomain).ToList()));
    }

    internal static async Task<Results<Ok<SecurityUserResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpsertSecurityUserAsync(
        SecurityUserUpsertRequest request,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var validation = await ValidateSecurityUserRequestAsync(
                request,
                userRepository,
                roleRepository,
                cancellationToken)
            .ConfigureAwait(false);
        if (validation is not null)
        {
            return validation;
        }

        var existing = await userRepository
            .FindByIdAsync(request.Id.Trim(), cancellationToken)
            .ConfigureAwait(false);
        var passwordHash = !string.IsNullOrWhiteSpace(request.Password)
            ? ShortenLinkSecurityCredentialHasher.HashPassword(request.Password)
            : existing?.PasswordHash ?? ShortenLinkSecurityCredentialHasher.PasswordNotSetHash;
        var roleIds = NormalizeDistinct(request.RoleIds);
        if (existing is null && roleIds.Count == 0)
        {
            roleIds = new[] { ShortenLinkRoles.User };
        }

        var user = new ShortenLinkSecurityUser(
            request.Id.Trim(),
            request.Username.Trim(),
            request.DisplayName.Trim(),
            passwordHash,
            roleIds,
            request.IsEnabled ?? true,
            isHidden: false,
            isBootstrap: false,
            existing?.CreatedAt ?? timeProvider.GetUtcNow());

        await userRepository.AddOrUpdateAsync(user, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(SecurityUserResponse.FromDomain(user));
    }

    internal static async Task<Results<Ok<SecurityUserDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableSecurityUserAsync(
        string id,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var existing = await userRepository.FindByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (existing is { IsBootstrap: true })
        {
            return CreateErrorResponse("bootstrap_user_immutable", "The bootstrap admin user cannot be disabled.");
        }

        var disabled = await userRepository.DisableAsync(id, cancellationToken).ConfigureAwait(false);
        if (!disabled)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Security user was not found.");
        }

        return TypedResults.Ok(new SecurityUserDisabledResponse(id, false));
    }

    internal static async Task<Results<Ok<ShortLinkAdminListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListShortLinksAsync(
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        int? limit,
        int? page,
        string? cursor,
        string? search,
        string? status,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksRead, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }
        var accessScope = await CreateAccessScopeAsync(
            authorization,
            shareRepository,
            cancellationToken).ConfigureAwait(false);

        var safeLimit = Math.Clamp(limit ?? 100, 1, 500);
        var hasListQuery = page is not null
            || !string.IsNullOrWhiteSpace(search)
            || !string.IsNullOrWhiteSpace(status)
            || !string.IsNullOrWhiteSpace(sortBy)
            || !string.IsNullOrWhiteSpace(sortDirection);
        if (hasListQuery)
        {
            if (!TryParseListStatus(status, out var parsedStatus))
            {
                return CreateErrorResponse("invalid_filter", "Status filter is invalid.");
            }

            if (!TryParseListSortBy(sortBy, out var parsedSortBy))
            {
                return CreateErrorResponse("invalid_sort", "Sort field is invalid.");
            }

            if (!TryParseSortDirection(sortDirection, out var parsedSortDirection))
            {
                return CreateErrorResponse("invalid_sort_direction", "Sort direction is invalid.");
            }

            var safePage = Math.Max(page ?? 1, 1);
            var numberedPage = await shortLinkService.ListAccessiblePageAsync(
                    (safePage - 1) * safeLimit,
                    safeLimit,
                    search,
                    parsedStatus,
                    parsedSortBy,
                    parsedSortDirection,
                    accessScope,
                    cancellationToken)
                .ConfigureAwait(false);
            var pageResponse = numberedPage.Items
                .Select(shortLink => ShortLinkAdminListItemResponse.FromDomain(
                    shortLink,
                    BuildShortUrl(shortLink.Code, options.Value, httpContext),
                    GetAccessLevel(shortLink, accessScope)))
                .ToList();
            var totalPages = Math.Max(1, (int)Math.Ceiling(numberedPage.TotalCount / (double)safeLimit));

            return TypedResults.Ok(new ShortLinkAdminListResponse(
                pageResponse,
                null,
                numberedPage.TotalCount,
                safePage,
                safeLimit,
                totalPages));
        }

        var cursorResult = TryDecodeCursor(cursor, out var beforeCreatedAt, out var beforeCode);
        if (!cursorResult)
        {
            return CreateErrorResponse("invalid_cursor", "Cursor is invalid.");
        }

        var shortLinks = await shortLinkService.ListAccessibleRecentAsync(
                safeLimit + 1,
                beforeCreatedAt,
                beforeCode,
                accessScope,
                cancellationToken)
            .ConfigureAwait(false);
        var pageItems = shortLinks.Take(safeLimit).ToList();
        var response = pageItems
            .Select(shortLink => ShortLinkAdminListItemResponse.FromDomain(
                shortLink,
                BuildShortUrl(shortLink.Code, options.Value, httpContext),
                GetAccessLevel(shortLink, accessScope)))
            .ToList();
        var nextCursor = shortLinks.Count > safeLimit
            ? EncodeCursor(pageItems[^1].CreatedAt, pageItems[^1].Code)
            : null;

        return TypedResults.Ok(new ShortLinkAdminListResponse(response, nextCursor));
    }

    private static bool TryParseListStatus(string? value, out ShortLinkListStatus status)
    {
        status = ShortLinkListStatus.All;
        if (string.IsNullOrWhiteSpace(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("active", StringComparison.OrdinalIgnoreCase))
        {
            status = ShortLinkListStatus.Active;
            return true;
        }

        if (value.Equals("inactive", StringComparison.OrdinalIgnoreCase))
        {
            status = ShortLinkListStatus.Inactive;
            return true;
        }

        if (value.Equals("expired", StringComparison.OrdinalIgnoreCase))
        {
            status = ShortLinkListStatus.Expired;
            return true;
        }

        if (value.Equals("expiring-soon", StringComparison.OrdinalIgnoreCase))
        {
            status = ShortLinkListStatus.ExpiringSoon;
            return true;
        }

        return false;
    }

    private static bool TryParseListSortBy(string? value, out ShortLinkListSortBy sortBy)
    {
        sortBy = ShortLinkListSortBy.Created;
        if (string.IsNullOrWhiteSpace(value) || value.Equals("created", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("expiry", StringComparison.OrdinalIgnoreCase))
        {
            sortBy = ShortLinkListSortBy.Expiry;
            return true;
        }

        if (value.Equals("destination", StringComparison.OrdinalIgnoreCase))
        {
            sortBy = ShortLinkListSortBy.Destination;
            return true;
        }

        if (value.Equals("code", StringComparison.OrdinalIgnoreCase))
        {
            sortBy = ShortLinkListSortBy.Code;
            return true;
        }

        if (value.Equals("status", StringComparison.OrdinalIgnoreCase))
        {
            sortBy = ShortLinkListSortBy.Status;
            return true;
        }

        return false;
    }

    private static bool TryParseSortDirection(string? value, out ShortLinkSortDirection sortDirection)
    {
        sortDirection = ShortLinkSortDirection.Desc;
        if (string.IsNullOrWhiteSpace(value) || value.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("asc", StringComparison.OrdinalIgnoreCase))
        {
            sortDirection = ShortLinkSortDirection.Asc;
            return true;
        }

        return false;
    }

    internal static async Task<Results<Created<ShortLinkCreatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> CreateShortLinkAsync(
        ShortLinkCreateRequest request,
        IShortLinkService shortLinkService,
        IShortenLinkAuthorizationService authorizationService,
        IShortenLinkUserSessionService userSessionService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksCreate, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var currentSession = await userSessionService
            .GetCurrentUserAsync(httpContext, cancellationToken)
            .ConfigureAwait(false);
        var creator = currentSession.Succeeded ? currentSession.Principal : null;

        var result = await shortLinkService.CreateAsync(
            new CreateShortLinkRequest(
                request.OriginalUrl,
                request.ExpiredAtUtc,
                creator?.UserId,
                creator?.DisplayName,
                creator?.Username),
            cancellationToken).ConfigureAwait(false);

        if (!result.Succeeded || result.ShortLink is null)
        {
            return CreateErrorResponse(
                result.ErrorCode,
                result.ErrorMessage,
                GetShortLinkValidationFieldErrors(result.ErrorCode, result.ErrorMessage));
        }

        var response = ShortLinkCreatedResponse.FromDomain(
            result.ShortLink,
            BuildShortUrl(result.ShortLink.Code, options.Value, httpContext));

        return TypedResults.Created($"/api/short-links/{result.ShortLink.Code}", response);
    }

    internal static async Task<Results<Ok<ShortLinkDetailsResponse>, JsonHttpResult<ShortLinkErrorResponse>>> GetShortLinkDetailsAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksRead, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }
        var result = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.ShortLink is null)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }
        if (!await CanAccessShortLinkAsync(
                result.ShortLink,
                authorization,
                shareRepository,
                ShortLinkShareAccess.View,
                false,
                cancellationToken).ConfigureAwait(false))
        {
            return CreateErrorResponse("forbidden", "You do not have access to this short link.");
        }

        return TypedResults.Ok(ShortLinkDetailsResponse.FromDomain(result.ShortLink));
    }

    internal static async Task<Results<Ok<ShortLinkAnalyticsResponse>, JsonHttpResult<ShortLinkErrorResponse>>> GetShortLinkAnalyticsAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkClickRepository clickRepository,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        int? limit,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AnalyticsRead, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var details = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!details.Succeeded || details.ShortLink is null)
        {
            return CreateErrorResponse(details.ErrorCode, details.ErrorMessage);
        }
        if (!await CanAccessShortLinkAsync(
                details.ShortLink,
                authorization,
                shareRepository,
                ShortLinkShareAccess.View,
                false,
                cancellationToken).ConfigureAwait(false))
        {
            return CreateErrorResponse("forbidden", "You do not have access to this short link.");
        }

        var safeLimit = Math.Clamp(limit ?? 20, 1, 100);
        var summary = await clickRepository.GetSummaryAsync(code, cancellationToken).ConfigureAwait(false);
        var recentClicks = await clickRepository.ListRecentAsync(code, safeLimit, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(ShortLinkAnalyticsResponse.FromClicks(
            code,
            summary,
            recentClicks));
    }

    internal static async Task<IResult> ListShortLinkSharesAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksRead, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded) return CreateAuthorizationErrorResponse(authorization);
        var details = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!details.Succeeded || details.ShortLink is null)
            return CreateErrorResponse(details.ErrorCode, details.ErrorMessage);
        if (!await CanAccessShortLinkAsync(
                details.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, true, cancellationToken).ConfigureAwait(false))
            return CreateErrorResponse("forbidden", "Only the owner or an admin can manage sharing.");

        var shares = await shareRepository.ListByShortCodeAsync(code, cancellationToken).ConfigureAwait(false);
        var response = new List<ShortLinkShareResponse>(shares.Count);
        foreach (var share in shares)
        {
            var user = await userRepository.FindByIdAsync(share.UserId, cancellationToken).ConfigureAwait(false);
            response.Add(ShortLinkShareResponse.FromDomain(share, user));
        }
        return TypedResults.Ok(new ShortLinkSharesResponse(response));
    }

    internal static async Task<IResult> UpsertShortLinkShareAsync(
        string code,
        ShortLinkShareUpsertRequest request,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksUpdate, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded) return CreateAuthorizationErrorResponse(authorization);
        var details = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!details.Succeeded || details.ShortLink is null)
            return CreateErrorResponse(details.ErrorCode, details.ErrorMessage);
        if (!await CanAccessShortLinkAsync(
                details.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, true, cancellationToken).ConfigureAwait(false))
            return CreateErrorResponse("forbidden", "Only the owner or an admin can manage sharing.");
        if (string.IsNullOrWhiteSpace(request.Username)
            || !Enum.TryParse<ShortLinkShareAccess>(request.Access, true, out var access))
            return CreateErrorResponse("invalid_share", "Choose a user and View or Edit access.");
        var targetUser = await userRepository
            .FindByUsernameAsync(request.Username, cancellationToken)
            .ConfigureAwait(false);
        if (targetUser is not { IsEnabled: true })
            return CreateErrorResponse("invalid_share_user", "The selected user is unavailable.");
        if (string.Equals(details.ShortLink.CreatedByUserId, targetUser.UserKey, StringComparison.Ordinal))
            return CreateErrorResponse("invalid_share", "The owner already has full access.");

        var share = new ShortLinkShare(
            code,
            targetUser.UserKey,
            access,
            authorization.UserId ?? "system",
            timeProvider.GetUtcNow());
        await shareRepository.AddOrUpdateAsync(share, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(ShortLinkShareResponse.FromDomain(share, targetUser));
    }

    internal static async Task<IResult> DeleteShortLinkShareAsync(
        string code,
        string userId,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksUpdate, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded) return CreateAuthorizationErrorResponse(authorization);
        var details = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!details.Succeeded || details.ShortLink is null)
            return CreateErrorResponse(details.ErrorCode, details.ErrorMessage);
        if (!await CanAccessShortLinkAsync(
                details.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, true, cancellationToken).ConfigureAwait(false))
            return CreateErrorResponse("forbidden", "Only the owner or an admin can manage sharing.");

        return await shareRepository.DeleteAsync(code, userId, cancellationToken).ConfigureAwait(false)
            ? TypedResults.NoContent()
            : CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Share was not found.");
    }

    internal static async Task<Results<Ok<SecurityAssignmentsListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListSecurityAssignmentsAsync(
        IShortenLinkSecurityAssignmentRepository assignmentRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var assignments = await assignmentRepository.ListAsync(cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(new SecurityAssignmentsListResponse(
            assignments.Select(SecurityAssignmentResponse.FromDomain).ToList()));
    }

    internal static async Task<Results<Ok<SecurityAssignmentResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpsertSecurityAssignmentAsync(
        SecurityAssignmentUpsertRequest request,
        IShortenLinkSecurityAssignmentRepository assignmentRepository,
        IShortenLinkAuthorizationService authorizationService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var validation = ValidateSecurityAssignmentRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        var assignment = new ShortenLinkSecurityAssignment(
            HashCredential(request.CredentialKey),
            request.Name?.Trim() ?? string.Empty,
            NormalizeDistinct(request.Roles),
            NormalizeDistinct(request.Permissions),
            request.IsEnabled ?? true,
            timeProvider.GetUtcNow());

        await assignmentRepository.AddOrUpdateAsync(assignment, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(SecurityAssignmentResponse.FromDomain(assignment));
    }

    internal static async Task<Results<Ok<SecurityAssignmentDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableSecurityAssignmentAsync(
        string credentialKeyHash,
        IShortenLinkSecurityAssignmentRepository assignmentRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.AdminOnly, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        if (!IsValidCredentialHash(credentialKeyHash))
        {
            return CreateErrorResponse("invalid_credential_hash", "Credential key hash is invalid.");
        }

        var disabled = await assignmentRepository.DisableAsync(credentialKeyHash, cancellationToken).ConfigureAwait(false);
        if (!disabled)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Security assignment was not found.");
        }

        return TypedResults.Ok(new SecurityAssignmentDisabledResponse(credentialKeyHash, false));
    }

    internal static async Task<Results<Ok<ShortLinkAdminListItemResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpdateShortLinkAsync(
        string code,
        ShortLinkUpdateRequest request,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksUpdate, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }
        var existing = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!existing.Succeeded || existing.ShortLink is null)
        {
            return CreateErrorResponse(existing.ErrorCode, existing.ErrorMessage);
        }
        if (!await CanAccessShortLinkAsync(
                existing.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, false, cancellationToken).ConfigureAwait(false))
        {
            return CreateErrorResponse("forbidden", "Edit access is required for this short link.");
        }

        var result = await shortLinkService.UpdateAsync(
            code,
            new UpdateShortLinkRequest(request.OriginalUrl, request.ExpiredAtUtc),
            cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.ShortLink is null)
        {
            return CreateErrorResponse(
                result.ErrorCode,
                result.ErrorMessage,
                GetShortLinkValidationFieldErrors(result.ErrorCode, result.ErrorMessage));
        }

        return TypedResults.Ok(ShortLinkAdminListItemResponse.FromDomain(
            result.ShortLink,
            BuildShortUrl(result.ShortLink.Code, options.Value, httpContext)));
    }

    internal static async Task<Results<Ok<ShortLinkDeactivatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DeactivateShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksStatus, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }
        var existing = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!existing.Succeeded || existing.ShortLink is null)
        {
            return CreateErrorResponse(existing.ErrorCode, existing.ErrorMessage);
        }
        if (!await CanAccessShortLinkAsync(
                existing.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, false, cancellationToken).ConfigureAwait(false))
        {
            return CreateErrorResponse("forbidden", "Edit access is required for this short link.");
        }

        var result = await shortLinkService.DeactivateAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(new ShortLinkDeactivatedResponse(code, false));
    }

    internal static async Task<Results<Ok<ShortLinkDeactivatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ActivateShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksStatus, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }
        var existing = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!existing.Succeeded || existing.ShortLink is null)
        {
            return CreateErrorResponse(existing.ErrorCode, existing.ErrorMessage);
        }
        if (!await CanAccessShortLinkAsync(
                existing.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, false, cancellationToken).ConfigureAwait(false))
        {
            return CreateErrorResponse("forbidden", "Edit access is required for this short link.");
        }

        var result = await shortLinkService.ActivateAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(new ShortLinkDeactivatedResponse(code, true));
    }

    internal static async Task<Results<Ok<ShortLinkDeletedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DeleteShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkShareRepository shareRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksDelete, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }
        var existing = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!existing.Succeeded || existing.ShortLink is null)
        {
            return CreateErrorResponse(existing.ErrorCode, existing.ErrorMessage);
        }
        if (!await CanAccessShortLinkAsync(
                existing.ShortLink, authorization, shareRepository,
                ShortLinkShareAccess.Edit, true, cancellationToken).ConfigureAwait(false))
        {
            return CreateErrorResponse("forbidden", "Only the owner or an admin can delete this short link.");
        }

        var result = await shortLinkService.DeleteAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }
        await shareRepository.DeleteByShortCodeAsync(code, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(new ShortLinkDeletedResponse(code));
    }

    internal static async Task<IResult> RedirectShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkClickRecorder shortLinkClickRecorder,
        IOptions<ShortenLinkOptions> options,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await shortLinkService.ResolveAsync(code, cancellationToken).ConfigureAwait(false);
        if (result.Succeeded && result.ShortLink is not null)
        {
            await shortLinkClickRecorder.RecordAsync(
                new RecordShortLinkClickRequest(
                    result.ShortLink.Code,
                    timeProvider.GetUtcNow(),
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    httpContext.Request.Headers.UserAgent.ToString(),
                    httpContext.Request.Headers.Referer.ToString()),
                cancellationToken).ConfigureAwait(false);

            return TypedResults.Redirect(result.ShortLink.OriginalUrl.AbsoluteUri);
        }

        if (ShouldUseFallback(options.Value, result.ErrorCode))
        {
            return TypedResults.Redirect(NormalizeFallbackPath(options.Value.Redirect.FrontendFallbackPath));
        }

        return CreateErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    private static JsonHttpResult<ShortLinkErrorResponse> CreateErrorResponse(
        string? errorCode,
        string? errorMessage,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? fieldErrors = null)
    {
        var response = new ShortLinkErrorResponse(
            errorCode ?? "unknown_error",
            errorMessage ?? "An unexpected short-link error occurred.",
            fieldErrors);

        return TypedResults.Json(response, statusCode: GetStatusCode(response.ErrorCode));
    }

    private static JsonHttpResult<ShortLinkErrorResponse> CreateAuthorizationErrorResponse(
        ShortenLinkAuthorizationResult authorization)
    {
        var response = new ShortLinkErrorResponse(
            authorization.ErrorCode ?? "forbidden",
            authorization.ErrorMessage ?? "The request is not authorized.",
            null);

        return TypedResults.Json(
            response,
            statusCode: authorization.IsAuthenticated
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status401Unauthorized);
    }

    private static async Task<ShortLinkAccessScope> CreateAccessScopeAsync(
        ShortenLinkAuthorizationResult authorization,
        IShortLinkShareRepository shareRepository,
        CancellationToken cancellationToken)
    {
        var sharedAccess = string.IsNullOrWhiteSpace(authorization.UserId)
            ? new Dictionary<string, ShortLinkShareAccess>(StringComparer.Ordinal)
            : await shareRepository
                .ListSharedAccessAsync(authorization.UserId, cancellationToken)
                .ConfigureAwait(false);
        return new ShortLinkAccessScope(
            authorization.UserId,
            authorization.IsAdmin,
            sharedAccess);
    }

    private static async Task<bool> CanAccessShortLinkAsync(
        ShortLink shortLink,
        ShortenLinkAuthorizationResult authorization,
        IShortLinkShareRepository shareRepository,
        ShortLinkShareAccess requiredAccess,
        bool ownerOnly,
        CancellationToken cancellationToken)
    {
        if (authorization.IsAdmin)
        {
            return true;
        }
        if (!string.IsNullOrWhiteSpace(authorization.UserId)
            && string.Equals(shortLink.CreatedByUserId, authorization.UserId, StringComparison.Ordinal))
        {
            return true;
        }
        if (ownerOnly || string.IsNullOrWhiteSpace(authorization.UserId))
        {
            return false;
        }
        var share = await shareRepository
            .FindAsync(shortLink.Code, authorization.UserId, cancellationToken)
            .ConfigureAwait(false);
        return share is not null && share.Access >= requiredAccess;
    }

    private static string GetAccessLevel(
        ShortLink shortLink,
        ShortLinkAccessScope accessScope)
    {
        if (accessScope.IsAdmin) return "Admin";
        if (!string.IsNullOrWhiteSpace(accessScope.UserId)
            && string.Equals(shortLink.CreatedByUserId, accessScope.UserId, StringComparison.Ordinal))
            return "Owner";
        return accessScope.SharedAccess.TryGetValue(shortLink.Code, out var access)
            ? access.ToString()
            : "None";
    }

    private static IResult CreateErrorResult(string? errorCode, string? errorMessage) =>
        CreateErrorResponse(errorCode, errorMessage);

    private static JsonHttpResult<ShortLinkErrorResponse> CreateFieldErrorResponse(
        string errorCode,
        string errorMessage,
        string field) =>
        CreateErrorResponse(
            errorCode,
            errorMessage,
            CreateFieldErrors(new[] { (field, errorMessage) }));

    private static IReadOnlyDictionary<string, IReadOnlyList<string>>? GetShortLinkValidationFieldErrors(
        string? errorCode,
        string? errorMessage)
    {
        var field = errorCode switch
        {
            ShortLinkErrorCodes.InvalidUrl => "originalUrl",
            ShortLinkErrorCodes.InvalidExpiration => "expiredAtUtc",
            _ => null
        };

        return field is null
            ? null
            : CreateFieldErrors(new[] { (field, errorMessage ?? "The value is invalid.") });
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> CreateFieldErrors(
        IEnumerable<(string Field, string Message)> errors) =>
        errors
            .GroupBy(static error => error.Field, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => (IReadOnlyList<string>)group
                    .Select(static error => error.Message)
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                StringComparer.Ordinal);

    private static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            ShortLinkErrorCodes.InvalidCode => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidExpiration => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidUrl => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.NotFound => StatusCodes.Status404NotFound,
            "invalid_api_key" => StatusCodes.Status400BadRequest,
            "invalid_credential_hash" => StatusCodes.Status400BadRequest,
            "invalid_cursor" => StatusCodes.Status400BadRequest,
            "invalid_filter" => StatusCodes.Status400BadRequest,
            "invalid_login" => StatusCodes.Status401Unauthorized,
            "invalid_permission" => StatusCodes.Status400BadRequest,
            "invalid_role" => StatusCodes.Status400BadRequest,
            "invalid_sort" => StatusCodes.Status400BadRequest,
            "invalid_sort_direction" => StatusCodes.Status400BadRequest,
            "invalid_security_role" => StatusCodes.Status400BadRequest,
            "invalid_security_assignment" => StatusCodes.Status400BadRequest,
            "invalid_security_user" => StatusCodes.Status400BadRequest,
            "role_in_use" => StatusCodes.Status409Conflict,
            "system_role_immutable" => StatusCodes.Status400BadRequest,
            "bootstrap_user_immutable" => StatusCodes.Status400BadRequest,
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "forbidden" => StatusCodes.Status403Forbidden,
            "invalid_share" => StatusCodes.Status400BadRequest,
            "invalid_share_user" => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.Expired => StatusCodes.Status410Gone,
            ShortLinkErrorCodes.Inactive => StatusCodes.Status410Gone,
            _ => StatusCodes.Status500InternalServerError
        };

    private static bool TryDecodeCursor(
        string? cursor,
        out DateTimeOffset? beforeCreatedAt,
        out string? beforeCode)
    {
        beforeCreatedAt = null;
        beforeCode = null;
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return true;
        }

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = decoded.Split('|', 2);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return false;
            }

            if (DateTimeOffset.TryParseExact(
                parts[0],
                "O",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
            {
                beforeCreatedAt = parsed;
                beforeCode = parts[1];
                return true;
            }
        }
        catch (FormatException)
        {
        }

        return false;
    }

    private static string EncodeCursor(DateTimeOffset beforeCreatedAt, string beforeCode) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $"{beforeCreatedAt.ToString("O", CultureInfo.InvariantCulture)}|{beforeCode}"));

    private static bool ShouldUseFallback(ShortenLinkOptions options, string? errorCode) =>
        options.Redirect.EnableFrontendFallback
        && errorCode is ShortLinkErrorCodes.NotFound or ShortLinkErrorCodes.Expired or ShortLinkErrorCodes.Inactive;

    private static string BuildShortUrl(string code, ShortenLinkOptions options, HttpContext httpContext)
    {
        if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var configuredBaseUrl))
        {
            return new Uri(configuredBaseUrl, code).AbsoluteUri;
        }

        var requestBaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/";
        return new Uri(new Uri(requestBaseUrl, UriKind.Absolute), code).AbsoluteUri;
    }

    private static string NormalizeFallbackPath(string? fallbackPath) =>
        string.IsNullOrWhiteSpace(fallbackPath) ? "/not-found" : fallbackPath;

    private static JsonHttpResult<ShortLinkErrorResponse>? ValidateSecurityAssignmentRequest(
        SecurityAssignmentUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CredentialKey))
        {
            return CreateFieldErrorResponse("invalid_security_assignment", "Credential key is required.", "credentialKey");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return CreateFieldErrorResponse("invalid_security_assignment", "Assignment name is required.", "name");
        }

        foreach (var role in NormalizeDistinct(request.Roles))
        {
            if (!ShortenLinkRoles.PermissionBundles.ContainsKey(role))
            {
                return CreateFieldErrorResponse("invalid_role", $"Unknown system role '{role}'.", "roles");
            }
        }

        foreach (var permission in NormalizeDistinct(request.Permissions))
        {
            if (!ShortenLinkPermissions.All.Contains(permission))
            {
                return CreateFieldErrorResponse("invalid_permission", $"Unknown permission '{permission}'.", "permissions");
            }
        }

        return null;
    }

    private static JsonHttpResult<ShortLinkErrorResponse>? ValidateCustomRoleRequest(
        SecurityCustomRoleUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return CreateFieldErrorResponse("invalid_security_role", "Custom role id is required.", "id");
        }

        if (ShortenLinkSystemRoles.PermissionBundles.ContainsKey(request.Id.Trim()))
        {
            return CreateErrorResponse("system_role_immutable", "System roles cannot be created or updated through custom role APIs.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return CreateFieldErrorResponse("invalid_security_role", "Custom role name is required.", "name");
        }

        foreach (var permission in NormalizeDistinct(request.Permissions))
        {
            if (!ShortenLinkPermissions.All.Contains(permission))
            {
                return CreateFieldErrorResponse("invalid_permission", $"Unknown permission '{permission}'.", "permissions");
            }
        }

        return null;
    }

    private static async Task<JsonHttpResult<ShortLinkErrorResponse>?> ValidateSecurityUserRequestAsync(
        SecurityUserUpsertRequest request,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkSecurityRoleRepository roleRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return CreateFieldErrorResponse("invalid_security_user", "User id is required.", "id");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return CreateFieldErrorResponse("invalid_security_user", "Username is required.", "username");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return CreateFieldErrorResponse("invalid_security_user", "Display name is required.", "displayName");
        }

        var existing = await userRepository
            .FindByIdAsync(request.Id.Trim(), cancellationToken)
            .ConfigureAwait(false);
        if (existing is { IsBootstrap: true })
        {
            return CreateErrorResponse("bootstrap_user_immutable", "The bootstrap admin user cannot be updated through user management APIs.");
        }

        var usernameOwner = await userRepository
            .FindByUsernameAsync(request.Username.Trim(), cancellationToken)
            .ConfigureAwait(false);
        if (usernameOwner is not null && !usernameOwner.UserKey.Equals(request.Id.Trim(), StringComparison.Ordinal))
        {
            return CreateFieldErrorResponse("invalid_security_user", "Username is already assigned to another user.", "username");
        }

        foreach (var roleId in NormalizeDistinct(request.RoleIds))
        {
            if (ShortenLinkSystemRoles.PermissionBundles.ContainsKey(roleId))
            {
                continue;
            }

            var customRole = await roleRepository.FindCustomRoleAsync(roleId, cancellationToken).ConfigureAwait(false);
            if (customRole is null)
            {
                return CreateFieldErrorResponse("invalid_role", $"Unknown role '{roleId}'.", "roleIds");
            }
        }

        return null;
    }

    private static IReadOnlyList<string> NormalizeDistinct(IEnumerable<string>? values) =>
        (values ?? Array.Empty<string>())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static string HashCredential(string apiKey) =>
        ShortenLinkSecurityCredentialHasher.HashApiKey(apiKey);

    private static string CreateRawUserApiKey() =>
        $"slk_{Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant()}";

    private static bool IsValidCredentialHash(string credentialKeyHash) =>
        credentialKeyHash.Length == 64
        && credentialKeyHash.All(static c =>
            c is >= '0' and <= '9'
            || c is >= 'a' and <= 'f'
            || c is >= 'A' and <= 'F');

}
