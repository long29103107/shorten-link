---
id: 002_001
phase: 002
task: 001
title: PostgreSQL provider toggle MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: feature
priority: high
depends_on:
  - 001_008
tags:
  - postgresql
  - ef-core
  - provider-toggle
  - configuration
  - phase-2
---

# 002_001 - PostgreSQL Provider Toggle MVP

## Step Goal

Enable the reusable Shorten Link library and demo API to select either SQLite or PostgreSQL through configuration only, while keeping the existing service, repository, endpoint, and package-consumption surface unchanged.

This step should turn the Phase 2 goal into a real runtime capability instead of a placeholder config value that is ignored or rejected during startup.

## Dependency

- `001_005` completed the EF Core persistence model and repository boundary that provider selection must reuse.
- `001_006` completed the reusable ASP.NET Core DI surface through `AddShortenLink(...)`.
- `001_008` completed Phase 001 closure and confirmed the current SQLite/API/Web baseline before provider-toggle work begins.

## Scope

In:

- Add PostgreSQL EF Core provider support to the reusable infrastructure/library surface.
- Extend `ShortenLink` options so PostgreSQL connection-string settings are represented explicitly.
- Update `AddShortenLink(...)` to select SQLite or PostgreSQL from configuration only.
- Keep SQLite as the default provider when `UsePostgres` is `false`.
- Require a PostgreSQL connection string when `UsePostgres` is `true`.
- Preserve repository, service, endpoint, and consumer setup contracts.
- Add verification/tests for provider selection and PostgreSQL-capable model wiring where a live PostgreSQL server is not required.
- Update README with PostgreSQL configuration and local run guidance.

Out:

- Do not add Redis, analytics, workers, rate limiting, Docker Compose, or authentication.
- Do not require a running PostgreSQL instance for the default local SQLite path.
- Do not redesign repository/service contracts or move business logic out of the reusable library.
- Do not add full migration automation beyond the minimum documentation or verification needed for this MVP.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/002/PHASE_SUMMARY.md`
- `.okf/phase/002/002_001-postgresql-provider-toggle-mvp.md`
- `src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj`
- `src/ShortenLink.AspNetCore/ShortenLinkOptions.cs`
- `src/ShortenLink.AspNetCore/ShortenLinkServiceCollectionExtensions.cs`
- `src/ShortenLink.Api/appsettings.json`
- `README.md`
- `tests/ShortenLink.Api.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/`

## Acceptance Criteria

- `ShortenLink:Database:UsePostgres = false` keeps SQLite selected by default.
- `ShortenLink:Database:UsePostgres = true` selects PostgreSQL without requiring application-code changes.
- PostgreSQL selection is driven only by configuration values passed through `AddShortenLink(...)`.
- Startup validation rejects `UsePostgres = true` when the PostgreSQL connection string is missing or blank.
- Existing repository/service/API contracts remain unchanged for consumers.
- The EF Core model still defines the required indexes for short-link persistence, and provider-selection verification covers both SQLite and PostgreSQL-capable wiring where practical.
- README documents SQLite default behavior, PostgreSQL config shape, and the command path or notes needed for local PostgreSQL usage.
- `dotnet build ShortenLink.slnx`, `dotnet test ShortenLink.slnx`, and `dotnet pack ShortenLink.slnx -c Release` pass after the change.

## Foundation for Next Step

This step leaves the library with a real provider-selection boundary. The next task can build migrations, README run commands, or stronger PostgreSQL integration checks on top of a stable config-driven toggle instead of reworking the DI/persistence contract again.

## Verification

Run from the repository root:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If the implementation adds provider-selection tests at the project level, run the smallest relevant test project first while iterating.

## Done Notes

Completed on 2026-07-12.

Implemented:

- Added PostgreSQL EF Core provider support to the reusable infrastructure package with `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.3`.
- Extended `ShortenLinkDatabaseOptions` with explicit `PostgresConnectionString` support.
- Updated `AddShortenLink(...)` option validation so provider selection is configuration-driven instead of hard-blocking PostgreSQL.
- Updated `AddShortenLink(...)` DbContext registration to choose SQLite or PostgreSQL at runtime from `ShortenLink:Database:UsePostgres`.
- Added provider-selection tests that verify SQLite remains the default, PostgreSQL can be selected without host code changes, and missing PostgreSQL connection strings are rejected.
- Added a PostgreSQL-provider model test to confirm the expected short-link indexes remain part of the EF Core model under provider selection.
- Updated README configuration samples and Phase 2 notes to document the PostgreSQL toggle and setup expectations.

Verification:

- `dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --verbosity minimal` passed with 6 tests.
- `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal` passed with 12 tests.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 46 total tests: 28 core, 6 infrastructure, and 12 API.
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal` passed for the reusable packages. The demo API remained intentionally non-packable and emitted the expected informational warning from NuGet pack targets.

Notes:

- The first package restore failed because `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.4` was not published on NuGet. The final implementation uses `10.0.3`, which the NuGet package page lists as the current .NET 10-compatible stable release.
- This task verified provider selection and PostgreSQL-capable model wiring without a live PostgreSQL instance. A follow-up task should cover real host startup/smoke against PostgreSQL where the environment allows it.
