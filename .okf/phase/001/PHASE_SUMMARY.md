---
phase: 001
title: MVP Library And Demo Flow
status: complete
created_at: 2026-07-09
updated_at: 2026-07-12
current_task: null
task_count: 8
done_count: 8
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
| 001_008 | Phase 001 end-to-end smoke closure | done | 2026-07-12 |

## Current Task

No task is active. Phase 001 is complete.

## Completed Notes

- `001_001` completed on 2026-07-09. Created the .NET solution, reusable library projects, demo API, React/Vite scaffold, test placeholders, package metadata, placeholder host integration extensions, README setup notes, and verified build/pack/reference direction.
- `001_002` completed on 2026-07-09. Expanded README with maintainer build/pack commands, local NuGet feed usage, project-reference usage, consumer `Program.cs` setup, minimum SQLite config, package output paths, and intended direct `IShortLinkService` usage.
- `001_003` completed on 2026-07-09. Added Swashbuckle Swagger/OpenAPI to the demo API only, documented `/swagger`, verified build, Swagger UI, Swagger JSON, and `/api/health`.
- `001_004` completed on 2026-07-11. Added reusable core domain/contracts, Base62 code generation, alias and URL validation, repository/service interfaces, validation-focused service behavior, and 28 passing core tests.
- `001_005` completed on 2026-07-11. Added EF Core SQLite persistence, `ShortLinkDbContext`, SQLite-backed `IShortLinkRepository`, required indexes, duplicate-code database enforcement, and 5 passing SQLite integration tests.
- `001_006` completed on 2026-07-12. Added reusable ASP.NET Core options/DI wiring, SQLite database initialization, create/detail/deactivate/redirect endpoint mapping, stable error payloads, and 8 passing API integration tests against a SQLite-backed host.
- `001_007` completed on 2026-07-12. Replaced the frontend scaffold with a real React demo flow for create/result/detail/deactivate/fallback behavior, added feature-scoped API modules, updated Vite proxy/dev config, and verified the production frontend build.
- `001_008` completed on 2026-07-12. Verified the end-to-end local API + React flow with real browser smoke, updated local run guidance to use the API `https` launch profile, fixed development fallback routing so unknown short codes land on the React fallback page, and confirmed Phase 001 closure with build, test, and pack evidence.

## Next Task Proposal

Create `002_001 - PostgreSQL provider toggle MVP` next.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 001_001 - Solution And NuGet Package Boundary Scaffold

Source before compaction: `001_001-solution-and-nuget-package-boundary-scaffold.md`

#### Step Goal

Create the initial solution structure for the Shorten Link product with the reusable library separated from the demo API/Web.

The outcome should make the package boundary explicit: consumer projects can reference the reusable projects, and the reusable library surface can later be packed as NuGet without taking a dependency on the demo application.

#### Foundation for Next Step

This step establishes project names, references, package metadata, and build commands that every later core, infrastructure, API, frontend, and test task will build on.

#### Scope

In:

- Inspect `REQUEST.md` and `PRODUCT_VISION.md` before creating projects.
- Create the solution file.
- Create reusable library projects:
  - `ShortenLink.Core`
  - `ShortenLink.Infrastructure`
  - `ShortenLink.AspNetCore`
- Create demo host projects:
  - `ShortenLink.Api`
  - `ShortenLink.Web`
- Create test project placeholders:
  - `ShortenLink.Core.Tests`
  - `ShortenLink.Infrastructure.Tests`
  - `ShortenLink.Api.Tests`
- Wire project references so demo projects depend on reusable projects, not the reverse.
- Add basic NuGet package metadata to the reusable library project or projects that form the public package surface.
- Add README notes for build, test, pack, and consume-from-another-project commands.

Out:

- Do not implement short-link domain behavior yet.
- Do not implement EF Core persistence yet.
- Do not implement API endpoints yet.
- Do not implement React screens yet.
- Do not add PostgreSQL, Redis, analytics worker, Docker Compose, or CI.

#### Acceptance Criteria

- The solution builds with the created projects.
- Reusable projects do not reference `ShortenLink.Api` or `ShortenLink.Web`.
- Demo API references the reusable library surface.
- Package metadata exists for the reusable library surface.
- `dotnet pack` can run for the reusable package project or the README clearly identifies the package project and command.
- README documents how another .NET project is expected to reference/use the library at a high level.
- `PHASE_SUMMARY.md` is updated only after this task is actually completed.

#### Affected Files

Expected starting points:

