---
phase: 005
title: Publishing And Release Automation
status: complete
created_at: 2026-07-14
updated_at: 2026-07-14
current_task: null
task_count: 1
done_count: 1
depends_on:
  - 004
---

# Phase 005 Summary

## Phase Goal

Prepare Shorten Link for repeatable package release operations by adding safe publish dry-run checks, release checklist documentation, and automation guardrails that build on the completed consumer package validation without publishing packages by accident.

Phase 005 should turn the Phase 004 package confidence into a maintainer-ready release path. The phase must keep actual NuGet publishing explicit and manual unless a later task deliberately adds a secure publishing workflow.

## Phase Done Criteria

- Maintainers have a documented release checklist covering version review, build, test, pack, consumer smoke, package artifact inspection, and publish prerequisites.
- A repeatable dry-run command validates package artifacts before any external publish step.
- Release automation refuses to publish without explicit credentials and an intentional publish command.
- Package artifacts can be inspected for expected metadata, README inclusion, dependency shape, and reusable package IDs.
- CI or documented local verification continues to cover build, test, pack, and consumer smoke expectations.
- The reusable package boundary remains free of demo API/Web coupling.

## Scope

In:

- Release checklist documentation.
- Package artifact inspection or dry-run scripts.
- NuGet publish preparation guardrails.
- CI or local verification alignment for release checks where practical.
- Package metadata validation that does not require publishing.

Out:

- Do not publish to NuGet.
- Do not add real API keys, secrets, or credentials.
- Do not create production deployment automation.
- Do not add SaaS billing, tenants, authentication, or authorization.
- Do not introduce public API changes unless package inspection exposes a concrete release blocker.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 005_001 | NuGet publish dry-run and release checklist MVP | done | 2026-07-14 |

## Current Task

No task is active. Phase 005 is complete.

## Completed Notes

- `005_001` completed on 2026-07-14. Added a safe NuGet release dry-run script, package artifact inspection, publish guardrails, README release checklist, and verified build, test, pack, dry-run, publish guard, and consumer package smoke.

## Next Task Proposal

Phase 005 is complete. Next, decide whether to open a credential-protected publishing workflow phase or finalize the current product definition.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 005_001 - NuGet Publish Dry-Run And Release Checklist MVP

Source before compaction: `005_001-nuget-publish-dry-run-and-release-checklist-mvp.md`

#### Step Goal

Add a safe maintainer release path that validates the reusable NuGet packages and documents the exact checklist for publishing readiness without actually publishing packages.

This step should give maintainers confidence that `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` are ready to hand off or publish later, while making accidental publish impossible from the default command path.

#### Dependency

- Phase 004 completed clean consumer package validation for `ShortenLink.AspNetCore`.
- `004_002` hardened package metadata and README guidance for the reusable package surface.
- Phase 003 CI already validates restore, build, test, and pack for the solution.

#### Scope

In:

- Add or update release checklist documentation for maintainers.
- Add a local dry-run script or command path that builds package artifacts into a controlled output directory.
- Inspect package artifacts for expected package IDs, README inclusion, metadata, dependency shape, and absence of demo API/Web coupling.
- Document the difference between dry-run validation and a future real `dotnet nuget push` command.
- Keep publish credentials, API keys, and real publish operations out of this task.
- Reuse existing build, test, pack, and consumer smoke commands where possible.

Out:

- Do not publish to NuGet.
- Do not add NuGet API keys, secrets, encrypted credentials, or CI publishing permissions.
- Do not create GitHub Releases or deployment artifacts.
- Do not change package IDs, public endpoints, or service contracts unless artifact inspection exposes a concrete blocker.
- Do not require Docker, PostgreSQL, Redis, or frontend assets for the release dry-run.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/005/PHASE_SUMMARY.md`
- `.okf/phase/005/005_001-nuget-publish-dry-run-and-release-checklist-mvp.md`
- `README.md`
- `scripts/`
- `.github/workflows/ci.yml`
- `src/ShortenLink.Core/ShortenLink.Core.csproj`
- `src/ShortenLink.Infrastructure/ShortenLink.Infrastructure.csproj`
- `src/ShortenLink.AspNetCore/ShortenLink.AspNetCore.csproj`

#### Acceptance Criteria

- A release checklist exists and tells maintainers the order for version review, build, test, pack, consumer smoke, package artifact inspection, and publish prerequisites.
- A dry-run command exists or is documented clearly enough to run from the repository root.
- The dry-run validates that all three reusable packages are produced: `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- The dry-run or documented inspection verifies package metadata and README inclusion without pushing to NuGet.
- The dry-run path fails closed if a package artifact is missing or if a publish operation would require credentials.
- Documentation makes clear that real NuGet publishing is out of scope until a later explicit publish task.
- Existing release-hardening checks remain valid:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
  - `.\scripts\smoke-consumer-package.ps1`

#### Foundation for Next Step

This step should leave Phase 005 with a safe package-release gate. The next step can decide whether to wire that gate into CI or add a credential-protected manual publish workflow without having to invent the package inspection rules from scratch.

#### Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

Run the release dry-run command added or documented by this task.

Run the consumer smoke unless the task only changes documentation and package inspection logic:

```powershell
.\scripts\smoke-consumer-package.ps1
```

If any command needs NuGet network access, record that requirement or blocker precisely.

#### Done Notes

- Completed on 2026-07-14.
- Added `scripts/release-dry-run.ps1`, a safe package release dry-run that packs reusable packages into `.tmp/release-dry-run`, inspects `.nupkg` contents, validates package IDs, version, authors, README inclusion, MIT license expression, repository metadata, tags, dependency shape, `lib/net10.0` assemblies, and absence of demo API/Web coupling.
- The dry-run reports `Published: false` and rejects `-Publish` before any package operation, so the default release path cannot accidentally push packages.
- Updated `README.md` release-readiness guidance to include `.\scripts\release-dry-run.ps1`, explain what the dry-run validates, document `-KeepArtifacts`, and add a maintainer release checklist covering version review, build, test, pack, dry-run, consumer smoke, and future publish prerequisites.
- Verified publish guard with `.\scripts\release-dry-run.ps1 -Publish`; it failed closed with the expected dry-run-only error.
- The first dry-run attempt was blocked by sandboxed NuGet access to `api.nuget.org`; after approval, `.\scripts\release-dry-run.ps1` completed successfully and validated `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` artifacts.
- Verified repo checks:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
  - `.\scripts\smoke-consumer-package.ps1`
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Consumer smoke completed with create `201`, detail `200`, redirect `302`, delete `200`, and post-delete redirect `410`.


## Scan Rule
Agents must read this file before working on any `005_*` task note. Do not activate Phase 005 until Phase 004 is complete.
