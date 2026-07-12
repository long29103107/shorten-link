---
id: 004_001
phase: 004
task: 001
title: Consumer package installation smoke MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: release-hardening
priority: high
depends_on:
  - 003
tags:
  - consumer
  - package
  - smoke
  - release
  - phase-4
---

# 004_001 - Consumer Package Installation Smoke MVP

## Step Goal

Add a repeatable consumer smoke path that proves a clean ASP.NET Core app can consume the packaged Shorten Link integration, configure it through normal appsettings/environment values, and exercise the create, detail, redirect, and deactivate workflow without referencing demo API internals.

This step should validate the product from the perspective of a .NET developer installing the reusable package, not from the perspective of the repository's own demo API.

## Dependency

- Phase 001 delivered the reusable library boundary, package metadata, SQLite default behavior, API endpoints, and demo flow.
- Phase 002 delivered configuration-driven PostgreSQL provider selection.
- Phase 003 delivered analytics, cache, rate limiting, Docker Compose, and CI validation.

## Scope

In:

- Add a consumer smoke project, script, or documented fixture that starts from a clean app shape.
- Consume `ShortenLink.AspNetCore` as the normal host-facing package entry point.
- Use local packed package output or a local package source generated from the current repo.
- Configure SQLite default mode without requiring PostgreSQL, Redis, Docker, or external credentials.
- Smoke-check `POST /api/short-links`, `GET /api/short-links/{code}`, `GET /{code}`, and `DELETE /api/short-links/{code}`.
- Ensure the consumer app does not reference `ShortenLink.Api` or duplicate short-link business logic.
- Document the consumer smoke command in `README.md` if the command surface changes.

Out:

- Do not publish to NuGet.
- Do not add public cloud deployment or registry publishing.
- Do not require Docker, PostgreSQL, Redis, or frontend assets for this smoke.
- Do not change public contracts unless the consumer smoke exposes a concrete package-boundary defect.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/004/PHASE_SUMMARY.md`
- `.okf/phase/004/004_001-consumer-package-installation-smoke-mvp.md`
- `README.md`
- `scripts/`
- `samples/` or `.tmp/` depending on the chosen smoke shape
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`

## Acceptance Criteria

- A repeatable consumer smoke exists and can be run from the repository root.
- The smoke uses `ShortenLink.AspNetCore` as the consumer-facing integration entry point.
- The smoke verifies package/reference isolation by avoiding `ShortenLink.Api` internals.
- The smoke runs with SQLite default configuration and no external infrastructure.
- The smoke verifies create, detail, redirect, and deactivate behavior.
- Documentation tells maintainers how to run the consumer smoke.
- Existing build, test, and pack verification still pass:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

## Foundation for Next Step

This step gives Phase 004 a real consumer confidence check. The next step can harden release docs, package metadata, and CI around a proven consumer workflow instead of relying only on internal demo-host verification.

## Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

Run the new consumer smoke command added by this task and record its output or blocker precisely.

## Done Notes

- Completed on 2026-07-12.
- Added `scripts/smoke-consumer-package.ps1`, a repeatable consumer smoke that packs the reusable packages into `.tmp/consumer-packages`, creates a clean ASP.NET Core consumer app under `.tmp/consumer-smoke`, installs `ShortenLink.AspNetCore`, maps the package endpoints, and runs SQLite default configuration without PostgreSQL, Redis, Docker, frontend assets, or publishing.
- Added `.tmp/` to `.gitignore` so generated consumer smoke artifacts stay out of source control.
- Updated `README.md` with the consumer package smoke command and its scope.
- The consumer project is checked to use a `PackageReference` for `ShortenLink.AspNetCore` and to avoid `ProjectReference` or `ShortenLink.Api` internals.
- Verified consumer behavior with `.\scripts\smoke-consumer-package.ps1`; the smoke returned create `201`, detail `200`, redirect `302`, delete `200`, and post-delete redirect `410`.
- The smoke required NuGet network access in this environment to restore missing package dependencies from `api.nuget.org`; after approval, it completed successfully.
- Verified repo checks:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