- `ShortenLink.slnx` or `ShortenLink.sln`
- `src/ShortenLink.Core/`
- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.AspNetCore/`
- `src/ShortenLink.Api/`
- `src/ShortenLink.Web/`
- `tests/ShortenLink.Core.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/`
- `tests/ShortenLink.Api.Tests/`
- `README.md`

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Implementation Notes

Prefer .NET 8 LTS if .NET 10 tooling is not available locally. Keep any framework choice explicit in README.

If the package surface is split across multiple reusable projects, document whether consumers should install/reference `ShortenLink.AspNetCore`, `ShortenLink.Core`, or both.

Flag any package-boundary compromise with `PACKAGE RISK: <reason>`.

#### Verification

Run the smallest relevant checks:

```powershell
dotnet build
```

```powershell
dotnet pack
```

If the frontend scaffold is created, run the smallest available frontend check after dependencies are installed:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

#### Done Notes

Completed on 2026-07-09.

Implemented:

- Created `ShortenLink.slnx`.
- Created reusable library projects:
  - `src/ShortenLink.Core`
  - `src/ShortenLink.Infrastructure`
  - `src/ShortenLink.AspNetCore`
- Created demo host projects:
  - `src/ShortenLink.Api`
  - `src/ShortenLink.Web`
- Created test placeholder projects:
  - `tests/ShortenLink.Core.Tests`
  - `tests/ShortenLink.Infrastructure.Tests`
  - `tests/ShortenLink.Api.Tests`
- Wired references so reusable projects do not depend on demo API/Web:
  - `ShortenLink.Infrastructure` -> `ShortenLink.Core`
  - `ShortenLink.AspNetCore` -> `ShortenLink.Core`, `ShortenLink.Infrastructure`
  - `ShortenLink.Api` -> `ShortenLink.AspNetCore`
- Added basic package metadata for the reusable package surface.
- Added placeholder `AddShortenLink(...)` and `MapShortenLinkEndpoints()` extension methods so consumer code can compile against the host-facing package.
- Added demo API health endpoint and default `ShortenLink` configuration shape.
- Added React/Vite frontend scaffold files without installing dependencies.
- Added README build, test, pack, and high-level consumer usage notes.

Verification:

- `dotnet build ShortenLink.slnx` passed with 0 warnings and 0 errors.
- `dotnet pack ShortenLink.slnx -c Release` created:
  - `src/ShortenLink.Core/bin/Release/ShortenLink.Core.1.0.0.nupkg`
  - `src/ShortenLink.Infrastructure/bin/Release/ShortenLink.Infrastructure.1.0.0.nupkg`
  - `src/ShortenLink.AspNetCore/bin/Release/ShortenLink.AspNetCore.1.0.0.nupkg`
- `dotnet pack` reported the expected `IsPackable=false` warning for `ShortenLink.Api`.
- `dotnet test ShortenLink.slnx --no-build` exited successfully. Test projects are placeholders and contain no test cases yet.
- Project reference checks confirmed `ShortenLink.Core` has no project references and demo API depends inward through `ShortenLink.AspNetCore`.
- Frontend build was skipped because `node_modules` is not installed; dependencies are declared in `src/ShortenLink.Web/package.json`.

### 001_002 - NuGet Package Build And Consumer Usage Guide

Source before compaction: `001_002-nuget-package-build-and-consumer-usage-guide.md`

#### Step Goal

Document and verify how the Shorten Link library is built, packed, and consumed by another .NET project after the initial solution/package scaffold exists.

The outcome should answer two practical questions clearly:

- How does a maintainer build and pack the reusable library?
- How does another .NET application install/reference the package and call it?

#### Dependency

- `001_001` must create the solution, reusable library projects, demo projects, project references, and basic package metadata.

#### Foundation for Next Step

This step establishes the consumer-facing package contract and usage documentation so later implementation tasks can keep public APIs stable and avoid demo-only shortcuts.

#### Scope

In:

- Identify the package project or projects that a consumer should install/reference.
- Document local build command.
- Document local `dotnet pack` command and expected output location.
- Document local package feed usage, for example adding a folder source or direct project reference during development.
- Document consumer setup in another ASP.NET Core app:
  - package reference or project reference
  - `builder.Services.AddShortenLink(builder.Configuration)`
  - `app.MapShortenLinkEndpoints()`
  - minimum `appsettings.json` sample
- Document direct service usage through `IShortLinkService`.
- Include a small end-to-end consumer code sample in README.
- Verify the package command after the scaffold exists.

Out:

- Do not implement full short-link business logic in this task unless it is already required by the scaffold.
- Do not publish to nuget.org.
- Do not add PostgreSQL, Redis, analytics worker, Docker Compose, or CI.
- Do not replace later API/service implementation tasks.

#### Acceptance Criteria

- README has a clear "Build and Pack" section for maintainers.
- README has a clear "Use From Another .NET App" section for consumers.
- The docs name the intended package surface, such as `ShortenLink.AspNetCore`, `ShortenLink.Core`, or both.
- The consumer sample shows service registration with `AddShortenLink(...)`.
- The consumer sample shows endpoint mapping with `MapShortenLinkEndpoints()`.
- The consumer sample shows direct `IShortLinkService` usage.
- The docs include minimum `ShortenLink` configuration needed for SQLite default usage.
- `dotnet pack` is run or the exact blocker is documented in Done Notes.
- `PHASE_SUMMARY.md` is updated only after this task is actually completed.

#### Affected Files

Expected starting points:

- `README.md`
- `src/ShortenLink.Core/`
- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.AspNetCore/`
- package project `.csproj` files
- `PRODUCT_VISION.md` only if public package direction changes

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Implementation Notes

