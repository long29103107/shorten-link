---
id: 001_002
phase: 001
task: 002
title: NuGet package build and consumer usage guide
status: done
created_at: 2026-07-09
completed_at: 2026-07-09
owner: codex
type: feature
priority: high
depends_on:
  - 001_001
tags:
  - nuget
  - package-usage
  - documentation
  - library-boundary
---

# 001_002 - NuGet Package Build And Consumer Usage Guide

## Step Goal

Document and verify how the Shorten Link library is built, packed, and consumed by another .NET project after the initial solution/package scaffold exists.

The outcome should answer two practical questions clearly:

- How does a maintainer build and pack the reusable library?
- How does another .NET application install/reference the package and call it?

## Dependency

- `001_001` must create the solution, reusable library projects, demo projects, project references, and basic package metadata.

## Foundation for Next Step

This step establishes the consumer-facing package contract and usage documentation so later implementation tasks can keep public APIs stable and avoid demo-only shortcuts.

## Scope

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

## Acceptance Criteria

- README has a clear "Build and Pack" section for maintainers.
- README has a clear "Use From Another .NET App" section for consumers.
- The docs name the intended package surface, such as `ShortenLink.AspNetCore`, `ShortenLink.Core`, or both.
- The consumer sample shows service registration with `AddShortenLink(...)`.
- The consumer sample shows endpoint mapping with `MapShortenLinkEndpoints()`.
- The consumer sample shows direct `IShortLinkService` usage.
- The docs include minimum `ShortenLink` configuration needed for SQLite default usage.
- `dotnet pack` is run or the exact blocker is documented in Done Notes.
- `PHASE_SUMMARY.md` is updated only after this task is actually completed.

## Affected Files

Expected starting points:

- `README.md`
- `src/ShortenLink.Core/`
- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.AspNetCore/`
- package project `.csproj` files
- `PRODUCT_VISION.md` only if public package direction changes

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Implementation Notes

Prefer documenting the host-facing package as the easiest consumer entry point if `ShortenLink.AspNetCore` owns `AddShortenLink(...)` and `MapShortenLinkEndpoints()`.

If the library is split into multiple packages, document the expected install order and which package a normal ASP.NET Core consumer should start with.

Keep examples copy-pasteable and aligned with the real namespaces created in `001_001`.

## Verification

Run the package verification once the project exists:

```powershell
dotnet pack
```

If the package output path is customized, verify that README names the same path.

Optionally create a tiny local consumer smoke project only if needed to prove package usage; remove or isolate it from the main product code before completion.

## Done Notes

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
