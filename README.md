# Shorten Link

Reusable .NET short-link library plus a demo ASP.NET Core API and React frontend.

The reusable library projects are intentionally separated from the demo application so they can be packed as NuGet packages and consumed by other .NET applications. `ShortenLink.AspNetCore` is the normal package for ASP.NET Core hosts; the demo API and React app exist to prove the package behavior, not to own short-link business logic.

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

## Package Surface

| Package | Use when |
|---|---|
| `ShortenLink.AspNetCore` | You are building an ASP.NET Core host and want DI registration, options binding, endpoint mapping, redirect fallback, analytics worker integration, cache wiring, and rate limiting. Start here for normal API projects. |
| `ShortenLink.Core` | You need direct access to reusable domain models, validation, service contracts, request/result types, or `IShortLinkService` from non-host code. |
| `ShortenLink.Infrastructure` | You are composing persistence manually or extending provider wiring. Most ASP.NET Core hosts receive it transitively through `ShortenLink.AspNetCore`. |

`ShortenLink.Api` and `ShortenLink.Web` are demo applications and are not part of the reusable package surface.

## Build, Test, And Pack

Use these commands from the repository root.

### Build And Test

```powershell
dotnet build ShortenLink.slnx
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

### Release-Readiness Verification

Before handing local packages to a consumer app, run:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
.\scripts\release-dry-run.ps1
.\scripts\smoke-consumer-package.ps1
```

The build/test/pack commands validate the repository and package artifacts. The consumer smoke creates a clean app, installs the packaged `ShortenLink.AspNetCore` entry point from a local package source, and verifies create, detail, redirect, deactivate, and post-delete redirect behavior without using demo API internals.

The release dry-run script packs the reusable packages into `.tmp\release-dry-run`, inspects the package metadata and contents, confirms `README.md` is included, checks dependency shape, and verifies that demo API/Web artifacts are not coupled into the reusable packages. It never publishes to NuGet; passing `-Publish` fails closed.

Keep the dry-run package artifacts for inspection when needed:

```powershell
.\scripts\release-dry-run.ps1 -KeepArtifacts
```

### Release Checklist

Use this checklist before any future real package publish:

- Review package versions and release notes for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Complete the maintainer preflight in `docs\nuget-publish-preflight.md`, including package ID ownership, account or organization ownership, API key scope, version availability, and release approval.
- Run `dotnet build ShortenLink.slnx --verbosity minimal`.
- Run `dotnet test ShortenLink.slnx --verbosity minimal`.
- Run `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`.
- Run `.\scripts\release-dry-run.ps1` and confirm it reports `Published: false`.
- Run `.\scripts\rehearse-local-feed.ps1 -PackageVersion <version> -ResetFeed` to prove the publish path against a local folder feed.
- Run `.\scripts\smoke-consumer-package.ps1` to validate a clean ASP.NET Core consumer installation.
- If publishing later, use a separate explicit publish command or workflow with a real NuGet API key, manual approval, and the package artifacts inspected by the dry-run.

NuGet publishing is intentionally out of scope for the default verification path. No script in this repository should publish packages unless a later task adds a credential-protected publish workflow deliberately.

### Manual NuGet Publish Workflow

Publishing is a maintainer-only operation. The default release commands stay dry-run-only, and `scripts\publish-nuget.ps1` only calls `dotnet nuget push` when a maintainer supplies both explicit intent and credentials.

Before any publish attempt:

- Complete `docs\nuget-publish-preflight.md` and stop if package ID ownership, credentials, version choice, or maintainer approval is missing.
- Review the package version and confirm it has not already been pushed to NuGet.
- Review release notes and public package metadata.
- Run `dotnet build ShortenLink.slnx --verbosity minimal`.
- Run `dotnet test ShortenLink.slnx --verbosity minimal`.
- Run `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`.
- Run `.\scripts\release-dry-run.ps1 -PackageVersion <version>` and confirm it reports `Published: false`.
- Run `.\scripts\smoke-consumer-package.ps1 -PackageVersion <version>`.