Prefer documenting the host-facing package as the easiest consumer entry point if `ShortenLink.AspNetCore` owns `AddShortenLink(...)` and `MapShortenLinkEndpoints()`.

If the library is split into multiple packages, document the expected install order and which package a normal ASP.NET Core consumer should start with.

Keep examples copy-pasteable and aligned with the real namespaces created in `001_001`.

#### Verification

Run the package verification once the project exists:

```powershell
dotnet pack
```

If the package output path is customized, verify that README names the same path.

Optionally create a tiny local consumer smoke project only if needed to prove package usage; remove or isolate it from the main product code before completion.

#### Done Notes

Completed on 2026-07-09.

Implemented:

- Expanded README with a dedicated `Build And Pack` section.
- Documented `ShortenLink.AspNetCore` as the normal ASP.NET Core consumer entry point.
- Documented package output paths for:
  - `ShortenLink.AspNetCore`
  - `ShortenLink.Core`
  - `ShortenLink.Infrastructure`
- Documented local development usage with direct project reference.
- Documented local NuGet folder usage with `dotnet nuget add source` and `dotnet add package`.
- Documented consumer `Program.cs` setup with:
  - `builder.Services.AddShortenLink(builder.Configuration)`
  - `app.MapShortenLinkEndpoints()`
- Documented minimum SQLite-default `ShortenLink` configuration.
- Documented intended direct `IShortLinkService` usage without moving core service implementation into the demo API.

Verification:

- `dotnet pack ShortenLink.slnx -c Release` passed and created:
  - `src/ShortenLink.Core/bin/Release/ShortenLink.Core.1.0.0.nupkg`
  - `src/ShortenLink.Infrastructure/bin/Release/ShortenLink.Infrastructure.1.0.0.nupkg`
  - `src/ShortenLink.AspNetCore/bin/Release/ShortenLink.AspNetCore.1.0.0.nupkg`
- `dotnet pack` reported the expected `IsPackable=false` warning for `ShortenLink.Api`, which is the demo host and should not be packaged as the reusable library.

### 001_003 - Demo API Swagger/OpenAPI

Source before compaction: `001_003-demo-api-swagger-openapi.md`

#### Step Goal

Add Swagger/OpenAPI to the demo API so developers can inspect and try the available HTTP surface while Phase 001 endpoints are built out.

This is a demo-host feature. It must not move Swagger dependencies into the reusable library packages.

#### Dependency

- `001_002` documented the library package/consumer flow and left Phase 001 ready for the next demo/API developer-experience slice.

#### Foundation for Next Step

This step gives later API endpoint tasks an immediate documentation surface. As create/detail/delete/redirect endpoints are implemented, they should show up in Swagger without adding a separate documentation pass.

#### Scope

In:

- Add Swagger/OpenAPI package support to `ShortenLink.Api`.
- Register Swagger services in the demo API host.
- Enable Swagger UI in development.
- Add a stable endpoint name for the current health endpoint so it appears cleanly in generated docs.
- Document how to open Swagger locally.

Out:

- Do not add Swagger dependencies to `ShortenLink.Core`, `ShortenLink.Infrastructure`, or `ShortenLink.AspNetCore`.
- Do not implement short-link endpoints in this task.
- Do not publish OpenAPI artifacts.
- Do not add Scalar, ReDoc, Docker, or CI.

#### Acceptance Criteria

- `ShortenLink.Api` builds with Swagger enabled.
- `ShortenLink.Api` has a package reference for Swagger/OpenAPI support.
- `Program.cs` registers Swagger services before `builder.Build()`.
- `Program.cs` maps Swagger UI in development.
- README documents the local Swagger URL.
- Reusable library projects remain free of Swagger package references.

#### Affected Files

