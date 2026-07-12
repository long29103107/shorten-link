using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ShortenLink.Core.Generation;
using ShortenLink.Core.Repositories;
using ShortenLink.Core.Services;
using ShortenLink.Infrastructure.Persistence;
using ShortenLink.Infrastructure.Repositories;

namespace ShortenLink.AspNetCore;

public static class ShortenLinkServiceCollectionExtensions
{
    public static IServiceCollection AddShortenLink(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ShortenLinkOptions>()
            .Bind(configuration.GetSection(ShortenLinkOptions.SectionName))
            .Validate(
                static options => HasRequiredConnectionString(options.Database),
                "ShortenLink database configuration requires SqliteConnectionString when UsePostgres is false, or PostgresConnectionString when UsePostgres is true.")
            .Validate(
                static options => string.IsNullOrWhiteSpace(options.BaseUrl)
                    || Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
                        && (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps),
                "ShortenLink:BaseUrl must be an absolute HTTP or HTTPS URL when provided.")
            .Validate(
                static options => IsValidFrontendFallbackPath(options.Redirect.FrontendFallbackPath),
                "ShortenLink:Redirect:FrontendFallbackPath must be a root-relative path or an absolute HTTP/HTTPS URL.")
            .ValidateOnStart();

        services.AddDbContext<ShortLinkDbContext>((serviceProvider, options) =>
        {
            var shortenLinkOptions = serviceProvider
                .GetRequiredService<IOptions<ShortenLinkOptions>>()
                .Value;

            if (shortenLinkOptions.Database.UsePostgres)
            {
                options.UseNpgsql(shortenLinkOptions.Database.PostgresConnectionString);
                return;
            }

            options.UseSqlite(shortenLinkOptions.Database.SqliteConnectionString);
        });

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<IShortCodeGenerator, Base62ShortCodeGenerator>();
        services.TryAddScoped<IShortLinkRepository, EfCoreShortLinkRepository>();
        services.TryAddScoped<IShortLinkService, ShortLinkService>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, ShortLinkDatabaseInitializationService>());

        return services;
    }

    private static bool IsValidFrontendFallbackPath(string? fallbackPath)
    {
        if (string.IsNullOrWhiteSpace(fallbackPath))
        {
            return true;
        }

        if (fallbackPath.StartsWith("/", StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(fallbackPath, UriKind.Absolute, out var absoluteFallbackUri)
            && (absoluteFallbackUri.Scheme == Uri.UriSchemeHttp
                || absoluteFallbackUri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool HasRequiredConnectionString(ShortenLinkDatabaseOptions databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);

        return databaseOptions.UsePostgres
            ? !string.IsNullOrWhiteSpace(databaseOptions.PostgresConnectionString)
            : !string.IsNullOrWhiteSpace(databaseOptions.SqliteConnectionString);
    }
}
