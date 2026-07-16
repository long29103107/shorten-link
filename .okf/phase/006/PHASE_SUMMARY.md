---
phase: 006
title: Credential-Protected Publishing Workflow
status: complete
created_at: 2026-07-15
updated_at: 2026-07-15
current_task: null
task_count: 1
done_count: 1
depends_on:
  - 005
---

# Phase 006 Summary

## Phase Goal

Prepare a secure, explicit NuGet publishing workflow that builds on the Phase 005 dry-run release gate while keeping real package publication protected by maintainer intent, credentials, and verification checks.

Phase 006 should turn the local release dry-run into a publish-ready operational path without making accidental or unauthenticated publishing possible. The phase must keep credentials out of source control and require a deliberate manual action before any `dotnet nuget push` path can run.

## Phase Done Criteria

- Maintainers have a documented manual publishing workflow that starts from the existing release dry-run and consumer smoke.
- Any publish-capable script or CI workflow requires explicit opt-in, a package version, and NuGet credentials supplied through the execution environment.
- The default command path remains dry-run-only and cannot publish packages by accident.
- Package artifacts are validated before a publish step can proceed.
- Documentation explains required secrets, version checks, rollback or unlist considerations, and how to verify package availability after publication.
- Build, test, pack, release dry-run, and consumer smoke remain the required release gate before any publish attempt.
- No NuGet API key, secret value, or real publish action is added by default.

## Scope

In:

- Manual NuGet publishing workflow design.
- Credential and explicit-intent guardrails for publish-capable automation.
- Documentation for required secrets, version review, artifact validation, publish command shape, and post-publish verification.
- Optional CI workflow scaffolding that is manual-only and refuses to run without secrets.
- Reuse of the existing `scripts/release-dry-run.ps1` artifact validation.

Out:

- Do not publish to NuGet.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not create automatic publish-on-push behavior.
- Do not create production deployment automation.
- Do not change package IDs or public APIs unless publish-readiness checks expose a concrete blocker.
- Do not add SaaS billing, tenants, authentication, or authorization.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 006_001 | Manual NuGet publish workflow guardrails MVP | done | 2026-07-15 |

## Current Task

No task is active. Phase 006 is complete.

## Completed Notes

- `006_001` completed on 2026-07-15. Added a manual NuGet publish wrapper that previews by default, requires explicit `-Publish` intent and NuGet credentials, masks API key display, reruns the release dry-run before pushing, and documents the full maintainer publish workflow without committing secrets or publishing packages. Verified preview mode, no-key fail-closed behavior, build, test, pack, release dry-run, and consumer package smoke.

## Next Task Proposal

Phase 006 is complete. Next, decide whether to rehearse publication against a safe internal/local feed or finalize the current product definition without adding a live external publish task.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 006_001 - Manual NuGet Publish Workflow Guardrails MVP

Source before compaction: `006_001-manual-nuget-publish-workflow-guardrails-mvp.md`

#### Step Goal

Add a maintainer-ready manual publishing path that reuses the Phase 005 release dry-run, documents required credentials and verification steps, and makes any publish-capable command fail closed unless a maintainer explicitly opts in with the required environment secrets.

This step should prepare the repository for a future real NuGet publish without publishing packages, committing credentials, or weakening the dry-run-only default.

#### Dependency

- Phase 004 validated that a clean consumer app can install and use the packaged `ShortenLink.AspNetCore` integration.
- Phase 005 added `scripts/release-dry-run.ps1`, package artifact inspection, publish guardrails, and a release checklist for maintainers.

#### Scope

In:

- Document the exact manual NuGet publish workflow from version review through post-publish verification.
- Add or update publish-capable automation only if it is manual, explicit, and credential-gated.
- Require maintainers to run build, test, pack, release dry-run, and consumer smoke before any publish attempt.
- Require a NuGet API key or equivalent credential from the execution environment, never from source.
- Make publish-capable paths require an explicit intent flag or manual workflow input.
- Document package version checks, duplicate-version failure expectations, rollback or unlist guidance, and package availability verification.
- Preserve the default dry-run command as non-publishing.

