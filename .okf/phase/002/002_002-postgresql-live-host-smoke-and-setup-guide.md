---
id: 002_002
phase: 002
task: 002
title: PostgreSQL live host smoke and setup guide
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: validation
priority: high
depends_on:
  - 002_001
tags:
  - postgresql
  - smoke-test
  - documentation
  - setup
  - phase-2
---

# 002_002 - PostgreSQL Live Host Smoke And Setup Guide

## Step Goal

Prove that the verified PostgreSQL provider toggle from `002_001` works against a real PostgreSQL instance in the demo host, then finish the remaining setup and run guidance needed for developers to reproduce that flow locally.

This step should convert the current provider-selection capability from configuration-level confidence into host-level evidence for Phase 002 closure.

## Dependency

- `002_001` completed configuration-driven provider selection for SQLite and PostgreSQL without changing the library/API contracts.
- `001_008` completed the SQLite-backed local host and browser smoke baseline that PostgreSQL startup must now match.

## Scope

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

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/002/PHASE_SUMMARY.md`
- `.okf/phase/002/002_002-postgresql-live-host-smoke-and-setup-guide.md`
- `README.md`
- `src/ShortenLink.Api/appsettings.json`
- `src/ShortenLink.Api/appsettings.Development.json`
- `tests/ShortenLink.Api.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/`

## Acceptance Criteria

- The demo host can start successfully with `ShortenLink:Database:UsePostgres = true` and a valid PostgreSQL connection string where the environment allows.
- PostgreSQL-backed create/detail/delete-deactivate/redirect behavior is verified through the live host path or the task records the exact external blocker that prevented it.
- PostgreSQL setup and run guidance in `README.md` is sufficient for another developer to reproduce the host path without changing application code.
- Phase 002 closure status is clearer after this step:
  - if the live host path is verified and docs are complete, the phase can close or move to the smallest remaining cleanup step;
  - if not, the remaining blocker is documented concretely.

## Foundation for Next Step

This step leaves Phase 002 with real PostgreSQL host evidence instead of only provider-wiring tests. The next step can either close Phase 002 or address one last concrete PostgreSQL gap without reopening provider-selection design.

## Verification

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

## Done Notes

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
