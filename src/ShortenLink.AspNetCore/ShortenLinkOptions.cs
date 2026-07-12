namespace ShortenLink.AspNetCore;

public sealed class ShortenLinkOptions
{
    public const string SectionName = "ShortenLink";

    public string? BaseUrl { get; set; }

    public ShortenLinkDatabaseOptions Database { get; set; } = new();

    public ShortenLinkRedirectOptions Redirect { get; set; } = new();
}

public sealed class ShortenLinkDatabaseOptions
{
    public bool UsePostgres { get; set; }

    public string SqliteConnectionString { get; set; } = "Data Source=shorten-link.db";
}

public sealed class ShortenLinkRedirectOptions
{
    public bool EnableFrontendFallback { get; set; } = true;

    public string FrontendFallbackPath { get; set; } = "/not-found";
}
