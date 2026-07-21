using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShortenLink.Core;
using ShortenLink.Core.Domain;
using ShortenLink.Core.Repositories;
using ShortenLink.Core.Security;
using ShortenLink.Core.Services;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace ShortenLink.AspNetCore;

public static class ShortenLinkEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapShortenLinkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var shortLinks = endpoints.MapGroup("/api/short-links")
            .WithTags("Short Links");

        shortLinks.MapGet("/", ListShortLinksAsync)
            .WithName("ListShortLinks");

        var createEndpoint = shortLinks.MapPost("/", CreateShortLinkAsync)
            .WithName("CreateShortLink");

        shortLinks.MapGet("/{code}", GetShortLinkDetailsAsync)
            .WithName("GetShortLinkDetails");

        shortLinks.MapGet("/{code}/analytics", GetShortLinkAnalyticsAsync)
            .WithName("GetShortLinkAnalytics");

        shortLinks.MapPut("/{code}", UpdateShortLinkAsync)
            .WithName("UpdateShortLink");

        shortLinks.MapPost("/{code}/deactivate", DeactivateShortLinkAsync)
            .WithName("DeactivateShortLink");

        shortLinks.MapPost("/{code}/activate", ActivateShortLinkAsync)
            .WithName("ActivateShortLink");

        shortLinks.MapDelete("/{code}", DeleteShortLinkAsync)
            .WithName("DeleteShortLink");

        var redirectEndpoint = endpoints.MapGet("/{code}", RedirectShortLinkAsync)
            .WithName("RedirectShortLink");

        var security = endpoints.MapGroup("/api/security")
            .WithTags("Security");

        security.MapPost("/login", LoginSecurityUserAsync)
            .WithName("LoginSecurityUser");

        security.MapPost("/refresh", RefreshSecurityUserAsync)
            .WithName("RefreshSecurityUser");

        security.MapGet("/me", GetCurrentSecurityUserAsync)
            .WithName("GetCurrentSecurityUser");

        var userApiKeys = security.MapGroup("/api-keys")
            .WithTags("Security API Keys");

        userApiKeys.MapGet("/", ListCurrentUserApiKeysAsync)
            .WithName("ListCurrentUserApiKeys");

        userApiKeys.MapPost("/", CreateCurrentUserApiKeyAsync)
            .WithName("CreateCurrentUserApiKey");

        userApiKeys.MapPut("/{id}", RenameCurrentUserApiKeyAsync)
            .WithName("RenameCurrentUserApiKey");

        userApiKeys.MapPost("/{id}/disable", DisableCurrentUserApiKeyAsync)
            .WithName("DisableCurrentUserApiKey");

        var securityRoles = security.MapGroup("/roles")
            .WithTags("Security Roles");

        securityRoles.MapGet("/", ListSecurityRolesAsync)
            .WithName("ListSecurityRoles");

        securityRoles.MapPut("/custom", UpsertCustomSecurityRoleAsync)
            .WithName("UpsertCustomSecurityRole");

        securityRoles.MapPost("/custom/{id}/disable", DisableCustomSecurityRoleAsync)
            .WithName("DisableCustomSecurityRole");

        var securityUsers = security.MapGroup("/users")
            .WithTags("Security Users");

        securityUsers.MapGet("/", ListSecurityUsersAsync)
            .WithName("ListSecurityUsers");

        securityUsers.MapPut("/", UpsertSecurityUserAsync)
            .WithName("UpsertSecurityUser");

        securityUsers.MapPost("/{id}/disable", DisableSecurityUserAsync)
            .WithName("DisableSecurityUser");

        var securityAssignments = security.MapGroup("/assignments")
            .WithTags("Security Assignments");

        securityAssignments.MapGet("/", ListSecurityAssignmentsAsync)
            .WithName("ListSecurityAssignments");

        securityAssignments.MapPut("/", UpsertSecurityAssignmentAsync)
            .WithName("UpsertSecurityAssignment");

        securityAssignments.MapPost("/{credentialKeyHash}/disable", DisableSecurityAssignmentAsync)
            .WithName("DisableSecurityAssignment");

        var options = endpoints.ServiceProvider
            .GetRequiredService<IOptions<ShortenLinkOptions>>()
            .Value;
        if (options.RateLimiting.Enabled)
        {
            createEndpoint.RequireRateLimiting(ShortenLinkRateLimitingPolicyNames.Create);
            redirectEndpoint.RequireRateLimiting(ShortenLinkRateLimitingPolicyNames.Redirect);
        }

        return endpoints;
    }

    private static async Task<Results<Ok<SecurityLoginResponse>, JsonHttpResult<ShortLinkErrorResponse>>> LoginSecurityUserAsync(
        SecurityLoginRequest request,
        IShortenLinkUserSessionService userSessionService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var missingLoginFields = new List<(string Field, string Message)>();
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            missingLoginFields.Add(("username", "Username is required."));
        }
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            missingLoginFields.Add(("password", "Password is required."));
        }
        if (missingLoginFields.Count > 0)
        {
            return CreateErrorResponse(
                "invalid_login",
                "Username or password is invalid.",
                CreateFieldErrors(missingLoginFields));
        }

        var login = await userSessionService
            .LoginAsync(request.Username, request.Password, cancellationToken)
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

    private static async Task<Results<Ok<SecurityLoginResponse>, JsonHttpResult<ShortLinkErrorResponse>>> RefreshSecurityUserAsync(
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

    private static async Task<Results<Ok<SecurityCurrentUserResponse>, JsonHttpResult<ShortLinkErrorResponse>>> GetCurrentSecurityUserAsync(
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

    private static async Task<Results<Ok<SecurityUserApiKeysListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListCurrentUserApiKeysAsync(
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

    private static async Task<Results<Ok<SecurityUserApiKeyCreatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> CreateCurrentUserApiKeyAsync(
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

    private static async Task<Results<Ok<SecurityUserApiKeyResponse>, JsonHttpResult<ShortLinkErrorResponse>>> RenameCurrentUserApiKeyAsync(
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
            apiKey.Id,
            apiKey.UserId,
            request.DisplayName.Trim(),
            apiKey.KeyHash,
            apiKey.IsEnabled,
            apiKey.CreatedAt);

        await apiKeyRepository.AddOrUpdateAsync(renamed, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(SecurityUserApiKeyResponse.FromDomain(renamed));
    }

    private static async Task<Results<Ok<SecurityUserApiKeyDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableCurrentUserApiKeyAsync(
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

    private static async Task<Results<Ok<SecurityRolesListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListSecurityRolesAsync(
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var systemRoles = ShortenLinkSystemRoles.PermissionBundles
            .OrderBy(role => role.Key, StringComparer.OrdinalIgnoreCase)
            .Select(role => SecurityRoleResponse.System(role.Key, role.Value))
            .ToList();
        var customRoles = await roleRepository.ListCustomRolesAsync(cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(new SecurityRolesListResponse(
            systemRoles,
            customRoles.Select(SecurityRoleResponse.Custom).ToList()));
    }

    private static async Task<Results<Ok<SecurityRoleResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpsertCustomSecurityRoleAsync(
        SecurityCustomRoleUpsertRequest request,
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
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
        return TypedResults.Ok(SecurityRoleResponse.Custom(role));
    }

    private static async Task<Results<Ok<SecurityRoleDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableCustomSecurityRoleAsync(
        string id,
        IShortenLinkSecurityRoleRepository roleRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        if (ShortenLinkSystemRoles.PermissionBundles.ContainsKey(id))
        {
            return CreateErrorResponse("system_role_immutable", "System roles cannot be disabled.");
        }

        var disabled = await roleRepository.DisableCustomRoleAsync(id, cancellationToken).ConfigureAwait(false);
        if (!disabled)
        {
            return CreateErrorResponse(ShortLinkErrorCodes.NotFound, "Custom role was not found.");
        }

        return TypedResults.Ok(new SecurityRoleDisabledResponse(id, false));
    }

    private static async Task<Results<Ok<SecurityUsersListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListSecurityUsersAsync(
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var users = await userRepository.ListAsync(includeHidden: false, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(new SecurityUsersListResponse(
            users.Select(SecurityUserResponse.FromDomain).ToList()));
    }

    private static async Task<Results<Ok<SecurityUserResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpsertSecurityUserAsync(
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
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
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
            : existing!.PasswordHash;

        var user = new ShortenLinkSecurityUser(
            request.Id.Trim(),
            request.Username.Trim(),
            request.DisplayName.Trim(),
            passwordHash,
            NormalizeDistinct(request.RoleIds),
            request.IsEnabled ?? true,
            isHidden: false,
            isBootstrap: false,
            existing?.CreatedAt ?? timeProvider.GetUtcNow());

        await userRepository.AddOrUpdateAsync(user, cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(SecurityUserResponse.FromDomain(user));
    }

    private static async Task<Results<Ok<SecurityUserDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableSecurityUserAsync(
        string id,
        IShortenLinkSecurityUserRepository userRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
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

    private static async Task<Results<Ok<ShortLinkAdminListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListShortLinksAsync(
        IShortLinkService shortLinkService,
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
            var numberedPage = await shortLinkService.ListPageAsync(
                    (safePage - 1) * safeLimit,
                    safeLimit,
                    search,
                    parsedStatus,
                    parsedSortBy,
                    parsedSortDirection,
                    cancellationToken)
                .ConfigureAwait(false);
            var pageResponse = numberedPage.Items
                .Select(shortLink => ShortLinkAdminListItemResponse.FromDomain(
                    shortLink,
                    BuildShortUrl(shortLink.Code, options.Value, httpContext)))
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

        var shortLinks = await shortLinkService.ListRecentAsync(safeLimit + 1, beforeCreatedAt, beforeCode, cancellationToken)
            .ConfigureAwait(false);
        var pageItems = shortLinks.Take(safeLimit).ToList();
        var response = pageItems
            .Select(shortLink => ShortLinkAdminListItemResponse.FromDomain(
                shortLink,
                BuildShortUrl(shortLink.Code, options.Value, httpContext)))
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

    private static async Task<Results<Created<ShortLinkCreatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> CreateShortLinkAsync(
        ShortLinkCreateRequest request,
        IShortLinkService shortLinkService,
        IShortenLinkAuthorizationService authorizationService,
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

        var result = await shortLinkService.CreateAsync(
            new CreateShortLinkRequest(request.OriginalUrl, request.ExpiredAtUtc),
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

    private static async Task<Results<Ok<ShortLinkDetailsResponse>, JsonHttpResult<ShortLinkErrorResponse>>> GetShortLinkDetailsAsync(
        string code,
        IShortLinkService shortLinkService,
        CancellationToken cancellationToken)
    {
        var result = await shortLinkService.GetDetailsAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.ShortLink is null)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(ShortLinkDetailsResponse.FromDomain(result.ShortLink));
    }

    private static async Task<Results<Ok<ShortLinkAnalyticsResponse>, JsonHttpResult<ShortLinkErrorResponse>>> GetShortLinkAnalyticsAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortLinkClickRepository clickRepository,
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

        var safeLimit = Math.Clamp(limit ?? 20, 1, 100);
        var summary = await clickRepository.GetSummaryAsync(code, cancellationToken).ConfigureAwait(false);
        var recentClicks = await clickRepository.ListRecentAsync(code, safeLimit, cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(ShortLinkAnalyticsResponse.FromClicks(
            code,
            summary,
            recentClicks));
    }

    private static async Task<Results<Ok<SecurityAssignmentsListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListSecurityAssignmentsAsync(
        IShortenLinkSecurityAssignmentRepository assignmentRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var assignments = await assignmentRepository.ListAsync(cancellationToken).ConfigureAwait(false);
        return TypedResults.Ok(new SecurityAssignmentsListResponse(
            assignments.Select(SecurityAssignmentResponse.FromDomain).ToList()));
    }

    private static async Task<Results<Ok<SecurityAssignmentResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpsertSecurityAssignmentAsync(
        SecurityAssignmentUpsertRequest request,
        IShortenLinkSecurityAssignmentRepository assignmentRepository,
        IShortenLinkAuthorizationService authorizationService,
        TimeProvider timeProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
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

    private static async Task<Results<Ok<SecurityAssignmentDisabledResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DisableSecurityAssignmentAsync(
        string credentialKeyHash,
        IShortenLinkSecurityAssignmentRepository assignmentRepository,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.SecurityAssignmentsManage, cancellationToken)
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

    private static async Task<Results<Ok<ShortLinkAdminListItemResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpdateShortLinkAsync(
        string code,
        ShortLinkUpdateRequest request,
        IShortLinkService shortLinkService,
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

    private static async Task<Results<Ok<ShortLinkDeactivatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DeactivateShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksDeactivate, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var result = await shortLinkService.DeactivateAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(new ShortLinkDeactivatedResponse(code, false));
    }

    private static async Task<Results<Ok<ShortLinkDeactivatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ActivateShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IShortenLinkAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var authorization = await authorizationService
            .AuthorizeAsync(httpContext, ShortenLinkPermissions.ShortLinksActivate, cancellationToken)
            .ConfigureAwait(false);
        if (!authorization.Succeeded)
        {
            return CreateAuthorizationErrorResponse(authorization);
        }

        var result = await shortLinkService.ActivateAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(new ShortLinkDeactivatedResponse(code, true));
    }

    private static async Task<Results<Ok<ShortLinkDeletedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> DeleteShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
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

        var result = await shortLinkService.DeleteAsync(code, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
        }

        return TypedResults.Ok(new ShortLinkDeletedResponse(code));
    }

    private static async Task<IResult> RedirectShortLinkAsync(
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
            "system_role_immutable" => StatusCodes.Status400BadRequest,
            "bootstrap_user_immutable" => StatusCodes.Status400BadRequest,
            "unauthorized" => StatusCodes.Status401Unauthorized,
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

        if (existing is null && string.IsNullOrWhiteSpace(request.Password))
        {
            return CreateFieldErrorResponse("invalid_security_user", "Password is required when creating a user.", "password");
        }

        var usernameOwner = await userRepository
            .FindByUsernameAsync(request.Username.Trim(), cancellationToken)
            .ConfigureAwait(false);
        if (usernameOwner is not null && !usernameOwner.Id.Equals(request.Id.Trim(), StringComparison.Ordinal))
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

    public sealed record ShortLinkCreateRequest(
        string OriginalUrl,
        DateTimeOffset? ExpiredAtUtc);

    public sealed record ShortLinkUpdateRequest(
        string OriginalUrl,
        DateTimeOffset? ExpiredAtUtc);

    public sealed record ShortLinkCreatedResponse(
        string Code,
        string ShortUrl,
        string OriginalUrl,
        DateTimeOffset CreatedAtUtc)
    {
        public static ShortLinkCreatedResponse FromDomain(ShortLink shortLink, string shortUrl) =>
            new(shortLink.Code, shortUrl, shortLink.OriginalUrl.AbsoluteUri, shortLink.CreatedAt);
    }

    public sealed record ShortLinkAdminListItemResponse(
        string Code,
        string ShortUrl,
        string OriginalUrl,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? ExpiredAtUtc,
        bool IsActive)
    {
        public static ShortLinkAdminListItemResponse FromDomain(ShortLink shortLink, string shortUrl) =>
            new(
                shortLink.Code,
                shortUrl,
                shortLink.OriginalUrl.AbsoluteUri,
                shortLink.CreatedAt,
                shortLink.ExpiresAt,
                shortLink.IsActive);
    }

    public sealed record ShortLinkAdminListResponse(
        IReadOnlyList<ShortLinkAdminListItemResponse> Items,
        string? NextCursor,
        int? TotalCount = null,
        int? Page = null,
        int? PageSize = null,
        int? TotalPages = null);

    public sealed record ShortLinkDetailsResponse(
        string Code,
        string OriginalUrl,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? ExpiredAtUtc,
        bool IsActive)
    {
        public static ShortLinkDetailsResponse FromDomain(ShortLink shortLink) =>
            new(
                shortLink.Code,
                shortLink.OriginalUrl.AbsoluteUri,
                shortLink.CreatedAt,
                shortLink.ExpiresAt,
                shortLink.IsActive);
    }

    public sealed record ShortLinkAnalyticsResponse(
        string Code,
        long ClickCount,
        DateTimeOffset? LastClickedAtUtc,
        IReadOnlyList<ShortLinkClickActivityResponse> RecentClicks)
    {
        public static ShortLinkAnalyticsResponse FromClicks(
            string code,
            ShortLinkClickSummary summary,
            IReadOnlyList<ShortLinkClick> recentClicks) =>
            new(
                code,
                summary.ClickCount,
                summary.LastClickedAtUtc,
                recentClicks.Select(ShortLinkClickActivityResponse.FromDomain).ToList());
    }

    public sealed record ShortLinkClickActivityResponse(
        DateTimeOffset ClickedAtUtc,
        string? RemoteIpAddress,
        string? UserAgent,
        string? Referrer)
    {
        public static ShortLinkClickActivityResponse FromDomain(ShortLinkClick click) =>
            new(
                click.ClickedAtUtc,
                click.RemoteIpAddress,
                click.UserAgent,
                click.Referrer);
    }

    public sealed record SecurityAssignmentUpsertRequest(
        string Name,
        string CredentialKey,
        IReadOnlyList<string>? Roles,
        IReadOnlyList<string>? Permissions,
        bool? IsEnabled);

    public sealed record SecurityLoginRequest(
        string Username,
        string Password);

    public sealed record SecurityRefreshRequest(string RefreshToken);

    public sealed record SecurityLoginResponse(
        string Token,
        string AccessToken,
        string RefreshToken,
        SecurityCurrentUserResponse User);

    public sealed record SecurityCurrentUserResponse(
        string UserId,
        string Username,
        string DisplayName,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions,
        DateTimeOffset IssuedAtUtc)
    {
        public static SecurityCurrentUserResponse FromPrincipal(ShortenLinkUserSessionPrincipal principal) =>
            new(
                principal.UserId,
                principal.Username,
                principal.DisplayName,
                principal.Roles,
                principal.Permissions,
                principal.IssuedAtUtc);
    }

    public sealed record SecurityUserApiKeyCreateRequest(
        string DisplayName);

    public sealed record SecurityUserApiKeyRenameRequest(
        string DisplayName);

    public sealed record SecurityUserApiKeysListResponse(
        IReadOnlyList<SecurityUserApiKeyResponse> Items);

    public sealed record SecurityUserApiKeyCreatedResponse(
        SecurityUserApiKeyResponse ApiKey,
        string RawApiKey);

    public sealed record SecurityUserApiKeyResponse(
        string Id,
        string DisplayName,
        bool IsEnabled,
        DateTimeOffset CreatedAtUtc)
    {
        public static SecurityUserApiKeyResponse FromDomain(ShortenLinkUserApiKey apiKey) =>
            new(
                apiKey.Id,
                apiKey.DisplayName,
                apiKey.IsEnabled,
                apiKey.CreatedAt);
    }

    public sealed record SecurityUserApiKeyDisabledResponse(
        string Id,
        bool IsEnabled);

    public sealed record SecurityRolesListResponse(
        IReadOnlyList<SecurityRoleResponse> SystemRoles,
        IReadOnlyList<SecurityRoleResponse> CustomRoles);

    public sealed record SecurityRoleResponse(
        string Id,
        string Name,
        IReadOnlyList<string> Permissions,
        bool IsSystem,
        bool IsEnabled,
        bool CanDelete,
        DateTimeOffset? CreatedAtUtc)
    {
        public static SecurityRoleResponse System(string id, IEnumerable<string> permissions) =>
            new(
                id,
                id,
                permissions.OrderBy(static permission => permission, StringComparer.Ordinal).ToList(),
                IsSystem: true,
                IsEnabled: true,
                CanDelete: false,
                CreatedAtUtc: null);

        public static SecurityRoleResponse Custom(ShortenLinkCustomRole role) =>
            new(
                role.Id,
                role.Name,
                role.Permissions,
                IsSystem: false,
                role.IsEnabled,
                CanDelete: true,
                role.CreatedAt);
    }

    public sealed record SecurityCustomRoleUpsertRequest(
        string Id,
        string Name,
        IReadOnlyList<string>? Permissions,
        bool? IsEnabled);

    public sealed record SecurityRoleDisabledResponse(
        string Id,
        bool IsEnabled);

    public sealed record SecurityUsersListResponse(
        IReadOnlyList<SecurityUserResponse> Items);

    public sealed record SecurityUserResponse(
        string Id,
        string Username,
        string DisplayName,
        IReadOnlyList<string> RoleIds,
        bool IsEnabled,
        bool IsHidden,
        bool IsBootstrap,
        DateTimeOffset CreatedAtUtc)
    {
        public static SecurityUserResponse FromDomain(ShortenLinkSecurityUser user) =>
            new(
                user.Id,
                user.Username,
                user.DisplayName,
                user.RoleIds,
                user.IsEnabled,
                user.IsHidden,
                user.IsBootstrap,
                user.CreatedAt);
    }

    public sealed record SecurityUserUpsertRequest(
        string Id,
        string Username,
        string DisplayName,
        string? Password,
        IReadOnlyList<string>? RoleIds,
        bool? IsEnabled);

    public sealed record SecurityUserDisabledResponse(
        string Id,
        bool IsEnabled);

    public sealed record SecurityAssignmentsListResponse(
        IReadOnlyList<SecurityAssignmentResponse> Items);

    public sealed record SecurityAssignmentResponse(
        string CredentialKeyHash,
        string Name,
        IReadOnlyList<string> Roles,
        IReadOnlyList<string> Permissions,
        bool IsEnabled,
        DateTimeOffset CreatedAtUtc)
    {
        public static SecurityAssignmentResponse FromDomain(ShortenLinkSecurityAssignment assignment) =>
            new(
                assignment.CredentialKeyHash,
                assignment.Name,
                assignment.Roles,
                assignment.Permissions,
                assignment.IsEnabled,
                assignment.CreatedAt);
    }

    public sealed record SecurityAssignmentDisabledResponse(
        string CredentialKeyHash,
        bool IsEnabled);

    public sealed record ShortLinkDeactivatedResponse(string Code, bool IsActive);

    public sealed record ShortLinkDeletedResponse(string Code);

    public sealed record ShortLinkErrorResponse(
        string ErrorCode,
        string Message,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyDictionary<string, IReadOnlyList<string>>? FieldErrors = null);
}
