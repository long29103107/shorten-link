---
id: 009_001
phase: 009
task: 001
title: NuGet.org publish preflight checklist
status: done
created_at: 2026-07-15
completed_at: 2026-07-15
owner: codex
type: release-preflight
priority: high
depends_on:
  - 008
tags:
  - release
  - nuget
  - publishing
  - preflight
  - phase-9
---

# 009_001 - NuGet.org Publish Preflight Checklist

## Step Goal

Create a maintainer-facing preflight checklist for a future live NuGet.org publish attempt that separates repository-ready evidence from external registry prerequisites such as package ID ownership, account access, API key scope, and maintainer approval.

This task should make the go/no-go decision explicit before anyone runs `scripts\publish-nuget.ps1 -Publish`, while keeping the repository default path dry-run-only and credential-free.

## Dependency

- Phase 008 closed the current product definition with `docs\release-readiness-closure.md`.
- Phase 007 proved local feed rehearsal and clean consumer installation without NuGet.org.
- Phase 006 added `scripts\publish-nuget.ps1`, which previews by default and requires explicit `-Publish` intent plus NuGet credentials.
- Phase 005 added `scripts\release-dry-run.ps1` for package artifact validation.

## Scope

In:

- Add or update documentation with a live publish preflight checklist.
- List the external facts a maintainer must confirm: NuGet.org account access, package ID ownership or availability, API key scope, package owner/organization, version choice, and approval authority.
- Map required local gates to commands: build, test, pack, release dry-run, local feed rehearsal, and consumer package smoke.
- Document the final manual publish command shape without embedding secrets.
- Define go/no-go outcomes, including what to do when ownership, credentials, or approval are missing.
- Keep the default release workflow dry-run-only.

Out:

- Do not publish to NuGet.org.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not automate publish-on-push, publish-on-tag, GitHub Releases, or production deployment.
- Do not change package IDs, package metadata, public APIs, or endpoint contracts unless the checklist exposes a concrete blocker for a later task.
- Do not require Docker, PostgreSQL, Redis, or frontend assets for this preflight.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/009/PHASE_SUMMARY.md`
- `.okf/phase/009/009_001-nuget-org-publish-preflight-checklist.md`
- `docs\release-readiness-closure.md`
- `README.md`
- `scripts\publish-nuget.ps1`
- `scripts\release-dry-run.ps1`
- `scripts\rehearse-local-feed.ps1`
- `scripts\smoke-consumer-package.ps1`

## Acceptance Criteria

- A maintainer can tell exactly which local commands must pass before considering a live publish.
- The preflight checklist names the external NuGet.org prerequisites that repository tests cannot verify.
- The documentation explains how to provide `NUGET_API_KEY` or `-NuGetApiKey` without committing secrets.
- The checklist includes package ID and version review for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Go/no-go outcomes are explicit: missing ownership, credentials, or approval stops the publish attempt.
- The live publish command remains manual and uses the existing credential-protected script.
- No secrets, credentials, package pushes, or automatic publishing behavior are added.

## Foundation for Next Step

This step should leave maintainers with a clear external-readiness gate. The next step can only create a real live-publish task if registry ownership, API key handling, package version, and maintainer approval are confirmed outside the repository.

## Verification

For docs-only changes, at minimum read back the changed documentation and task bookkeeping.

If the implementation changes scripts, package metadata, CI, or executable release command paths, run:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
.\scripts\release-dry-run.ps1
.\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0 -ResetFeed
.\scripts\smoke-consumer-package.ps1
```

Do not run a real `dotnet nuget push` against NuGet.org for this task.

## Done Notes

- Added `docs\nuget-publish-preflight.md` with maintainer-facing local gates, external NuGet.org prerequisites, credential handling, go/no-go outcomes, package ID/version review, and post-publish checks.
- Linked the preflight from `README.md` release and manual publish sections.
- Updated `docs\release-readiness-closure.md` so release readiness distinguishes repository-ready evidence from external NuGet.org facts.
- Verification was docs-only: read back the changed documentation and task bookkeeping. No publish command or package push was run.