- `src/ShortenLink.Api/ShortenLink.Api.csproj`
- `src/ShortenLink.Api/Program.cs`
- `README.md`

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`

#### Implementation Notes

Keep Swagger in the demo API host. The library package should stay focused on reusable short-link behavior and ASP.NET Core integration extensions.

#### Verification

```powershell
dotnet build ShortenLink.slnx
```

#### Done Notes

Completed on 2026-07-09.

Implemented:

- Added `Swashbuckle.AspNetCore` to `ShortenLink.Api`.
- Registered `AddEndpointsApiExplorer()` and `AddSwaggerGen()`.
- Enabled `UseSwagger()` and `UseSwaggerUI()` in development.
- Added a stable endpoint name to `/api/health`.
- Documented local Swagger URL in README.

Verification:

- `dotnet build ShortenLink.slnx --no-restore` passed with 0 warnings and 0 errors.
- Local smoke test passed:
  - `GET http://127.0.0.1:5188/swagger/index.html` returned 200.
  - `GET http://127.0.0.1:5188/swagger/v1/swagger.json` returned 200.
  - `GET http://127.0.0.1:5188/api/health` returned `{"status":"ok","app":"ShortenLink.Api"}`.

### 001_004 - Core Domain Contracts And Validation MVP

Source before compaction: `001_004-core-domain-contracts-and-validation-mvp.md`

#### Step Goal

Implement the first real `ShortenLink.Core` domain surface so the product has reusable models, request/result contracts, service/repository interfaces, Base62 short-code generation, custom alias validation, URL validation, and focused core tests.

This task should make the core library useful on its own while leaving persistence, ASP.NET Core endpoint mapping, and demo UI integration for later Phase 001 tasks.

#### Dependency

- `001_001` created the solution, reusable project boundary, package metadata, and placeholder consumer-facing extension points.
- `001_002` documented how consumers should reference and call the package surface.
- `001_003` added a Swagger/OpenAPI surface to the demo API so later endpoints can be inspected as they are implemented.

#### Foundation for Next Step

This step establishes the stable domain and service contracts that the SQLite repository, ASP.NET Core DI/endpoint mapping, demo API, and React demo flow can reuse without redefining short-link behavior in host projects.

#### Scope

In:

- Add the core `ShortLink` domain model.
- Add create/detail/resolve request and result DTOs needed by the first service contract.
- Add `IShortLinkService`, `IShortCodeGenerator`, and `IShortLinkRepository` contracts.
- Implement a default Base62 short-code generator with default length 7.
- Implement custom alias validation for letters, numbers, `_`, and `-`.
- Implement URL validation that rejects empty, malformed, and non-HTTP/HTTPS URLs.
- Add focused `ShortenLink.Core.Tests` coverage for generator behavior, alias validation, URL validation, and core service edge cases that can be tested without persistence.

Out:

- Do not implement EF Core or SQLite persistence in this task.
- Do not implement ASP.NET Core endpoint mapping beyond any compile-only contract adjustments required by the new interfaces.
- Do not implement demo API create/detail/delete/redirect endpoints yet.
- Do not implement React UI flows yet.
- Do not add PostgreSQL, Redis, analytics worker, rate limiting, Docker Compose, or CI.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `src/ShortenLink.Core/`
- `tests/ShortenLink.Core.Tests/`
- `src/ShortenLink.AspNetCore/` only if placeholders must compile against the new contracts
- `README.md` only if public contract names differ from existing consumer examples

#### Acceptance Criteria

- `ShortenLink.Core` exposes a reusable `ShortLink` model with code, original URL, active state, created timestamp, and optional expiry information.
- Core request/result DTOs are available for creating, resolving, and inspecting short links without depending on demo API types.
- `IShortLinkService`, `IShortCodeGenerator`, and `IShortLinkRepository` are defined in the core package.
- The default short-code generator uses Base62 characters and defaults to length 7.
- Custom aliases allow only letters, numbers, `_`, and `-`.
- Empty, malformed, and non-HTTP/HTTPS URLs are rejected by reusable core validation.
- Core tests cover generator shape, URL validation, alias validation, and service-level validation behavior that does not need a database.
- The solution builds after the new contracts are added.
- Package boundaries remain intact: `ShortenLink.Core` does not reference demo API/Web or infrastructure.

#### Implementation Notes

Keep validation reusable and host-agnostic. Prefer small core types and interfaces that later persistence/API tasks can compose rather than controller-specific models.

If a full service implementation needs persistence to be meaningful, define the service contract now and keep implementation limited to validation-only behavior or a small in-memory test seam only when it directly supports the acceptance criteria.

#### Verification

Run the smallest relevant checks:

```powershell
dotnet test ShortenLink.slnx --filter ShortenLink.Core.Tests
```

If test filtering is not supported by the current project shape, run:

```powershell
dotnet test ShortenLink.slnx
```

Also verify the package boundary with:

