---
phase: 001
title: MVP Library And Demo Flow
status: active
created_at: 2026-07-09
updated_at: 2026-07-12
current_task: null
task_count: 7
done_count: 7
depends_on: []
---

# Phase 001 Summary

## Phase Goal

Deliver the first usable Shorten Link product slice from `PRODUCT_VISION.md`: a reusable .NET library that can be packed as NuGet, SQLite-backed persistence by default, demo API endpoints, and a React UI that can create and use short links.

Phase 001 must prove the product is library-first, not a demo-only URL shortener. The demo API/Web should consume the reusable projects instead of owning short-link business logic.

## Phase Done Criteria

- Solution/project structure exists for reusable library projects, demo API, demo Web, and tests.
- Reusable library projects are isolated from demo API/Web and can be packed with `dotnet pack`.
- Core models, service contracts, code generation, alias validation, and URL validation are implemented.
- SQLite persistence works by default with required indexes.
- `AddShortenLink(...)` and endpoint mapping are available to consumer projects.
- Demo API supports create, redirect, detail, and delete/deactivate endpoints.
- React UI can create a short link, show/copy the result, display detail, and show a friendly fallback page.
- Unknown code behavior follows redirect fallback config.
- Minimum tests cover generator, URL validation, create/resolve service, and SQLite integration.
- README explains local SQLite usage and package consumption.

## Scope

In:

- .NET solution/project scaffold.
- Packable reusable library boundary.
- Core domain, validation, and service contracts.
- EF Core SQLite default persistence.
- ASP.NET Core DI and endpoint integration.
- Demo API and React demo UI.
- Minimum tests and README.

Out:

- PostgreSQL provider toggle.
- Redis cache provider.
- Async analytics worker.
- Rate limiting.
- Docker Compose.
- GitHub Actions CI.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 001_001 | Solution and NuGet package boundary scaffold | done | 2026-07-09 |
| 001_002 | NuGet package build and consumer usage guide | done | 2026-07-09 |
| 001_003 | Demo API Swagger/OpenAPI | done | 2026-07-09 |
| 001_004 | Core domain contracts and validation MVP | done | 2026-07-11 |
| 001_005 | SQLite repository persistence MVP | done | 2026-07-11 |
| 001_006 | ASP.NET Core DI and short-link endpoints MVP | done | 2026-07-12 |
| 001_007 | React create/detail/fallback demo flow | done | 2026-07-12 |

## Current Task

No task is active.

## Completed Notes

- `001_001` completed on 2026-07-09. Created the .NET solution, reusable library projects, demo API, React/Vite scaffold, test placeholders, package metadata, placeholder host integration extensions, README setup notes, and verified build/pack/reference direction.
- `001_002` completed on 2026-07-09. Expanded README with maintainer build/pack commands, local NuGet feed usage, project-reference usage, consumer `Program.cs` setup, minimum SQLite config, package output paths, and intended direct `IShortLinkService` usage.
- `001_003` completed on 2026-07-09. Added Swashbuckle Swagger/OpenAPI to the demo API only, documented `/swagger`, verified build, Swagger UI, Swagger JSON, and `/api/health`.
- `001_004` completed on 2026-07-11. Added reusable core domain/contracts, Base62 code generation, alias and URL validation, repository/service interfaces, validation-focused service behavior, and 28 passing core tests.
- `001_005` completed on 2026-07-11. Added EF Core SQLite persistence, `ShortLinkDbContext`, SQLite-backed `IShortLinkRepository`, required indexes, duplicate-code database enforcement, and 5 passing SQLite integration tests.
- `001_006` completed on 2026-07-12. Added reusable ASP.NET Core options/DI wiring, SQLite database initialization, create/detail/deactivate/redirect endpoint mapping, stable error payloads, and 8 passing API integration tests against a SQLite-backed host.
- `001_007` completed on 2026-07-12. Replaced the frontend scaffold with a real React demo flow for create/result/detail/deactivate/fallback behavior, added feature-scoped API modules, updated Vite proxy/dev config, and verified the production frontend build.

## Next Task Proposal

Create `001_008 - Phase 001 end-to-end smoke closure` next. It should run the API and frontend together, confirm the browser-level create/detail/deactivate/fallback journey against the real local stack, then either close Phase 001 or document the last gap that blocks closure.

## Scan Rule

Agents must read this file before loading any `001_*` task file. Use this summary to determine progress, current task, and whether a proposed task already exists.
