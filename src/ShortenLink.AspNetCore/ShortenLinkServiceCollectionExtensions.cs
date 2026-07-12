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
                static options => !options.Database.UsePostgres,
                "PostgreSQL provider selection is not available until Phase 002.")
            .Validate(
                static options => !string.IsNullOrWhiteSpace(options.Database.SqliteConnectionString),
                "ShortenLink:Database:SqliteConnectionString is required.")
            .Validate(
                static options => string.IsNullOrWhiteSpace(options.BaseUrl)
                    || Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)
                        && (baseUri.Scheme == Uri.UriSchemeHttp || baseUri.Scheme == Uri.UriSchemeHttps),
                "ShortenLink:BaseUrl must be an absolute HTTP or HTTPS URL when provided.")
            .Validate(
                static options => string.IsNullOrWhiteSpace(options.Redirect.FrontendFallbackPath)
                    || options.Redirect.FrontendFallbackPath.StartsWith("/", StringComparison.Ordinal),
                "ShortenLink:Redirect:FrontendFallbackPath must start with '/'.")
            .ValidateOnStart();

        services.AddDbContext<ShortLinkDbContext>((serviceProvider, options) =>
        {
            var shortenLinkOptions = serviceProvider
                .GetRequiredService<IOptions<ShortenLinkOptions>>()
                .Value;

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
}
