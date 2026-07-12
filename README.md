# Shorten Link

Reusable .NET short-link library plus a demo ASP.NET Core API and React frontend.

This repository is currently in Phase 001 scaffold work. The reusable library projects are intentionally separated from the demo application so they can be packed as NuGet packages and consumed by other .NET applications.

## Project Structure

```text
src/
  ShortenLink.Core/              # Domain contracts and core abstractions
  ShortenLink.Infrastructure/    # Persistence and provider adapters
  ShortenLink.AspNetCore/        # DI setup and endpoint mapping integration
  ShortenLink.Api/               # Demo ASP.NET Core API host
  ShortenLink.Web/               # Demo React + Vite frontend

tests/
  ShortenLink.Core.Tests/
  ShortenLink.Infrastructure.Tests/
  ShortenLink.Api.Tests/
```

## Build And Pack

Use these commands from the repository root.

### Build

```powershell
dotnet build ShortenLink.slnx
```

## Test

The test projects are placeholders in `001_001`. Real unit and integration tests are added by later Phase 001 tasks.

```powershell
dotnet test ShortenLink.slnx
```

### Pack The Consumer Package

The normal ASP.NET Core consumer entry point is `ShortenLink.AspNetCore`. It references the lower-level reusable projects and exposes host-facing extension methods.

```powershell
dotnet pack src\ShortenLink.AspNetCore\ShortenLink.AspNetCore.csproj -c Release
```

The package is created at:

```text
src\ShortenLink.AspNetCore\bin\Release\ShortenLink.AspNetCore.1.0.0.nupkg
```

Lower-level packages can also be packed when a consumer needs them directly:

```powershell
dotnet pack src\ShortenLink.Core\ShortenLink.Core.csproj -c Release
dotnet pack src\ShortenLink.Infrastructure\ShortenLink.Infrastructure.csproj -c Release
```

Their default output paths are:

```text
src\ShortenLink.Core\bin\Release\ShortenLink.Core.1.0.0.nupkg
src\ShortenLink.Infrastructure\bin\Release\ShortenLink.Infrastructure.1.0.0.nupkg
```

To pack every packable project in the solution:

```powershell
dotnet pack ShortenLink.slnx -c Release
```

## Use From Another .NET App

Most ASP.NET Core consumers should start with `ShortenLink.AspNetCore`. That package is the host-facing entry point for dependency injection and endpoint mapping. It brings the lower-level reusable projects with it through package/project references.

### Option 1: Project Reference During Local Development

From the consumer app directory:

```powershell
dotnet add reference ..\shorten-link\src\ShortenLink.AspNetCore\ShortenLink.AspNetCore.csproj
```

### Option 2: Install From A Local NuGet Folder

Create a local package folder and copy the packed package into it:

```powershell
New-Item -ItemType Directory -Force .\.nupkg
Copy-Item ..\shorten-link\src\ShortenLink.AspNetCore\bin\Release\*.nupkg .\.nupkg\
```

Add that folder as a NuGet source for the consumer app:

```powershell
dotnet nuget add source .\.nupkg --name shorten-link-local
```

Install the package:

```powershell
dotnet add package ShortenLink.AspNetCore --source .\.nupkg
```

If the consumer needs direct access to lower-level contracts, install/reference `ShortenLink.Core` as well. Normal API hosts should not need to start there.

### ASP.NET Core Setup

In the consumer app's `Program.cs`:

```csharp
using ShortenLink.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShortenLink(builder.Configuration);

var app = builder.Build();

app.MapShortenLinkEndpoints();

app.Run();
```

Minimum `appsettings.json` configuration for SQLite default mode:

```json
{
  "ShortenLink": {
    "BaseUrl": "https://localhost:5001",
    "Database": {
      "UsePostgres": false,
      "SqliteConnectionString": "Data Source=shorten-link.db",
      "PostgresConnectionString": "Host=localhost;Port=5432;Database=shorten_link;Username=postgres;Password=postgres"
    },
    "Redirect": {
      "EnableFrontendFallback": true,
      "FrontendFallbackPath": "/not-found"
    }
  }
}
```

### Direct Service Usage

After the core service task lands, application services can depend on the reusable `IShortLinkService` contract directly. The intended shape is:

```csharp
using ShortenLink.Core.Services;

public sealed class MyLinkService
{
    private readonly IShortLinkService _shortLinkService;

    public MyLinkService(IShortLinkService shortLinkService)
    {
        _shortLinkService = shortLinkService;
    }

    public Task<CreateShortLinkResult> CreateAsync(string url, CancellationToken cancellationToken = default)
    {
        return _shortLinkService.CreateAsync(
            new CreateShortLinkRequest(url),
            cancellationToken);
    }
}
```

Switch to PostgreSQL by configuration only:

```json
{
  "ShortenLink": {
    "Database": {
      "UsePostgres": true,
      "PostgresConnectionString": "Host=localhost;Port=5432;Database=shorten_link;Username=postgres;Password=postgres"
    }
  }
}
```

The demo host still uses `AddShortenLink(builder.Configuration);` with no application-code changes. On startup it calls `EnsureCreated()` for the selected provider, so SQLite remains the default local path while PostgreSQL can be enabled with a valid connection string.

`IShortLinkService`, `CreateShortLinkRequest`, and `CreateShortLinkResult` live in `ShortenLink.Core.Services`. Consumer code should continue to call the reusable service contract instead of re-creating short-link rules in the host app.

## Demo API Swagger And Demo UI

The demo API uses Swashbuckle for development-time Swagger/OpenAPI.

```powershell
dotnet run --project src\ShortenLink.Api\ShortenLink.Api.csproj --launch-profile https
```

Open:

```text
https://localhost:7154/swagger
```

The API now exposes:

- `POST /api/short-links`
- `GET /api/short-links/{code}`
- `DELETE /api/short-links/{code}`
- `GET /{code}`
- `GET /api/health`

In development, `src\ShortenLink.Api\appsettings.Development.json` overrides `ShortenLink:BaseUrl` to `https://localhost:7154` and sets `ShortenLink:Redirect:FrontendFallbackPath` to `http://localhost:5173/not-found` so returned short URLs and unknown-code fallback both line up with the local split API + Vite setup.

## Frontend Demo

The React + Vite demo app now provides the Phase 001 create, copy, detail, deactivate, and fallback flow.

Start the API in one terminal:

```powershell
dotnet run --project src\ShortenLink.Api\ShortenLink.Api.csproj --launch-profile https
```

The `https` launch profile keeps both `https://localhost:7154` and `http://localhost:5188` available, so the returned short URLs and the Vite proxy target both work during local development and smoke runs.

Then start the frontend in another:

```powershell
cd .\src\ShortenLink.Web
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

The Vite dev server proxies `/api/*` requests to `http://localhost:5188` by default. Override that target when needed:

```powershell
$env:SHORTENLINK_API_PROXY_TARGET = "http://localhost:5188"
npm run dev
```

For a production-style frontend build:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Dependencies are not vendored in this repo.

## PostgreSQL Notes

Phase 2 adds configuration-driven provider selection to the reusable library boundary:

- Leave `ShortenLink:Database:UsePostgres` as `false` to keep SQLite as the default provider.
- Set `ShortenLink:Database:UsePostgres` to `true` and provide `ShortenLink:Database:PostgresConnectionString` to switch the same host and library code to PostgreSQL.
- The reusable API, repository, and service contracts do not change between providers.
- `dotnet pack ShortenLink.slnx -c Release` still produces the same reusable packages; provider choice stays in configuration.
