---
phase: 002
title: PostgreSQL Provider Toggle
created_at: 2026-07-09
updated_at: 2026-07-12
status: complete
current_task: null
task_count: 2
done_count: 2
depends_on:
  - 001
---

# Phase 002 Summary

## Phase Goal

Allow the same reusable library and demo API to switch between SQLite and PostgreSQL by configuration only, without changing application code or weakening the NuGet package boundary.

## Phase Done Criteria

- SQLite remains the default provider.
- PostgreSQL can be enabled with `ShortenLink:Database:UsePostgres = true`.
- Provider selection is controlled by configuration, not code changes.
- Repository/service/API contracts remain unchanged.
- Required indexes exist for both providers where practical.
- README documents PostgreSQL setup, configuration, and migration/update commands.
- Verification covers provider selection and PostgreSQL setup where the local environment allows.

## Scope

In:

- PostgreSQL EF Core provider support.
- Configuration toggle and options.
- Migration/update database guidance.
- Provider-selection tests or verification.
- README updates.

Out:

- Redis cache provider.
- Analytics worker.
- Rate limiting.
- Docker Compose unless needed only as local PostgreSQL helper documentation.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 002_001 | PostgreSQL provider toggle MVP | done | 2026-07-12 |
| 002_002 | PostgreSQL live host smoke and setup guide | done | 2026-07-12 |

## Current Task

No task is active. Phase 002 is complete.

## Completed Notes

- `002_001` completed on 2026-07-12. Added configuration-driven PostgreSQL provider selection to the reusable library boundary, extended options validation and DbContext wiring, added provider-selection and PostgreSQL-model tests, and updated README PostgreSQL guidance while keeping SQLite as the default path.
- `002_002` completed on 2026-07-12. Added a reusable PostgreSQL host smoke script, documented the PostgreSQL prerequisites and run path, verified the exact local blocker when no PostgreSQL instance was reachable, and confirmed the repo-side Phase 002 verification set still passed.

## Next Task Proposal

Create `003_001 - async click analytics MVP` next.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 002_001 - PostgreSQL Provider Toggle MVP

Source before compaction: `002_001-postgresql-provider-toggle-mvp.md`

#### Step Goal

Enable the reusable Shorten Link library and demo API to select either SQLite or PostgreSQL through configuration only, while keeping the existing service, repository, endpoint, and package-consumption surface unchanged.

This step should turn the Phase 2 goal into a real runtime capability instead of a placeholder config value that is ignored or rejected during startup.

#### Dependency

- `001_005` completed the EF Core persistence model and repository boundary that provider selection must reuse.
- `001_006` completed the reusable ASP.NET Core DI surface through `AddShortenLink(...)`.
- `001_008` completed Phase 001 closure and confirmed the current SQLite/API/Web baseline before provider-toggle work begins.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

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

#### Acceptance Criteria

- `ShortenLink:Database:UsePostgres = false` keeps SQLite selected by default.
- `ShortenLink:Database:UsePostgres = true` selects PostgreSQL without requiring application-code changes.
- PostgreSQL selection is driven only by configuration values passed through `AddShortenLink(...)`.
- Startup validation rejects `UsePostgres = true` when the PostgreSQL connection string is missing or blank.
- Existing repository/service/API contracts remain unchanged for consumers.
- The EF Core model still defines the required indexes for short-link persistence, and provider-selection verification covers both SQLite and PostgreSQL-capable wiring where practical.
- README documents SQLite default behavior, PostgreSQL config shape, and the command path or notes needed for local PostgreSQL usage.
- `dotnet build ShortenLink.slnx`, `dotnet test ShortenLink.slnx`, and `dotnet pack ShortenLink.slnx -c Release` pass after the change.

#### Foundation for Next Step

This step leaves the library with a real provider-selection boundary. The next task can build migrations, README run commands, or stronger PostgreSQL integration checks on top of a stable config-driven toggle instead of reworking the DI/persistence contract again.

#### Verification

Run from the repository root:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If the implementation adds provider-selection tests at the project level, run the smallest relevant test project first while iterating.

#### Done Notes

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

### 002_002 - PostgreSQL Live Host Smoke And Setup Guide

Source before compaction: `002_002-postgresql-live-host-smoke-and-setup-guide.md`

#### Step Goal

