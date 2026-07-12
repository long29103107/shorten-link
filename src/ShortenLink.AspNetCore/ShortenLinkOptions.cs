namespace ShortenLink.AspNetCore;

public sealed class ShortenLinkOptions
{
    public const string SectionName = "ShortenLink";

    public string? BaseUrl { get; set; }

    public ShortenLinkDatabaseOptions Database { get; set; } = new();

    public ShortenLinkRedirectOptions Redirect { get; set; } = new();

    public ShortenLinkAnalyticsOptions Analytics { get; set; } = new();

    public ShortenLinkCacheOptions Cache { get; set; } = new();

    public ShortenLinkRateLimitingOptions RateLimiting { get; set; } = new();
}

public sealed class ShortenLinkDatabaseOptions
{
    public bool UsePostgres { get; set; }

    public string SqliteConnectionString { get; set; } = "Data Source=shorten-link.db";

    public string PostgresConnectionString { get; set; } = string.Empty;
}

public sealed class ShortenLinkRedirectOptions
{
    public bool EnableFrontendFallback { get; set; } = true;

    public string FrontendFallbackPath { get; set; } = "/not-found";
}

public sealed class ShortenLinkAnalyticsOptions
{
    public bool Enabled { get; set; }

    public bool UseAsyncWorker { get; set; } = true;

    public int QueueCapacity { get; set; } = 512;
}

public sealed class ShortenLinkCacheOptions
{
    public bool Enabled { get; set; }

    public string Provider { get; set; } = "Memory";

    public string RedisConnectionString { get; set; } = string.Empty;

    public int EntryTtlSeconds { get; set; } = 3600;
}

public sealed class ShortenLinkRateLimitingOptions
{
    public bool Enabled { get; set; }

    public ShortenLinkFixedWindowRateLimitOptions Create { get; set; } = new();

    public ShortenLinkFixedWindowRateLimitOptions Redirect { get; set; } = new();
}

public sealed class ShortenLinkFixedWindowRateLimitOptions
{
    public int PermitLimit { get; set; } = 60;

    public int WindowSeconds { get; set; } = 60;

    public int QueueLimit { get; set; }
}
