---
id: 001_006
phase: 001
task: 006
title: ASP.NET Core DI and short-link endpoints MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: feature
priority: high
depends_on:
  - 001_005
tags:
  - aspnet-core
  - minimal-api
  - dependency-injection
  - endpoint-tests
---

# 001_006 - ASP.NET Core DI And Short-Link Endpoints MVP

## Step Goal

Wire the verified core service and SQLite repository into the reusable `ShortenLink.AspNetCore` package and expose the first real short-link HTTP contracts through `MapShortenLinkEndpoints()`.

This task should prove that a host application can call `builder.Services.AddShortenLink(builder.Configuration)` and `app.MapShortenLinkEndpoints()` to get create, detail, delete/deactivate, redirect, and unknown-code fallback behavior without reimplementing short-link business logic in `ShortenLink.Api`.

## Dependency

- `001_004` completed the reusable core model, service contract, validation, code generator, and service behavior.
- `001_005` completed the EF Core SQLite persistence adapter, repository implementation, required indexes, and SQLite integration tests.
- `001_003` added Swagger/OpenAPI to the demo API so the new mapped endpoints can be inspected from the demo host.

## Scope

In:

- Register `IShortCodeGenerator`, `IShortLinkService`, `IShortLinkRepository`, `ShortLinkDbContext`, and SQLite defaults through `AddShortenLink(...)`.
- Add configuration binding for Phase 1 options needed by endpoint behavior, including base URL, SQLite connection string, and redirect fallback settings.
- Map `POST /api/short-links` for creating short links.
- Map `GET /api/short-links/{code}` for detail lookup.
- Map `DELETE /api/short-links/{code}` for deactivation.
- Map `GET /{code}` for active-link redirect.
- Implement unknown-code behavior for the redirect endpoint using frontend fallback config when enabled and JSON 404 when disabled.
- Return API-friendly response DTOs and error payloads from endpoint handlers.
- Keep endpoint handlers thin and delegate reusable behavior to `IShortLinkService`.
- Add focused API tests for create, detail, delete/deactivate, redirect, duplicate alias, expired or inactive behavior where supported by the service, and unknown-code fallback modes.
- Keep the demo `ShortenLink.Api` host as a consumer of the reusable ASP.NET Core package.

Out:

- Do not implement React UI flows in this task.
- Do not add PostgreSQL provider selection or migrations yet.
- Do not add analytics click tracking, Redis/cache, rate limiting, Docker Compose, worker infrastructure, or CI.
- Do not move business rules into `ShortenLink.Api`.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `src/ShortenLink.AspNetCore/`
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`
- `src/ShortenLink.Api/Program.cs`
- `src/ShortenLink.Api/appsettings.json`
- `src/ShortenLink.Api/appsettings.Development.json`
- `tests/ShortenLink.Api.Tests/`
- `tests/ShortenLink.Api.Tests/ShortenLink.Api.Tests.csproj`
- `README.md` only if public endpoint or configuration examples need to stay aligned

## Acceptance Criteria

- `AddShortenLink(...)` registers the core service, generator, SQLite DbContext, and EF Core repository using safe default configuration.
- `MapShortenLinkEndpoints()` exposes create, detail, delete/deactivate, and redirect endpoints from the reusable ASP.NET Core package.
- `POST /api/short-links` accepts `originalUrl`, optional `customAlias`, and optional `expiredAtUtc`, then returns code, short URL, original URL, and created timestamp.
- Duplicate custom alias requests return a client error without creating a second link.
- Invalid URLs and invalid aliases return client errors with stable API-friendly payloads.
- `GET /api/short-links/{code}` returns details for existing links and a JSON 404 for missing links.
- `DELETE /api/short-links/{code}` deactivates an existing link and reports missing links consistently.
- `GET /{code}` redirects active, unexpired links to the original URL.
- Unknown short codes follow `ShortenLink:Redirect:EnableFrontendFallback` and `ShortenLink:Redirect:FrontendFallbackPath`.
- The demo API consumes `AddShortenLink(...)` and `MapShortenLinkEndpoints()` without endpoint business logic in `Program.cs`.
- API tests cover the new HTTP contracts against a SQLite-backed test host.
- The solution builds and relevant API tests pass.

## Foundation for Next Step

This step gives Phase 001 a reusable HTTP integration surface that the React demo can consume directly. The next task can focus on frontend create/detail/fallback UX without inventing parallel API routes or duplicating short-link rules in the web app.

## Implementation Notes

Prefer Minimal API route groups and small endpoint DTOs inside `ShortenLink.AspNetCore`. Keep host-specific configuration in the demo app limited to appsettings values and normal ASP.NET Core service registration.

Use a temporary SQLite database or in-memory SQLite connection for API tests so the tests exercise the real repository and endpoint stack. Avoid EF Core's non-relational in-memory provider for endpoint tests that depend on uniqueness or persistence behavior.

## Verification

Run the smallest relevant checks:

```powershell
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj
```

Then verify the solution still builds:

```powershell
dotnet build ShortenLink.slnx
```

If package or public ASP.NET Core integration behavior changes, verify packability:

```powershell
dotnet pack ShortenLink.slnx -c Release
```

## Done Notes

Completed on 2026-07-12.

Implemented:

- Added `ShortenLinkOptions` with SQLite and redirect-fallback settings bound from `ShortenLink` configuration.
- Implemented `AddShortenLink(...)` to register SQLite `ShortLinkDbContext`, EF Core repository, short-code generator, core short-link service, and startup database initialization.
- Implemented `MapShortenLinkEndpoints()` as the reusable ASP.NET Core HTTP surface for create, detail, deactivate, and redirect flows.
- Added stable JSON error payloads for validation, duplicate alias, missing, inactive, and expired cases.
- Kept `ShortenLink.Api` as a thin demo host that only wires Swagger, `AddShortenLink(...)`, and `MapShortenLinkEndpoints()`.
- Added API integration tests using `WebApplicationFactory` and a SQLite-backed test host to cover create, duplicate alias, invalid URL, details, deactivate, redirect, and frontend-fallback behavior.

Verification:

- `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal` passed with 8 tests.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 41 tests total: 28 core, 5 infrastructure, and 8 API.
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal` passed and produced `ShortenLink.AspNetCore.1.0.0.nupkg`. It reported the expected `IsPackable=false` warning for `ShortenLink.Api`.