Preview the publish command without pushing packages:

```powershell
.\scripts\publish-nuget.ps1 -PackageVersion 1.0.0
```

To publish intentionally, provide the API key from the environment or another secret store and pass `-Publish`:

```powershell
$env:NUGET_API_KEY = "<set outside source control>"
.\scripts\publish-nuget.ps1 -PackageVersion 1.0.0 -Publish
```

Use `-SkipDuplicate` only when retrying a partially completed publish and after confirming the already-published package version is expected:

```powershell
.\scripts\publish-nuget.ps1 -PackageVersion 1.0.0 -Publish -SkipDuplicate
```

The publish script fails closed when `-Publish` is missing or no NuGet API key is available. When publishing is enabled, it reruns the release dry-run into `.tmp\nuget-publish` before pushing `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.

After publishing, verify the packages on NuGet, install `ShortenLink.AspNetCore` into a clean consumer app, and run the create, detail, redirect, and deactivate smoke flow again. If a bad package is published, prefer deprecating or unlisting the affected version and publishing a corrected version; do not overwrite the same NuGet version.

### Local Feed Publish Rehearsal

Before using real NuGet credentials, rehearse the publish path against a local folder feed:

```powershell
.\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0 -ResetFeed
```

The rehearsal validates packages with `release-dry-run.ps1`, copies `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` into `.tmp\local-nuget-feed`, then runs the clean consumer smoke against that existing feed. It does not call `dotnet nuget push`, does not require credentials, and never publishes to NuGet.org.

If the feed already contains the same package version, the script fails closed. Start a clean rehearsal feed with `-ResetFeed`, or intentionally retry against existing packages with `-SkipDuplicate`:

```powershell
.\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0 -SkipDuplicate
```

Keep rehearsal artifacts for inspection when needed:

```powershell
.\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0 -ResetFeed -KeepArtifacts
```

## Use From Another .NET App

Most ASP.NET Core consumers should start with `ShortenLink.AspNetCore`. That package is the host-facing entry point for dependency injection and endpoint mapping. It brings the lower-level reusable projects with it through package/project references.

### Consumer Package Smoke

To validate the package from a clean consumer app shape, run:

```powershell
.\scripts\smoke-consumer-package.ps1
```

The smoke script packs the reusable packages into a temporary local NuGet source, creates a clean ASP.NET Core app under `.tmp`, installs `ShortenLink.AspNetCore`, maps the library endpoints, runs SQLite default mode, and verifies create, detail, redirect, and deactivate behavior. It does not reference `ShortenLink.Api`, and it does not require PostgreSQL, Redis, Docker, frontend assets, credentials, or package publishing.

To smoke an existing local package source without regenerating it:

```powershell
.\scripts\smoke-consumer-package.ps1 -PackageSource .\.tmp\local-nuget-feed -UseExistingPackageSource
```

Keep the generated consumer app and local package source for inspection when needed:

```powershell
.\scripts\smoke-consumer-package.ps1 -KeepArtifacts
```

### Option 1: Project Reference During Local Development

From the consumer app directory:

```powershell
dotnet add reference ..\shorten-link\src\ShortenLink.AspNetCore\ShortenLink.AspNetCore.csproj
```

### Option 2: Install From A Local NuGet Folder

Create a local package folder that contains all reusable packages:

```powershell
New-Item -ItemType Directory -Force .\.nupkg
dotnet pack ..\shorten-link\ShortenLink.slnx -c Release -o .\.nupkg
```

Add that folder as a NuGet source for the consumer app:

```powershell
dotnet nuget add source .\.nupkg --name shorten-link-local
```

Install the package:

```powershell
dotnet add package ShortenLink.AspNetCore --source .\.nupkg
```

If the consumer needs direct access to lower-level contracts, install/reference `ShortenLink.Core` as well. If it needs to compose EF Core persistence manually, install/reference `ShortenLink.Infrastructure`. Normal API hosts should start with `ShortenLink.AspNetCore`.

### ASP.NET Core Setup

In the consumer app's `Program.cs`:

```csharp
using ShortenLink.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddShortenLink(builder.Configuration);