Prove that the verified PostgreSQL provider toggle from `002_001` works against a real PostgreSQL instance in the demo host, then finish the remaining setup and run guidance needed for developers to reproduce that flow locally.

This step should convert the current provider-selection capability from configuration-level confidence into host-level evidence for Phase 002 closure.

#### Dependency

- `002_001` completed configuration-driven provider selection for SQLite and PostgreSQL without changing the library/API contracts.
- `001_008` completed the SQLite-backed local host and browser smoke baseline that PostgreSQL startup must now match.

#### Scope

In:

- Run the demo API with `ShortenLink:Database:UsePostgres = true` against a real PostgreSQL instance where the environment allows.
- Verify host startup, database initialization, and minimum short-link CRUD/redirect behavior on PostgreSQL.
- Confirm the required short-link indexes and schema behavior exist under the PostgreSQL provider where practical.
- Document the PostgreSQL local setup path, configuration, run command, and any environment prerequisites in `README.md`.
- Add or refine verification notes/tests/scripts only where they materially improve reproducibility for the PostgreSQL host path.
- Update Phase 002 bookkeeping based on the verified result.

Out:

- Do not redesign provider selection or repository contracts again unless smoke exposes a real defect.
- Do not add Docker Compose, Redis, analytics, workers, rate limiting, or authentication in this task.
- Do not require PostgreSQL for the default SQLite developer path.
- Do not fabricate live-smoke success if the environment cannot provide a PostgreSQL instance; document the blocker precisely instead.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/002/PHASE_SUMMARY.md`
- `.okf/phase/002/002_002-postgresql-live-host-smoke-and-setup-guide.md`
- `README.md`
- `src/ShortenLink.Api/appsettings.json`
- `src/ShortenLink.Api/appsettings.Development.json`
- `tests/ShortenLink.Api.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/`

#### Acceptance Criteria

- The demo host can start successfully with `ShortenLink:Database:UsePostgres = true` and a valid PostgreSQL connection string where the environment allows.
- PostgreSQL-backed create/detail/delete-deactivate/redirect behavior is verified through the live host path or the task records the exact external blocker that prevented it.
- PostgreSQL setup and run guidance in `README.md` is sufficient for another developer to reproduce the host path without changing application code.
- Phase 002 closure status is clearer after this step:
  - if the live host path is verified and docs are complete, the phase can close or move to the smallest remaining cleanup step;
  - if not, the remaining blocker is documented concretely.

#### Foundation for Next Step

This step leaves Phase 002 with real PostgreSQL host evidence instead of only provider-wiring tests. The next step can either close Phase 002 or address one last concrete PostgreSQL gap without reopening provider-selection design.

#### Verification

Run the smallest relevant checks first while iterating, then the full Phase 002 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

When a PostgreSQL instance is available, also smoke-check the demo host with:

- `ShortenLink:Database:UsePostgres = true`
- a valid `ShortenLink:Database:PostgresConnectionString`
- create/detail/delete-deactivate/redirect behavior against the running API

#### Done Notes

Completed on 2026-07-12.

Implemented:

- Added `scripts/smoke-postgres-host.ps1` as a repeatable PostgreSQL host smoke path for the demo API.
- Updated `README.md` with PostgreSQL prerequisites, environment-override examples, and exact smoke-script usage.
- Validated that the smoke script fails early with a concrete blocker message when PostgreSQL is not reachable instead of reporting a false-positive host smoke result.

Verification:

- `powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\smoke-postgres-host.ps1` failed early with the concrete blocker `PostgreSQL is not reachable at localhost:5432. Start a PostgreSQL instance first, then rerun this script.`
- `psql --version` failed because `psql` is not installed in this environment.
- `docker ps` could not access a usable local Docker daemon in this environment.
- `Test-NetConnection localhost -Port 5432` confirmed PostgreSQL was not reachable locally.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 46 total tests: 28 core, 6 infrastructure, and 12 API.
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal` passed for the reusable packages. The demo API remained intentionally non-packable and emitted the expected informational warning from NuGet pack targets.

Notes:

- This environment could not provide a live PostgreSQL instance, so the host smoke result is a verified external blocker rather than a code failure.
- Phase 002 now includes the provider toggle, PostgreSQL-capable model/provider verification, setup documentation, and a reproducible host-smoke command for environments where PostgreSQL is available.


## Scan Rule
Agents must read this file before working on any `002_*` task note. Do not activate Phase 002 until Phase 001 is complete.
