---
id: 003_004
phase: 003
task: 004
title: Local operational stack and Docker Compose MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: operations
priority: high
depends_on:
  - 003_003
tags:
  - docker
  - docker-compose
  - local-stack
  - postgresql
  - redis
  - phase-3
---

# 003_004 - Local Operational Stack And Docker Compose MVP

## Step Goal

Add a reproducible local operational stack for the demo API and its production-readiness dependencies, so developers can run the Shorten Link host with PostgreSQL, Redis-backed cache, analytics, and rate limiting settings from documented Docker Compose commands.

This step should convert the verified configuration toggles from Phase 002 and Phase 003 into a practical local stack without making Docker mandatory for the default SQLite developer path.

## Dependency

- `002_002` completed PostgreSQL setup guidance and host smoke script behavior.
- `003_001` completed async click analytics and persistence.
- `003_002` completed cache abstraction with memory and Redis provider selection.
- `003_003` completed endpoint rate limiting with safe disabled defaults and config-driven enablement.

## Scope

In:

- Add Dockerfile support for the demo API if it is not already present.
- Add Docker Compose support for the local operational stack.
- Include PostgreSQL as the configured database provider for the composed API.
- Include Redis as the configured cache provider for the composed API.
- Configure analytics and rate limiting through environment variables or compose configuration.
- Preserve the default local non-Docker SQLite path.
- Document compose startup, shutdown, environment variables, expected ports, and smoke commands in `README.md`.
- Add or update scripts only where they materially improve repeatable local smoke verification.
- Verify compose files and Docker artifacts structurally where the local environment allows, and document any external Docker daemon blocker precisely.

Out:

- Do not add GitHub Actions CI in this task.
- Do not add production deployment manifests, Kubernetes, cloud infrastructure, TLS termination, authentication, or observability dashboards.
- Do not require Docker for unit/integration tests or the default developer flow.
- Do not change reusable service, repository, cache, analytics, or rate limiting contracts unless Docker smoke exposes a concrete configuration defect.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_004-local-operational-stack-and-docker-compose-mvp.md`
- `README.md`
- `docker-compose.yml` or `compose.yml`
- `src/ShortenLink.Api/Dockerfile`
- `src/ShortenLink.Api/appsettings.json`
- `scripts/`
- `tests/ShortenLink.Api.Tests/`

## Acceptance Criteria

- A Docker Compose file defines the demo API plus PostgreSQL and Redis services.
- The composed API is configured to use PostgreSQL by configuration only.
- The composed API can use Redis cache by configuration only.
- Analytics and rate limiting settings are represented in the compose path without changing application code.
- The default SQLite local path remains available outside Docker.
- README documents how to start, stop, and smoke-check the local operational stack.
- Verification includes structural checks for Docker/Compose artifacts and a live compose smoke where the local environment allows.
- If Docker is unavailable, the task records the exact blocker and still verifies the repo-side build/test/pack set.
- Phase verification passes with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

## Foundation for Next Step

This step leaves Phase 003 with a reproducible local stack for PostgreSQL, Redis, analytics, cache, and rate limiting. The next task can add GitHub Actions CI validation on top of a documented operational shape instead of inventing CI targets from scratch.

## Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

When Docker is available, also run the smallest practical compose validation and smoke path, such as:

```powershell
docker compose config
docker compose up --build
```

Then smoke-check:

- `GET /api/health`
- `POST /api/short-links`
- `GET /{code}`
- `GET /api/short-links/{code}`
- `DELETE /api/short-links/{code}`

## Done Notes

- Completed on 2026-07-12.
- Added `.dockerignore`, `src/ShortenLink.Api/Dockerfile`, root `compose.yml`, and `scripts/smoke-docker-compose.ps1` so the demo API can run in a local Docker Compose stack with PostgreSQL and Redis while enabling analytics, cache, and rate limiting through configuration only.
- Updated `README.md` with compose startup, shutdown, ports, configuration expectations, smoke commands, and an explicit reminder that the default non-Docker SQLite path remains available.
- Verified repo-side acceptance with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
- Verified Docker artifact structure with `docker compose -f compose.yml config`.
- Live compose smoke was blocked by the local environment because Docker daemon access to `//./pipe/docker_engine` was unavailable from the current shell, and Docker also reported access denied for `C:\Users\LENOVO\.docker\config.json`. The smoke script now fails fast with that exact blocker instead of masking it.
