---
id: 001_001
phase: 001
task: 001
title: Solution and NuGet package boundary scaffold
status: done
created_at: 2026-07-09
completed_at: 2026-07-09
owner: codex
type: feature
priority: high
depends_on: []
tags:
  - solution-structure
  - nuget
  - library-boundary
  - product-vision
---

# 001_001 - Solution And NuGet Package Boundary Scaffold

## Step Goal

Create the initial solution structure for the Shorten Link product with the reusable library separated from the demo API/Web.

The outcome should make the package boundary explicit: consumer projects can reference the reusable projects, and the reusable library surface can later be packed as NuGet without taking a dependency on the demo application.

## Foundation for Next Step

This step establishes project names, references, package metadata, and build commands that every later core, infrastructure, API, frontend, and test task will build on.

## Scope

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

## Acceptance Criteria

- The solution builds with the created projects.
- Reusable projects do not reference `ShortenLink.Api` or `ShortenLink.Web`.
- Demo API references the reusable library surface.
- Package metadata exists for the reusable library surface.
- `dotnet pack` can run for the reusable package project or the README clearly identifies the package project and command.
- README documents how another .NET project is expected to reference/use the library at a high level.
- `PHASE_SUMMARY.md` is updated only after this task is actually completed.

## Affected Files

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

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Implementation Notes

Prefer .NET 8 LTS if .NET 10 tooling is not available locally. Keep any framework choice explicit in README.

If the package surface is split across multiple reusable projects, document whether consumers should install/reference `ShortenLink.AspNetCore`, `ShortenLink.Core`, or both.

Flag any package-boundary compromise with `PACKAGE RISK: <reason>`.

## Verification

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

## Done Notes

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