```powershell
dotnet build ShortenLink.slnx
```

#### Done Notes

Completed on 2026-07-11.

Implemented:

- Added the reusable `ShortLink` domain model with code, original URL, active state, created timestamp, optional expiry, expiry checks, and deactivate behavior.
- Added core create/detail/resolve/deactivate request/result contracts and `ShortLinkErrorCodes`.
- Added `IShortLinkService`, `IShortCodeGenerator`, and `IShortLinkRepository`.
- Implemented `Base62ShortCodeGenerator` with default length 7 and Base62 alphabet.
- Implemented reusable custom alias validation for letters, numbers, `_`, and `-`.
- Implemented reusable URL validation for absolute HTTP/HTTPS URLs.
- Implemented `ShortLinkService` validation, duplicate custom alias handling, generated-code retry, resolve detail, and deactivate behavior against the repository contract.
- Added xUnit-based core tests for generator shape, alias validation, URL validation, duplicate alias handling, generated-code retry, expired link behavior, and deactivate behavior.
- Added `ShortenLink.Core.Tests` back into `ShortenLink.slnx` so root-level verification runs the core test suite.

Verification:

- `dotnet test tests\ShortenLink.Core.Tests\ShortenLink.Core.Tests.csproj --verbosity minimal` passed with 28 tests.
- `dotnet build ShortenLink.slnx --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --no-restore --verbosity minimal` passed with 28 tests.
- `dotnet pack ShortenLink.slnx -c Release --no-restore` passed. It reported the expected `IsPackable=false` warning for `ShortenLink.Api`, which is the demo host and should not produce a reusable package.

### 001_005 - SQLite Repository Persistence MVP

Source before compaction: `001_005-sqlite-repository-persistence-mvp.md`

#### Step Goal

Implement the first real `ShortenLink.Infrastructure` persistence adapter for the verified core contracts: EF Core SQLite storage, required indexes, repository behavior, and SQLite integration tests.

This task should prove that the core `IShortLinkRepository` contract can persist and retrieve `ShortLink` records through SQLite without moving business rules into the demo API or Web projects.

#### Dependency

- `001_004` completed the reusable core `ShortLink` model, `IShortLinkRepository`, `IShortLinkService`, validation helpers, Base62 generator, and focused core tests.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj`
- `tests/ShortenLink.Infrastructure.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/ShortenLink.Infrastructure.Tests.csproj`
- `ShortenLink.slnx` only if test project membership needs correction

#### Acceptance Criteria

- `ShortenLink.Infrastructure` references EF Core SQLite without adding persistence dependencies to `ShortenLink.Core`.
- A DbContext or equivalent EF Core persistence boundary exists for short links.
- `IShortLinkRepository` has a SQLite-backed implementation.
- Short-link code uniqueness is enforced at the database/index level.
- Integration tests prove add, find, exists, and update/deactivate behavior against SQLite.
- Integration tests prove duplicate short codes fail through the persistence layer.
- Stored records preserve original URL, active state, created timestamp, and optional expiry.
- `ShortenLink.Core` remains free of EF Core and SQLite package references.
- The solution builds and relevant infrastructure tests pass.

#### Foundation for Next Step

This step gives the ASP.NET Core integration task a concrete persistence adapter to register through DI. The next task can wire `AddShortenLink(...)` to SQLite-backed services and begin mapping real API endpoints without inventing a repository implementation inside the demo API.

#### Implementation Notes

Prefer keeping EF Core-specific mapping and configuration inside `ShortenLink.Infrastructure`. If the core `ShortLink` type is awkward for EF Core construction, use an internal persistence entity and map to/from the core model rather than weakening the core domain shape.

Use real SQLite for integration tests, either a temporary file database or an in-memory SQLite connection kept open for the test lifetime. Avoid EF Core's non-relational in-memory provider because this task needs SQLite/index behavior.

#### Verification

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

#### Done Notes

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

### 001_006 - ASP.NET Core DI And Short-Link Endpoints MVP

Source before compaction: `001_006-aspnet-core-di-and-short-link-endpoints-mvp.md`

#### Step Goal

Wire the verified core service and SQLite repository into the reusable `ShortenLink.AspNetCore` package and expose the first real short-link HTTP contracts through `MapShortenLinkEndpoints()`.

This task should prove that a host application can call `builder.Services.AddShortenLink(builder.Configuration)` and `app.MapShortenLinkEndpoints()` to get create, detail, delete/deactivate, redirect, and unknown-code fallback behavior without reimplementing short-link business logic in `ShortenLink.Api`.

#### Dependency

- `001_004` completed the reusable core model, service contract, validation, code generator, and service behavior.
- `001_005` completed the EF Core SQLite persistence adapter, repository implementation, required indexes, and SQLite integration tests.
- `001_003` added Swagger/OpenAPI to the demo API so the new mapped endpoints can be inspected from the demo host.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `src/ShortenLink.AspNetCore/`
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`
- `src/ShortenLink.Api/Program.cs`
- `src/ShortenLink.Api/appsettings.json`
- `src/ShortenLink.Api/appsettings.Development.json`
- `tests/ShortenLink.Api.Tests/`
- `tests/ShortenLink.Api.Tests/ShortenLink.Api.Tests.csproj`
- `README.md` only if public endpoint or configuration examples need to stay aligned

