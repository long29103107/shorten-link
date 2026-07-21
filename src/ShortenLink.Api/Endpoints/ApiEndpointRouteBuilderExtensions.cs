namespace ShortenLink.Api.Endpoints;

internal static class ApiEndpointRouteBuilderExtensions
{
    public static WebApplication MapApiHostEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthEndpoints();
        if (app.Environment.IsDevelopment())
        {
            app.MapMockDataEndpoints();
        }

        return app;
    }
}
