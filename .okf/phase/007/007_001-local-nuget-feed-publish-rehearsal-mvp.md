---
id: 007_001
phase: 007
task: 001
title: Local NuGet feed publish rehearsal MVP
status: done
created_at: 2026-07-15
completed_at: 2026-07-15
owner: codex
type: release-automation
priority: high
depends_on:
  - 006
tags:
  - release
  - nuget
  - local-feed
  - rehearsal
  - package
  - phase-7
---

# 007_001 - Local NuGet Feed Publish Rehearsal MVP

## Step Goal

Add a repeatable local NuGet feed publish rehearsal that exercises the Phase 006 publish workflow shape without pushing to NuGet.org or requiring real credentials.

This step should prove maintainers can validate package ordering, duplicate-version behavior, feed installation, and clean consumer behavior before considering a real external NuGet publish.

## Dependency

- Phase 005 added `scripts\release-dry-run.ps1` for package artifact validation.
- Phase 006 added `scripts\publish-nuget.ps1`, a manual publish wrapper with explicit intent and credential guardrails.
- Phase 006 documented the manual publish workflow and required post-publish consumer verification.

## Scope

In:

- Add or document a local feed rehearsal command that can run from the repository root.
- Reuse the release dry-run artifact validation before creating or publishing rehearsal feed packages.
- Use a safe local feed target under `.tmp` by default.
- Exercise all three reusable packages: `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Verify a clean consumer app can install `ShortenLink.AspNetCore` from the rehearsal feed and run create, detail, redirect, deactivate, and post-delete redirect checks.
- Document duplicate-version expectations and how maintainers should reset or intentionally retry the rehearsal feed.
- Keep the real NuGet publish path credential-gated and separate.

Out:

- Do not publish to NuGet.org.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not make CI publish automatically.
- Do not require Docker, PostgreSQL, Redis, or frontend assets for this rehearsal.
- Do not change package IDs, public endpoints, or service contracts unless a concrete rehearsal blocker is discovered.
- Do not add external registry or GitHub Release automation.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/007/PHASE_SUMMARY.md`
- `.okf/phase/007/007_001-local-nuget-feed-publish-rehearsal-mvp.md`
- `README.md`
- `scripts\release-dry-run.ps1`
- `scripts\publish-nuget.ps1`
- `scripts\smoke-consumer-package.ps1`
- `scripts\rehearse-local-feed.ps1`
- `scripts\`

## Acceptance Criteria

- A local feed rehearsal command exists or is documented clearly enough to run from the repository root.
- The rehearsal validates package artifacts before making them available through the rehearsal feed.
- The rehearsal feed contains `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` for the requested package version.
- A clean consumer smoke installs `ShortenLink.AspNetCore` from the rehearsal feed and verifies create, detail, redirect, deactivate, and post-delete redirect behavior.
- Duplicate-version behavior is either handled by a clear reset path or verified with an intentional skip/duplicate strategy.
- Documentation clearly distinguishes local feed rehearsal from real NuGet.org publishing.
- No real credentials are required or committed.
- Existing release checks remain valid:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
  - `.\scripts\release-dry-run.ps1`
  - `.\scripts\smoke-consumer-package.ps1`

## Foundation for Next Step

This step should leave a safe rehearsal feed path that maintainers can run before real publishing. The next step can decide whether to add internal-feed credentials, final product closure documentation, or no further release automation.

## Verification

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

Run the local feed rehearsal command added or documented by this task. Do not run a real `dotnet nuget push` against NuGet.org.

## Done Notes

- Completed on 2026-07-15.
- Added `scripts\rehearse-local-feed.ps1`, a local feed rehearsal script that runs `scripts\release-dry-run.ps1`, copies validated `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` packages into `.tmp\local-nuget-feed`, and runs the clean consumer smoke against that feed.
- Added duplicate-version guardrails: the rehearsal fails if the feed already contains the requested package version, supports `-ResetFeed` for a clean rehearsal, and supports `-SkipDuplicate` for intentional retry rehearsal.
- Updated `scripts\smoke-consumer-package.ps1` with `-UseExistingPackageSource` so release scripts can validate a feed without deleting or regenerating it.
- Updated `README.md` with the local feed rehearsal command, duplicate/reset guidance, `-KeepArtifacts`, and existing-source consumer smoke usage.
- Did not publish to NuGet.org, add credentials, add automatic publish behavior, or require Docker/PostgreSQL/Redis/frontend assets.
- Verified local feed rehearsal with `powershell.exe -ExecutionPolicy Bypass -File .\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0 -ResetFeed`; it copied all three reusable packages to `.tmp\local-nuget-feed`, reported `PublishedToNuGetOrg: false`, `RequiresCredentials: false`, and completed consumer smoke from `PackageSourceMode: Existing`.
- Verified duplicate guard with `powershell.exe -ExecutionPolicy Bypass -File .\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0`; it failed with the expected message telling maintainers to use `-ResetFeed` or `-SkipDuplicate`.
- Verified intentional retry with `powershell.exe -ExecutionPolicy Bypass -File .\scripts\rehearse-local-feed.ps1 -PackageVersion 1.0.0 -SkipDuplicate`; it skipped duplicate `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore` packages and completed consumer smoke from the existing feed.
- Verified repo checks:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
  - `powershell.exe -ExecutionPolicy Bypass -File .\scripts\release-dry-run.ps1`
  - `powershell.exe -ExecutionPolicy Bypass -File .\scripts\smoke-consumer-package.ps1`
- Initial non-escalated verification attempts that restored packages were blocked by sandboxed NuGet access to `api.nuget.org`; after approval, test, release dry-run, standard consumer smoke, and local-feed rehearsal commands completed successfully.
- `dotnet test` passed 69 tests total.
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Local feed and standard consumer smokes completed with create `201`, detail `200`, redirect `302`, delete `200`, and post-delete redirect `410`.