var app = builder.Build();

app.UseShortenLinkRateLimiting();
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
    },
    "Analytics": {
      "Enabled": false,
      "UseAsyncWorker": true,
      "QueueCapacity": 512
    },
    "Cache": {
      "Enabled": false,
      "Provider": "Memory",
      "RedisConnectionString": "localhost:6379",
      "EntryTtlSeconds": 3600
    },
    "RateLimiting": {
      "Enabled": false,
      "Create": {
        "PermitLimit": 60,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "Redirect": {
        "PermitLimit": 120,
        "WindowSeconds": 60,
        "QueueLimit": 0
      }
    }
  }
}
```

### Direct Service Usage

Application services can depend on the reusable `IShortLinkService` contract directly. The intended shape is:

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

## Configuration Defaults And Optional Providers

SQLite is the safe default and requires no external infrastructure. PostgreSQL, Redis cache, click analytics, and rate limiting are opt-in through configuration. A consumer can install the same `ShortenLink.AspNetCore` package and choose behavior through `ShortenLink:*` settings instead of changing application code.

## Click Analytics

Phase 3 adds an opt-in click analytics path for redirects:

- Leave `ShortenLink:Analytics:Enabled` as `false` to keep redirect behavior unchanged with no click persistence.
- Set `ShortenLink:Analytics:Enabled` to `true` to capture short code, click timestamp, remote IP, user agent, and referrer for successful redirects.
- Leave `ShortenLink:Analytics:UseAsyncWorker` as `true` to enqueue analytics writes through the hosted background worker so redirect responses do not wait for database persistence.
- `ShortenLink:Analytics:QueueCapacity` controls the bounded in-memory queue used by the async worker.

Example configuration:

```json
{
  "ShortenLink": {
    "Analytics": {
      "Enabled": true,
      "UseAsyncWorker": true,
      "QueueCapacity": 512
    }
  }
}
```

## Redirect Cache

Phase 3 also adds an opt-in cache path for successful redirects:

- Leave `ShortenLink:Cache:Enabled` as `false` to keep redirect lookups database-backed.
- Set `ShortenLink:Cache:Enabled` to `true` and `ShortenLink:Cache:Provider` to `Memory` for a local in-process cache.
- Set `ShortenLink:Cache:Provider` to `Redis` and provide `ShortenLink:Cache:RedisConnectionString` to use Redis without changing application code.
- `ShortenLink:Cache:EntryTtlSeconds` controls cache duration for links that do not have their own expiration.
- Deactivating a link invalidates its cache entry so previously cached redirects stop resolving.

Example memory-cache configuration:

```json
{
  "ShortenLink": {
    "Cache": {
      "Enabled": true,
      "Provider": "Memory",
      "RedisConnectionString": "localhost:6379",
      "EntryTtlSeconds": 3600
    }
  }
}
```

## Endpoint Rate Limiting

Phase 3 adds opt-in HTTP rate limiting for public create and redirect paths:

- Leave `ShortenLink:RateLimiting:Enabled` as `false` to keep current endpoint behavior.
- Set `ShortenLink:RateLimiting:Enabled` to `true` to apply independent fixed-window limits to create and redirect requests.
- `ShortenLink:RateLimiting:Create` applies to `POST /api/short-links`.
- `ShortenLink:RateLimiting:Redirect` applies to `GET /{code}` before cache lookup, database lookup, or click analytics recording.
- Over-limit requests return HTTP `429`.

Example configuration:

```json
{
  "ShortenLink": {
    "RateLimiting": {
      "Enabled": true,
      "Create": {
        "PermitLimit": 60,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "Redirect": {
        "PermitLimit": 120,
        "WindowSeconds": 60,
        "QueueLimit": 0
      }
    }
  }
}
```

Example Redis configuration:

```json
{
  "ShortenLink": {
    "Cache": {
      "Enabled": true,
      "Provider": "Redis",
      "RedisConnectionString": "localhost:6379",
      "EntryTtlSeconds": 3600
    }
  }
}
```

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

### Local PostgreSQL Host Smoke

Minimum prerequisites:

- A reachable PostgreSQL instance.
- A database and credentials that match your connection string.
- TCP access to the PostgreSQL host and port from this machine.

Example PowerShell environment override:

```powershell
$env:ShortenLink__Database__UsePostgres = "true"
$env:ShortenLink__Database__PostgresConnectionString = "Host=localhost;Port=5432;Database=shorten_link;Username=postgres;Password=postgres"
dotnet run --project src\ShortenLink.Api\ShortenLink.Api.csproj --no-launch-profile
```

For a repeatable host smoke run, use:

```powershell
.\scripts\smoke-postgres-host.ps1
```

Override the connection string or API URL when needed:

```powershell
.\scripts\smoke-postgres-host.ps1 -ConnectionString "Host=localhost;Port=5432;Database=shorten_link;Username=postgres;Password=postgres" -ApiUrl "http://127.0.0.1:5199"
```

The smoke script:

- checks that PostgreSQL is reachable before starting the API
- runs the demo host with `UsePostgres = true`
- verifies health, create, detail, redirect, and deactivate behavior
- returns a JSON summary on success

When PostgreSQL is not reachable, the script fails early with a concrete blocker message instead of pretending the host smoke passed.

## Local Operational Stack With Docker Compose

Phase 3 now includes an optional Docker Compose path for the demo API plus its operational dependencies:

- PostgreSQL for the configured database provider
- Redis for redirect cache
- async click analytics enabled by configuration
- endpoint rate limiting enabled by configuration

This stack is optional. The default non-Docker SQLite developer flow still works exactly as before.

### Start The Stack

From the repository root:

```powershell
docker compose up -d --build
```

The composed stack exposes:

- API: `http://localhost:5188`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

The compose path configures the API with environment variables only:

- `ShortenLink__Database__UsePostgres=true`
- `ShortenLink__Database__PostgresConnectionString=Host=postgres;Port=5432;Database=shorten_link;Username=postgres;Password=postgres`
- `ShortenLink__Cache__Enabled=true`
- `ShortenLink__Cache__Provider=Redis`
- `ShortenLink__Cache__RedisConnectionString=redis:6379`
- `ShortenLink__Analytics__Enabled=true`
- `ShortenLink__Analytics__UseAsyncWorker=true`
- `ShortenLink__RateLimiting__Enabled=true`

That keeps provider selection and operational behavior in configuration instead of application code.

### Stop The Stack

```powershell
docker compose down
```

To remove the PostgreSQL and Redis volumes as well:

```powershell
docker compose down -v
```

### Smoke Check The Stack

For a repeatable compose-backed smoke run:

```powershell
.\scripts\smoke-docker-compose.ps1
```

The script:

- validates the compose file with `docker compose config`
- starts the stack with `docker compose up -d --build`
- waits for `GET /api/health`
- verifies create, redirect, detail, and deactivate behavior
- shuts the stack down with `docker compose down -v`
- returns a JSON summary on success

Keep the stack running after the smoke when needed:

```powershell
.\scripts\smoke-docker-compose.ps1 -KeepRunning
```

Point the smoke script at a different compose file or API URL when needed:

```powershell
.\scripts\smoke-docker-compose.ps1 -ComposeFile .\compose.yml -ApiUrl http://127.0.0.1:5188
```

### Default SQLite Path Still Works

Docker Compose does not replace the default local path. Outside Docker, leave:

```json
{
  "ShortenLink": {
    "Database": {
      "UsePostgres": false,
      "SqliteConnectionString": "Data Source=shorten-link.db"
    }
  }
}
```

Then continue using the normal local host flow:

```powershell
dotnet run --project src\ShortenLink.Api\ShortenLink.Api.csproj --launch-profile https
```
