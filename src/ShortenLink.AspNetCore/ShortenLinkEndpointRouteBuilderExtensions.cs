using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
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

        shortLinks.MapPost("/", CreateShortLinkAsync)
            .WithName("CreateShortLink");

        shortLinks.MapGet("/{code}", GetShortLinkDetailsAsync)
            .WithName("GetShortLinkDetails");

        shortLinks.MapDelete("/{code}", DeactivateShortLinkAsync)
            .WithName("DeactivateShortLink");

        endpoints.MapGet("/{code}", RedirectShortLinkAsync)
            .WithName("RedirectShortLink");

        return endpoints;
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
            new CreateShortLinkRequest(request.OriginalUrl, request.CustomAlias, request.ExpiredAtUtc),
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

    private static async Task<IResult> RedirectShortLinkAsync(
        string code,
        IShortLinkService shortLinkService,
        IOptions<ShortenLinkOptions> options,
        CancellationToken cancellationToken)
    {
        var result = await shortLinkService.ResolveAsync(code, cancellationToken).ConfigureAwait(false);
        if (result.Succeeded && result.ShortLink is not null)
        {
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
            ShortLinkErrorCodes.InvalidAlias => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidCode => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidExpiration => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.InvalidUrl => StatusCodes.Status400BadRequest,
            ShortLinkErrorCodes.DuplicateAlias => StatusCodes.Status409Conflict,
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
        string? CustomAlias,
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

    public sealed record ShortLinkErrorResponse(string ErrorCode, string Message);
}
