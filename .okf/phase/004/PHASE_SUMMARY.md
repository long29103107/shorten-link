---
phase: 004
title: Release And Consumer Hardening
status: complete
created_at: 2026-07-12
updated_at: 2026-07-14
current_task: null
task_count: 2
done_count: 2
depends_on:
  - 003
---

# Phase 004 Summary

## Phase Goal

Harden the Shorten Link library for real consumer adoption by validating package installation, consumer integration, release documentation, and the final reusable API surface after the MVP, PostgreSQL toggle, and production-readiness phases are complete.

## Phase Done Criteria

- A clean external consumer app can install or reference the local package output and call `AddShortenLink(...)`.
- The consumer smoke verifies create, detail, redirect, and deactivate behavior through the packaged ASP.NET Core integration.
- Release-facing documentation clearly explains package selection, local package installation, configuration defaults, optional providers, and verification commands.
- The reusable package boundary remains free of demo-app-only coupling.
- CI continues to validate build, tests, and pack after release-hardening changes.
- Phase verification passes with build, test, pack, and the consumer smoke path.

## Scope

In:

- Consumer package smoke projects or scripts.
- Package installation and DI integration validation.
- Release-readiness README improvements.
- Public API and package-boundary checks.
- CI or verification updates only when needed to cover release readiness.

Out:

- Publishing to NuGet.
- Production deployment.
- SaaS billing, tenants, authentication, or authorization.
- Advanced analytics dashboards.
- Breaking changes to the public service or endpoint contracts unless a consumer smoke exposes a concrete defect.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 004_001 | Consumer package installation smoke MVP | done | 2026-07-12 |
| 004_002 | Release documentation and package metadata hardening | done | 2026-07-14 |

## Current Task

No task is active. Phase 004 is complete.

## Completed Notes

- `004_001` completed on 2026-07-12. Added a repeatable consumer package smoke that creates a clean ASP.NET Core app, installs `ShortenLink.AspNetCore` from local packed output, avoids demo API internals, runs SQLite default mode, and verifies create, detail, redirect, deactivate, and post-delete redirect behavior. README now documents the smoke command, and repo build, test, and pack verification passed.
- `004_002` completed on 2026-07-14. Hardened release-facing package metadata for the reusable packages, updated README package-selection and release-readiness guidance, documented the complete local package source flow and optional provider defaults, and verified build, test, pack, and the consumer package smoke.

## Next Task Proposal

Phase 004 is complete. Next, decide whether to open a publishing/release automation phase or finalize the current product definition.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 004_001 - Consumer Package Installation Smoke MVP

Source before compaction: `004_001-consumer-package-installation-smoke-mvp.md`

#### Step Goal

Add a repeatable consumer smoke path that proves a clean ASP.NET Core app can consume the packaged Shorten Link integration, configure it through normal appsettings/environment values, and exercise the create, detail, redirect, and deactivate workflow without referencing demo API internals.

This step should validate the product from the perspective of a .NET developer installing the reusable package, not from the perspective of the repository's own demo API.

#### Dependency

- Phase 001 delivered the reusable library boundary, package metadata, SQLite default behavior, API endpoints, and demo flow.
- Phase 002 delivered configuration-driven PostgreSQL provider selection.
- Phase 003 delivered analytics, cache, rate limiting, Docker Compose, and CI validation.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/004/PHASE_SUMMARY.md`
- `.okf/phase/004/004_001-consumer-package-installation-smoke-mvp.md`
- `README.md`
- `scripts/`
- `samples/` or `.tmp/` depending on the chosen smoke shape
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`

#### Acceptance Criteria

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

#### Foundation for Next Step

This step gives Phase 004 a real consumer confidence check. The next step can harden release docs, package metadata, and CI around a proven consumer workflow instead of relying only on internal demo-host verification.

#### Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

Run the new consumer smoke command added by this task and record its output or blocker precisely.

#### Done Notes

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

### 004_002 - Release Documentation And Package Metadata Hardening

Source before compaction: `004_002-release-documentation-and-package-metadata-hardening.md`

#### Step Goal

Harden the release-facing documentation and reusable package metadata so a .NET consumer can understand which package to install, what defaults are safe, which optional providers are available, and which commands prove the package is ready for local consumption.

This step should make the product feel installable and maintainable after the consumer package smoke proved the packaged ASP.NET Core integration can run outside the repository's demo host.

#### Dependency

- `004_001` completed a repeatable clean-consumer smoke for `ShortenLink.AspNetCore`, using local packed output, SQLite default mode, and no `ShortenLink.Api` internals.
- Phase 001 delivered the reusable core, SQLite default persistence, endpoint mapping, demo API, and React flow.
- Phase 002 delivered configuration-driven PostgreSQL selection.
- Phase 003 delivered analytics, cache, rate limiting, Docker Compose, and CI validation.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/004/PHASE_SUMMARY.md`
- `.okf/phase/004/004_002-release-documentation-and-package-metadata-hardening.md`
- `README.md`
- `src/ShortenLink.Core/ShortenLink.Core.csproj`
- `src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj`
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`
- `scripts/smoke-consumer-package.ps1`

#### Acceptance Criteria

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

#### Foundation for Next Step

This step should leave the reusable package story coherent enough for a release checklist or final public API review. The next Phase 004 step can focus on any remaining release gate, such as CI alignment for the consumer smoke, package artifact inspection, or phase closure, without rewriting the consumer docs from scratch.

#### Verification

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

#### Done Notes

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


## Scan Rule
Agents must read this file before working on any `004_*` task note. Do not activate Phase 004 until Phase 003 is complete.
