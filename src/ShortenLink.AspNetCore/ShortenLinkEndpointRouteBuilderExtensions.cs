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

        var securityAssignments = endpoints.MapGroup("/api/security/assignments")
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

    private static async Task<Results<Ok<ShortLinkAdminListResponse>, JsonHttpResult<ShortLinkErrorResponse>>> ListShortLinksAsync(
        IShortLinkService shortLinkService,
        IShortenLinkAuthorizationService authorizationService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        int? limit,
        int? page,
        string? cursor,
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
        if (page is not null)
        {
            var safePage = Math.Max(page.Value, 1);
            var totalCount = await shortLinkService.CountAsync(cancellationToken)
                .ConfigureAwait(false);
            var numberedPageItems = await shortLinkService.ListRecentPageAsync(
                    (safePage - 1) * safeLimit,
                    safeLimit,
                    cancellationToken)
                .ConfigureAwait(false);
            var pageResponse = numberedPageItems
                .Select(shortLink => ShortLinkAdminListItemResponse.FromDomain(
                    shortLink,
                    BuildShortUrl(shortLink.Code, options.Value, httpContext)))
                .ToList();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)safeLimit));

            return TypedResults.Ok(new ShortLinkAdminListResponse(
                pageResponse,
                null,
                totalCount,
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
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
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
            return CreateErrorResponse(result.ErrorCode, result.ErrorMessage);
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
        string? errorMessage)
    {
        var response = new ShortLinkErrorResponse(
            errorCode ?? "unknown_error",
            errorMessage ?? "An unexpected short-link error occurred.");

        return TypedResults.Json(response, statusCode: GetStatusCode(response.ErrorCode));
    }

    private static JsonHttpResult<ShortLinkErrorResponse> CreateAuthorizationErrorResponse(
        ShortenLinkAuthorizationResult authorization)
    {
        var response = new ShortLinkErrorResponse(
            authorization.ErrorCode ?? "forbidden",
            authorization.ErrorMessage ?? "The request is not authorized.");

        return TypedResults.Json(
            response,
            statusCode: authorization.IsAuthenticated
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status401Unauthorized);
    }

    private static IResult CreateErrorResult(string? errorCode, string? errorMessage) =>
        CreateErrorResponse(errorCode, errorMessage);

    private static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            ShortLinkErrorCodes.InvalidCode => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidExpiration => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidUrl => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.NotFound => StatusCodes.Status404NotFound,
            "invalid_credential_hash" => StatusCodes.Status400BadRequest,
            "invalid_cursor" => StatusCodes.Status400BadRequest,
            "invalid_permission" => StatusCodes.Status400BadRequest,
            "invalid_role" => StatusCodes.Status400BadRequest,
            "invalid_security_assignment" => StatusCodes.Status400BadRequest,
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
            return CreateErrorResponse("invalid_security_assignment", "Credential key is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return CreateErrorResponse("invalid_security_assignment", "Assignment name is required.");
        }

        foreach (var role in NormalizeDistinct(request.Roles))
        {
            if (!ShortenLinkRoles.PermissionBundles.ContainsKey(role))
            {
                return CreateErrorResponse("invalid_role", $"Unknown system role '{role}'.");
            }
        }

        foreach (var permission in NormalizeDistinct(request.Permissions))
        {
            if (!ShortenLinkPermissions.All.Contains(permission))
            {
                return CreateErrorResponse("invalid_permission", $"Unknown permission '{permission}'.");
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

    private static string HashCredential(string apiKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

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

    public sealed record ShortLinkErrorResponse(string ErrorCode, string Message);
}