#### Acceptance Criteria

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

#### Foundation for Next Step

This step gives Phase 001 a reusable HTTP integration surface that the React demo can consume directly. The next task can focus on frontend create/detail/fallback UX without inventing parallel API routes or duplicating short-link rules in the web app.

#### Implementation Notes

Prefer Minimal API route groups and small endpoint DTOs inside `ShortenLink.AspNetCore`. Keep host-specific configuration in the demo app limited to appsettings values and normal ASP.NET Core service registration.

Use a temporary SQLite database or in-memory SQLite connection for API tests so the tests exercise the real repository and endpoint stack. Avoid EF Core's non-relational in-memory provider for endpoint tests that depend on uniqueness or persistence behavior.

#### Verification

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

#### Done Notes

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

### 001_007 - React Create Detail Fallback Demo Flow

Source before compaction: `001_007-react-create-detail-fallback-demo-flow.md`

#### Step Goal

Build the Phase 1 React demo experience on top of the verified reusable API contracts: a user can create a short link, copy the generated short URL, inspect link details, deactivate a link, and land on a friendly fallback state when a short code or frontend route is not found.

This task should prove the product is usable from the browser without moving short-link business rules into `ShortenLink.Web`.

#### Dependency

- `001_006` completed reusable ASP.NET Core DI and endpoint mapping for create, detail, deactivate, redirect, stable JSON errors, and frontend fallback behavior.
- `001_005` completed SQLite-backed persistence for the API host.
- `001_004` completed reusable core validation and service behavior that the frontend must consume through HTTP rather than duplicate.

#### Scope

In:

- Replace the placeholder React scaffold with the actual demo app as the first screen.
- Add frontend API request/response types that match the `001_006` endpoint contracts.
- Add a feature-scoped API client for:
  - `POST /api/short-links`
  - `GET /api/short-links/{code}`
  - `DELETE /api/short-links/{code}`
- Build the home flow with long URL input, optional custom alias, optional expiry, create button, validation/error display, created result, and copy-short-url action.
- Build a detail flow for inspecting original URL, short code, created timestamp, optional expiry, and active/deactivated state.
- Build deactivate/delete behavior that updates UI state after `DELETE /api/short-links/{code}`.
- Build a friendly not-found/fallback page for `/not-found` and unknown frontend routes.
- Add simple client-side route handling that supports home, detail, and fallback views without introducing a heavy router unless needed.
- Configure frontend API base behavior for local development, such as a Vite proxy or a small API base helper.
- Keep backend business rules in the API/library; frontend validation should only improve UX and must not be treated as authoritative.
- Update README only if frontend run/build/API proxy instructions need to change.

Out:

- Do not add authentication, user accounts, dashboards, analytics charts, Redis/cache, rate limiting, Docker Compose, PostgreSQL, or worker infrastructure.
- Do not create backend endpoints that duplicate existing `MapShortenLinkEndpoints()` behavior.
- Do not add a marketing landing page; the first screen should be the usable short-link demo.
- Do not add a large frontend framework or state-management library unless the implementation clearly needs it.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `src/ShortenLink.Web/src/main.tsx`
- `src/ShortenLink.Web/src/styles.css`
- `src/ShortenLink.Web/src/features/short-links/`
- `src/ShortenLink.Web/src/shared/`
- `src/ShortenLink.Web/vite.config.ts`
- `src/ShortenLink.Web/package.json` only if frontend dependencies or scripts need to change
- `README.md` only if frontend run/build or API proxy instructions change

#### Acceptance Criteria

- The first rendered screen is the usable create-short-link experience, not placeholder or marketing copy.
- A user can submit a valid long URL with optional alias and optional expiry to create a short link through `POST /api/short-links`.
- The UI shows the returned short URL, original URL, code, and created timestamp after a successful create.
- The UI exposes a copy action for the short URL and clearly reflects copied/error states.
- Duplicate alias, invalid URL, invalid alias, and generic API errors are displayed with user-friendly messages based on the backend error payload.
- A user can inspect link details by code using `GET /api/short-links/{code}`.
- Detail view shows original URL, short code, created timestamp, optional expiry, and active/deactivated state.
- A user can deactivate an existing link through `DELETE /api/short-links/{code}` and the detail/result UI updates accordingly.
- `/not-found` and unknown frontend routes render a friendly fallback view with a path back to the create flow.
- Frontend types align with the API DTOs from `001_006`.
- Frontend code is feature-scoped and keeps API calls outside React components where practical.
- `ShortenLink.Web` builds successfully with the repository's frontend build command, or the task explicitly records why dependency installation/build verification was unavailable.