Out:

- Do not publish to NuGet.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not make CI publish on push, pull request, tag, or release automatically.
- Do not create GitHub Releases or production deployment automation.
- Do not change package IDs, public endpoints, or service contracts unless a concrete publish-readiness blocker is discovered.
- Do not require Docker, PostgreSQL, Redis, or frontend assets for the publish workflow.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/006/PHASE_SUMMARY.md`
- `.okf/phase/006/006_001-manual-nuget-publish-workflow-guardrails-mvp.md`
- `README.md`
- `scripts/publish-nuget.ps1`
- `scripts/release-dry-run.ps1`
- `.github/workflows/`

#### Acceptance Criteria

- README or equivalent release documentation explains the manual publish workflow, required checks, required secrets, and post-publish verification.
- Any publish-capable script or workflow refuses to continue without explicit maintainer intent and required NuGet credentials.
- The default release dry-run remains non-publishing and still validates reusable packages before publish preparation.
- The workflow prevents automatic publishing from push, pull request, tag, or release events unless a later task deliberately changes that scope.
- Documentation covers version review, duplicate-version behavior, package artifact validation, consumer smoke, NuGet push command shape, rollback or unlist considerations, and package availability checks.
- No secrets or real credentials are committed.
- Existing release checks remain valid:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
  - `.\scripts\release-dry-run.ps1`
  - `.\scripts\smoke-consumer-package.ps1`

#### Foundation for Next Step

This step should leave Phase 006 with a guarded, documented publish path that a maintainer can intentionally execute later. The next step can decide whether to rehearse publication against a safe internal/local feed, add more CI approval gates, or finalize the product without attempting a live external publish.

#### Verification

Run repo verification if implementation changes scripts, CI, package metadata, or release docs with executable command paths:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

Run release verification:

```powershell
.\scripts\release-dry-run.ps1
.\scripts\smoke-consumer-package.ps1
```

If a publish-capable command or workflow is added, verify the fail-closed path without credentials and without publishing. Do not run a real `dotnet nuget push` against NuGet.

#### Done Notes

- Completed on 2026-07-15.
- Added `scripts/publish-nuget.ps1`, a manual publish wrapper that previews by default, requires `-Publish` for any real push, requires `NUGET_API_KEY` or `-NuGetApiKey`, masks the API key in command display, and reruns `scripts/release-dry-run.ps1` before `dotnet nuget push`.
- The script keeps package artifacts under `.tmp\nuget-publish`, validates `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` through the existing release dry-run, and supports `-SkipDuplicate` only for intentional publish retries.
- Updated `README.md` with the manual NuGet publish workflow, required release gate commands, credential expectations, preview command, intentional publish command, duplicate-version guidance, post-publish verification, and rollback/unlist guidance.
- Did not add automatic publish-on-push, publish-on-tag, GitHub Releases, secrets, credentials, or a real NuGet publish action.
- Verified publish preview with `powershell.exe -ExecutionPolicy Bypass -File .\scripts\publish-nuget.ps1 -PackageVersion 1.0.0`; it returned `Status: PreviewOnly` and `Published: false`.
- Verified fail-closed publish guard with `powershell.exe -ExecutionPolicy Bypass -File .\scripts\publish-nuget.ps1 -PackageVersion 1.0.0 -Publish`; it failed before package operations because no NuGet API key was supplied.
- Verified repo checks:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
  - `powershell.exe -ExecutionPolicy Bypass -File .\scripts\release-dry-run.ps1`
  - `powershell.exe -ExecutionPolicy Bypass -File .\scripts\smoke-consumer-package.ps1`
- Initial non-escalated verification attempts that restored packages were blocked by sandboxed NuGet access to `api.nuget.org`; after approval, build, test, pack, release dry-run, and consumer package smoke completed successfully.
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Consumer smoke completed with create `201`, detail `200`, redirect `302`, delete `200`, and post-delete redirect `410`.


## Scan Rule
Agents must read this file before working on any `006_*` task note. Do not activate Phase 006 until Phase 005 is complete.
