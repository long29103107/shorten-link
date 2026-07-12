---
id: 001_005
phase: 001
task: 005
title: SQLite repository persistence MVP
status: done
created_at: 2026-07-11
completed_at: 2026-07-11
owner: codex
type: feature
priority: high
depends_on:
  - 001_004
tags:
  - sqlite
  - ef-core
  - persistence
  - integration-tests
---

# 001_005 - SQLite Repository Persistence MVP

## Step Goal

Implement the first real `ShortenLink.Infrastructure` persistence adapter for the verified core contracts: EF Core SQLite storage, required indexes, repository behavior, and SQLite integration tests.

This task should prove that the core `IShortLinkRepository` contract can persist and retrieve `ShortLink` records through SQLite without moving business rules into the demo API or Web projects.

## Dependency

- `001_004` completed the reusable core `ShortLink` model, `IShortLinkRepository`, `IShortLinkService`, validation helpers, Base62 generator, and focused core tests.

## Scope

In:

- Add EF Core infrastructure for `ShortLink` persistence in `ShortenLink.Infrastructure`.
- Add SQLite provider package support to the infrastructure project.
- Add a `ShortenLinkDbContext` or equivalent infrastructure DbContext.
- Map the core `ShortLink` model or a persistence entity while preserving the core repository contract.
- Implement `IShortLinkRepository` for SQLite-backed storage.
- Add required indexes for short code uniqueness and practical lookup fields such as created/expiry/active state where appropriate.
- Add SQLite integration tests that create a real SQLite database, persist links, enforce unique codes, retrieve by code, update/deactivate records, and verify expiry/active fields are stored.
- Keep SQLite as the Phase 1 default path; do not add PostgreSQL toggle behavior yet.

Out:

- Do not implement PostgreSQL support or provider switching in this task.
- Do not implement ASP.NET Core DI registration beyond compile-only plumbing if needed for infrastructure tests.
- Do not implement API endpoints in the demo host yet.
- Do not implement React UI flows yet.
- Do not add migrations, Docker Compose, Redis, analytics, rate limiting, or CI.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj`
- `tests/ShortenLink.Infrastructure.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/ShortenLink.Infrastructure.Tests.csproj`
- `ShortenLink.slnx` only if test project membership needs correction

## Acceptance Criteria

- `ShortenLink.Infrastructure` references EF Core SQLite without adding persistence dependencies to `ShortenLink.Core`.
- A DbContext or equivalent EF Core persistence boundary exists for short links.
- `IShortLinkRepository` has a SQLite-backed implementation.
- Short-link code uniqueness is enforced at the database/index level.
- Integration tests prove add, find, exists, and update/deactivate behavior against SQLite.
- Integration tests prove duplicate short codes fail through the persistence layer.
- Stored records preserve original URL, active state, created timestamp, and optional expiry.
- `ShortenLink.Core` remains free of EF Core and SQLite package references.
- The solution builds and relevant infrastructure tests pass.

## Foundation for Next Step

This step gives the ASP.NET Core integration task a concrete persistence adapter to register through DI. The next task can wire `AddShortenLink(...)` to SQLite-backed services and begin mapping real API endpoints without inventing a repository implementation inside the demo API.

## Implementation Notes

Prefer keeping EF Core-specific mapping and configuration inside `ShortenLink.Infrastructure`. If the core `ShortLink` type is awkward for EF Core construction, use an internal persistence entity and map to/from the core model rather than weakening the core domain shape.

Use real SQLite for integration tests, either a temporary file database or an in-memory SQLite connection kept open for the test lifetime. Avoid EF Core's non-relational in-memory provider because this task needs SQLite/index behavior.

## Verification

Run the smallest relevant checks:

```powershell
dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj
```

Then verify the solution still builds:

```powershell
dotnet build ShortenLink.slnx
```

If package references change, verify packability:

```powershell
dotnet pack ShortenLink.slnx -c Release
```

## Done Notes

Completed on 2026-07-11.

Implemented:

- Added `Microsoft.EntityFrameworkCore.Sqlite` to `ShortenLink.Infrastructure`.
- Added a direct `SQLitePCLRaw.lib.e_sqlite3` package override to avoid the vulnerable transitive SQLite native package warning.
- Added `ShortLinkDbContext` with a `short_links` table, required fields, primary key on code, unique code index, and indexes for created time, expiry, and active state.
- Added `ShortLinkRecord` as the EF persistence entity and mapper to/from the core `ShortLink` domain model.
- Added `EfCoreShortLinkRepository` implementing `IShortLinkRepository` with add, find, exists, and update behavior.
- Added xUnit-based SQLite integration tests using a real in-memory SQLite connection kept open for the test lifetime.
- Verified add/find, exists/missing, deactivate update, duplicate-code database enforcement, stored field preservation, and expected SQLite indexes.
- Confirmed `ShortenLink.Core` remains free of EF Core and SQLite references.

Verification:

- `dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --verbosity minimal` passed with 5 tests.
- `dotnet build ShortenLink.slnx --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --no-restore --verbosity minimal` passed with 33 tests total: 28 core tests and 5 infrastructure tests.
- `dotnet pack ShortenLink.slnx -c Release --no-restore` passed. It reported the expected `IsPackable=false` warning for `ShortenLink.Api`, which is the demo host and should not produce a reusable package.
