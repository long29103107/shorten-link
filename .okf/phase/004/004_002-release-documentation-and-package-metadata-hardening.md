---
id: 004_002
phase: 004
task: 002
title: Release documentation and package metadata hardening
status: done
created_at: 2026-07-14
completed_at: 2026-07-14
owner: codex
type: release-hardening
priority: high
depends_on:
  - 004_001
tags:
  - release
  - documentation
  - package
  - consumer
  - phase-4
---

# 004_002 - Release Documentation And Package Metadata Hardening

## Step Goal

Harden the release-facing documentation and reusable package metadata so a .NET consumer can understand which package to install, what defaults are safe, which optional providers are available, and which commands prove the package is ready for local consumption.

This step should make the product feel installable and maintainable after the consumer package smoke proved the packaged ASP.NET Core integration can run outside the repository's demo host.

## Dependency

- `004_001` completed a repeatable clean-consumer smoke for `ShortenLink.AspNetCore`, using local packed output, SQLite default mode, and no `ShortenLink.Api` internals.
- Phase 001 delivered the reusable core, SQLite default persistence, endpoint mapping, demo API, and React flow.
- Phase 002 delivered configuration-driven PostgreSQL selection.
- Phase 003 delivered analytics, cache, rate limiting, Docker Compose, and CI validation.

## Scope

In:

- Review reusable package metadata for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Ensure package descriptions, tags, repository metadata, readme/license settings where practical, and pack-facing properties are consistent with the consumer-facing package story.
- Improve README release and consumer guidance around package selection, local package installation, SQLite defaults, PostgreSQL/Redis/analytics/rate-limit options, and verification commands.
- Document the consumer smoke as a release-hardening check, including what it proves and what external dependencies it intentionally avoids.
- Keep demo-host documentation clearly secondary to the reusable package consumption path.
- Preserve the existing package boundary and command surface unless a metadata or documentation defect requires a small fix.

Out:

- Do not publish packages to NuGet.
- Do not add package signing, SBOM generation, release tagging, or deployment automation.
- Do not introduce new public APIs unless documentation exposes a concrete mismatch.
- Do not require Docker, PostgreSQL, Redis, frontend assets, or external credentials for release-readiness verification.
- Do not add broad feature work outside release documentation and package metadata.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/004/PHASE_SUMMARY.md`
- `.okf/phase/004/004_002-release-documentation-and-package-metadata-hardening.md`
- `README.md`
- `src/ShortenLink.Core/ShortenLink.Core.csproj`
- `src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj`
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`
- `scripts/smoke-consumer-package.ps1`

## Acceptance Criteria

- README clearly identifies `ShortenLink.AspNetCore` as the normal ASP.NET Core consumer entry point.
- README explains when a consumer would install or reference `ShortenLink.Core` or `ShortenLink.Infrastructure` directly.
- README documents SQLite default behavior, PostgreSQL toggle, Redis/cache options, analytics options, rate-limit options, and the consumer smoke verification path without making optional infrastructure mandatory.
- README includes a concise release-readiness verification command set for maintainers.
- Reusable package metadata is consistent across packable projects and accurately describes each package's role.
- Package metadata does not imply that the demo API or React app is part of the reusable package surface.
- Existing consumer smoke remains valid after documentation and metadata changes.
- Existing build, test, and pack verification still pass:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

## Foundation for Next Step

This step should leave the reusable package story coherent enough for a release checklist or final public API review. The next Phase 004 step can focus on any remaining release gate, such as CI alignment for the consumer smoke, package artifact inspection, or phase closure, without rewriting the consumer docs from scratch.

## Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

Run the consumer smoke to confirm the release docs still match the package behavior:

```powershell
.\scripts\smoke-consumer-package.ps1
```

If any command needs NuGet network access, record that requirement or blocker precisely.

## Done Notes

- Completed on 2026-07-14.
- Hardened reusable package metadata for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` with role-specific titles, descriptions, tags, project URL, repository URL, and repository type.
- Updated `README.md` so `ShortenLink.AspNetCore` is clearly the normal ASP.NET Core consumer entry point, while `ShortenLink.Core` and `ShortenLink.Infrastructure` are documented for direct contract or persistence-composition scenarios.
- Documented the release-readiness verification command set, the clean consumer package smoke, local package installation from a complete local package source, SQLite default behavior, PostgreSQL toggle, Redis/cache options, analytics options, and rate-limit options.
- Updated the consumer setup snippet to include `UseShortenLinkRateLimiting()` so opt-in rate limiting is wired through the documented host-facing package surface.
- Verified repo checks:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- The first consumer smoke attempt was blocked by sandboxed NuGet network access to `api.nuget.org`; after approval, `.\scripts\smoke-consumer-package.ps1` completed successfully with create `201`, detail `200`, redirect `302`, delete `200`, and post-delete redirect `410`.
