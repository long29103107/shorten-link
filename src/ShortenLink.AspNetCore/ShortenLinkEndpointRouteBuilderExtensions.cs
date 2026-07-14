using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShortenLink.Core;
using ShortenLink.Core.Domain;
using ShortenLink.Core.Services;

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

    private static async Task<Ok<IReadOnlyList<ShortLinkAdminListItemResponse>>> ListShortLinksAsync(
        IShortLinkService shortLinkService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        int? limit,
        CancellationToken cancellationToken)
    {
        var safeLimit = Math.Clamp(limit ?? 100, 1, 500);
        var shortLinks = await shortLinkService.ListRecentAsync(safeLimit, cancellationToken)
            .ConfigureAwait(false);
        var response = shortLinks
            .Select(shortLink => ShortLinkAdminListItemResponse.FromDomain(
                shortLink,
                BuildShortUrl(shortLink.Code, options.Value, httpContext)))
            .ToList();

        return TypedResults.Ok<IReadOnlyList<ShortLinkAdminListItemResponse>>(response);
    }

    private static async Task<Results<Created<ShortLinkCreatedResponse>, JsonHttpResult<ShortLinkErrorResponse>>> CreateShortLinkAsync(
        ShortLinkCreateRequest request,
        IShortLinkService shortLinkService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

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

    private static async Task<Results<Ok<ShortLinkAdminListItemResponse>, JsonHttpResult<ShortLinkErrorResponse>>> UpdateShortLinkAsync(
        string code,
        ShortLinkUpdateRequest request,
        IShortLinkService shortLinkService,
        IOptions<ShortenLinkOptions> options,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

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
        CancellationToken cancellationToken)
    {
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
        CancellationToken cancellationToken)
    {
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
        CancellationToken cancellationToken)
    {
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

    private static IResult CreateErrorResult(string? errorCode, string? errorMessage) =>
        CreateErrorResponse(errorCode, errorMessage);

    private static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            ShortLinkErrorCodes.InvalidCode => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidExpiration => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidUrl => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ShortLinkErrorCodes.Expired => StatusCodes.Status410Gone,
            ShortLinkErrorCodes.Inactive => StatusCodes.Status410Gone,
            _ => StatusCodes.Status500InternalServerError
        };

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

    public sealed record ShortLinkDeactivatedResponse(string Code, bool IsActive);

    public sealed record ShortLinkDeletedResponse(string Code);

    public sealed record ShortLinkErrorResponse(string ErrorCode, string Message);
}
