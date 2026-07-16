---
phase: 009
title: Live Publishing Preflight
status: done
created_at: 2026-07-15
updated_at: 2026-07-15
current_task:
task_count: 1
done_count: 1
depends_on:
  - 008
---

# Phase 009 Summary

## Phase Goal

Prepare the project for a future intentional live NuGet.org publishing decision by documenting and verifying the external preconditions that cannot be proven by repository-only release automation.

Phase 009 should bridge the completed release-readiness closure to real registry operations without publishing packages by default, committing credentials, or adding automatic publish behavior.

## Phase Done Criteria

- Maintainers have a preflight checklist for NuGet.org package ID ownership, account access, API key scope, and approval authority.
- The checklist maps the existing local release gates to the exact evidence required before a live publish attempt.
- The live publish path remains manual, credential-protected, and opt-in only.
- Dry-run, local feed rehearsal, and consumer smoke remain the required repository-side gates before any external publish.
- Documentation distinguishes repository-ready evidence from external registry state that a maintainer must confirm.
- No NuGet.org package push, API key, secret, token, or automatic publish workflow is added.

## Scope

In:

- NuGet.org package ownership and maintainer approval preflight.
- Manual API key and package ID checks that happen outside source control.
- Mapping repository release gates to live publish readiness.
- Documentation for go/no-go criteria before `scripts\publish-nuget.ps1 -Publish`.
- Task bookkeeping for the live publishing preflight phase.

Out:

- Do not publish to NuGet.org.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not create automatic publish-on-push, publish-on-tag, GitHub Releases, or deployment automation.
- Do not change package IDs, public APIs, or package metadata unless a later preflight task exposes a concrete blocker.
- Do not add SaaS billing, tenants, authentication, authorization, or analytics dashboards.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 009_001 | NuGet.org publish preflight checklist | done | 2026-07-15 |

## Current Task

None. Phase 009 is complete unless an external maintainer confirms the live publish prerequisites and explicitly asks for a publish task.

## Completed Notes

- `009_001` added `docs\nuget-publish-preflight.md`, linked it from README, and updated release-readiness closure evidence. Maintainers now have explicit local gates, external NuGet.org prerequisites, credential handling rules, package ID/version review, and go/no-go outcomes before any `scripts\publish-nuget.ps1 -Publish` attempt.

## Next Task Proposal

Phase 009 has met its repository-side goal. The next implementation phase can move to Phase 010 HTTP Status Experience, starting with `010_001 - HTTP 401 403 404 status pages MVP`, unless a maintainer first confirms all external NuGet.org prerequisites and explicitly requests a live publish task.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 009_001 - NuGet.org Publish Preflight Checklist

Source before compaction: `009_001-nuget-org-publish-preflight-checklist.md`

#### Step Goal

Create a maintainer-facing preflight checklist for a future live NuGet.org publish attempt that separates repository-ready evidence from external registry prerequisites such as package ID ownership, account access, API key scope, and maintainer approval.

This task should make the go/no-go decision explicit before anyone runs `scripts\publish-nuget.ps1 -Publish`, while keeping the repository default path dry-run-only and credential-free.

#### Dependency

- Phase 008 closed the current product definition with `docs\release-readiness-closure.md`.
- Phase 007 proved local feed rehearsal and clean consumer installation without NuGet.org.
- Phase 006 added `scripts\publish-nuget.ps1`, which previews by default and requires explicit `-Publish` intent plus NuGet credentials.
- Phase 005 added `scripts\release-dry-run.ps1` for package artifact validation.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/009/PHASE_SUMMARY.md`
- `.okf/phase/009/009_001-nuget-org-publish-preflight-checklist.md`
- `docs\release-readiness-closure.md`
- `README.md`
- `scripts\publish-nuget.ps1`
- `scripts\release-dry-run.ps1`
- `scripts\rehearse-local-feed.ps1`
- `scripts\smoke-consumer-package.ps1`

#### Acceptance Criteria

- A maintainer can tell exactly which local commands must pass before considering a live publish.
- The preflight checklist names the external NuGet.org prerequisites that repository tests cannot verify.
- The documentation explains how to provide `NUGET_API_KEY` or `-NuGetApiKey` without committing secrets.
- The checklist includes package ID and version review for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Go/no-go outcomes are explicit: missing ownership, credentials, or approval stops the publish attempt.
- The live publish command remains manual and uses the existing credential-protected script.
- No secrets, credentials, package pushes, or automatic publishing behavior are added.

#### Foundation for Next Step

This step should leave maintainers with a clear external-readiness gate. The next step can only create a real live-publish task if registry ownership, API key handling, package version, and maintainer approval are confirmed outside the repository.

#### Verification

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

#### Done Notes

- Added `docs\nuget-publish-preflight.md` with maintainer-facing local gates, external NuGet.org prerequisites, credential handling, go/no-go outcomes, package ID/version review, and post-publish checks.
- Linked the preflight from `README.md` release and manual publish sections.
- Updated `docs\release-readiness-closure.md` so release readiness distinguishes repository-ready evidence from external NuGet.org facts.
- Verification was docs-only: read back the changed documentation and task bookkeeping. No publish command or package push was run.


## Scan Rule
Agents must read this file before working on any `009_*` task note. Do not activate Phase 009 until Phase 008 is complete.