#### Foundation for Next Step

This step completes the browser-facing Phase 1 product flow. The next step can verify and close Phase 001 with README/run guidance and an end-to-end smoke path, or move to Phase 002 if all Phase 1 done criteria are satisfied.

#### Implementation Notes

Prefer a compact feature structure under `src/ShortenLink.Web/src/features/short-links/` with `api`, `components`, `pages`, and `types.ts` if the app grows beyond a single file. Keep the UI operational and task-focused: dense enough to create, inspect, copy, and deactivate links quickly.

Use the existing React + Vite stack. If local API calls need a proxy, prefer configuring Vite rather than adding CORS or host-specific frontend behavior in the backend.

#### Verification

Run the frontend build:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

If frontend dependencies are missing, install or restore them only when the environment allows it; otherwise report the skipped build honestly.

If API or backend configuration changes are required, also run:

```powershell
dotnet build ShortenLink.slnx
dotnet test ShortenLink.slnx
```

When dev servers are running, smoke-check:

- Create a short link from the React form.
- Copy the returned short URL.
- Open link details by code.
- Deactivate the link.
- Visit `/not-found` or an unknown frontend route.

#### Done Notes

Completed on 2026-07-12.

Implemented:

- Replaced the placeholder React scaffold with a real Phase 1 demo shell for create, result, detail lookup, deactivate, and fallback flows.
- Added feature-scoped frontend modules under `src/ShortenLink.Web/src/features/short-links/` for API access, DTO types, pages, and components.
- Added a lightweight browser-history router for `/`, `/links/{code}`, and `/not-found` without introducing a full routing dependency.
- Added create flow UX with local form validation, backend error mapping, result display, copy-to-clipboard action, and jump-to-details behavior.
- Added detail flow UX backed by `GET /api/short-links/{code}` and deactivate behavior backed by `DELETE /api/short-links/{code}`.
- Added Vite API proxy defaults for local development and updated development config so returned short URLs use the actual API HTTPS port.
- Updated README with current API endpoints, frontend dev/build instructions, proxy behavior, and corrected direct-service usage docs.

Verification:

- `npm install` completed in `src\ShortenLink.Web`.
- `npm install -D @types/react @types/react-dom` completed in `src\ShortenLink.Web`.
- `npm run build` passed in `src\ShortenLink.Web`.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 41 tests total: 28 core, 5 infrastructure, and 8 API.

Notes:

- I attempted to background-start the API and Vite dev servers for an extra live smoke pass, but this shell session was flaky about keeping those background launches reachable. The compiled frontend, backend build, tests, and updated run instructions are all in place.

### 001_008 - Phase 001 End-To-End Smoke Closure

Source before compaction: `001_008-phase-001-end-to-end-smoke-closure.md`

#### Step Goal

Run the completed Phase 001 API and React demo together against the real local stack, verify the create/detail/deactivate/redirect/fallback journey end to end, and close Phase 001 only if every Phase Done Criterion is supported by current build, test, package, documentation, and smoke evidence.

This task should prove the Phase 001 slice is not merely compiled code: it should be usable from the browser and ready to hand off as the foundation for Phase 002 provider-toggle work.

#### Dependency

- `001_007` completed the React create/result/detail/deactivate/fallback demo flow and verified the production frontend build.
- `001_006` completed reusable ASP.NET Core DI, endpoint mapping, create/detail/deactivate/redirect API behavior, and API integration tests.
- `001_005` completed SQLite-backed persistence and integration tests.
- `001_004` completed reusable core service contracts, validation, code generation, and core tests.
- `001_002` documented package build and consumer usage guidance.

#### Scope

In:

- Start or otherwise exercise the demo API with SQLite default configuration.
- Start or otherwise exercise the React/Vite demo against the API through the documented proxy/config path.
- Smoke-check the browser-level or HTTP-backed user journey:
  - create a short link from the React form or equivalent frontend request path
  - inspect link details by code
  - verify the short URL redirects while active
  - deactivate the link
  - verify detail state after deactivation
  - verify unknown code/fallback behavior
- Re-run required verification for Phase 001 closure:
  - frontend build
  - backend build
  - backend tests
  - package build
- Review README instructions for local SQLite run, frontend run/build, endpoints, and package consumption against the current code.
- Update docs or small config gaps found during smoke only when they are necessary to make the verified Phase 001 flow reproducible.
- Update `PHASE_SUMMARY.md` to either close Phase 001 or document the exact remaining blocker.

Out:

- Do not add new product features beyond closure fixes.
- Do not start Phase 002 implementation in this task.
- Do not add PostgreSQL, Redis, analytics, rate limiting, Docker Compose, authentication, accounts, or dashboards.
- Do not refactor completed library/API/frontend architecture unless the smoke path exposes a real closure blocker.
- Do not mask failing smoke behavior by changing acceptance criteria; record the blocker if it cannot be fixed inside this closure slice.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/001/PHASE_SUMMARY.md`
- `.okf/phase/001/001_008-phase-001-end-to-end-smoke-closure.md`
- `README.md`
- `src/ShortenLink.Api/appsettings.Development.json`
- `src/ShortenLink.Web/vite.config.ts`
- `src/ShortenLink.Web/src/features/short-links/`

Only edit source files if smoke verification exposes a real issue that prevents Phase 001 closure.

#### Acceptance Criteria

- The API can run locally with default SQLite configuration using the documented command path.
- The React demo can run locally against the API using the documented frontend command path and proxy/config behavior.
- The create flow produces a persisted short link and displays a usable short URL.
- `GET /api/short-links/{code}` returns detail for the created code.
- `GET /{code}` redirects active links to the original URL.
- `DELETE /api/short-links/{code}` deactivates the link, and a follow-up detail check reflects the inactive state.
- Unknown code behavior follows the configured fallback path and is consistent with README guidance.
- `npm run build` passes in `src/ShortenLink.Web`.
- `dotnet build ShortenLink.slnx` passes.
- `dotnet test ShortenLink.slnx` passes.
- `dotnet pack ShortenLink.slnx -c Release` passes or records a concrete package-boundary blocker.
- README run, endpoint, SQLite, frontend, and package-consumption guidance matches the current implementation.
- `PHASE_SUMMARY.md` is updated in the same pass:
  - if all Phase Done Criteria are satisfied, mark Phase 001 complete and point to Phase 002 / `002_001`;
  - if not, keep Phase 001 active and document the remaining blocker precisely.

#### Foundation for Next Step

This step leaves Phase 001 with closure evidence instead of inferred readiness. If the smoke path passes, Phase 002 can begin PostgreSQL provider-toggle work on top of a verified SQLite/API/Web/package baseline. If it fails, the next step can address a single documented blocker rather than rediscovering the end-to-end gap.

#### Implementation Notes

Prefer the smallest reliable smoke route available in this environment. Browser-level verification is ideal, but an HTTP-backed smoke script is acceptable when it exercises the same API/frontend configuration path and records exactly what could not be browser-verified.

Keep temporary logs and local databases out of the committed task scope. If previous background dev-server attempts left locked `.tmp` logs, inspect and clean only the processes/files that belong to this repo's smoke attempt.

#### Verification

Run:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Run from the repository root:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

When dev servers are running, smoke-check:

- Create a short link from the React form.
- Copy or use the returned short URL.
- Open link details by code.
- Open the short URL and confirm redirect.
- Deactivate the link.
- Re-open details and confirm inactive state.
- Visit an unknown short code or `/not-found` and confirm fallback behavior.

#### Done Notes

Completed on 2026-07-12.

Implemented:

- Verified the full Phase 001 create, copy, detail, redirect, deactivate, and fallback journey against the real local API and Vite frontend stack.
- Updated local run guidance in `README.md` to use the API `https` launch profile so returned short URLs and the frontend proxy target both work together during local runs.
- Updated development redirect fallback config so unknown short codes land on the React `/not-found` page instead of looping on the API host when the frontend runs separately.
- Expanded API integration coverage so frontend fallback configuration now supports both root-relative paths and absolute HTTP/HTTPS URLs.

Verification:

- `npm run build` passed in `src\ShortenLink.Web`.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 42 total tests: 28 core, 5 infrastructure, and 9 API.
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal` passed for the reusable packages. The demo API remained intentionally non-packable and emitted the expected informational warning from NuGet pack targets.
- Browser-level smoke passed through a local Edge Playwright run:
  - create from the React form
  - copy returned short URL
  - open details and confirm active state
  - open the short URL and confirm redirect to the original destination
  - deactivate the link and confirm inactive detail state
  - open an unknown short code and confirm frontend fallback
  - open an unknown frontend route and confirm fallback page


## Scan Rule
Agents must read this file before working on any `001_*` task note. Use this summary to determine progress, current task, and whether a proposed task already exists.
