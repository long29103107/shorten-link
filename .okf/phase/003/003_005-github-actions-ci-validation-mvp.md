---
id: 003_005
phase: 003
task: 005
title: GitHub Actions CI validation MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: ci
priority: high
depends_on:
  - 003_004
tags:
  - github-actions
  - ci
  - build
  - tests
  - phase-3
---

# 003_005 - GitHub Actions CI Validation MVP

## Step Goal

Add a minimal GitHub Actions CI workflow that validates the reusable Shorten Link package boundary and demo host on push and pull request, so Phase 003 has automated build and test feedback for the production-readiness work completed so far.

This step should make CI mirror the required local repo verification without introducing external infrastructure dependencies such as PostgreSQL, Redis, Docker daemon access, package publishing, secrets, or deployment.

## Dependency

- `003_001` completed async click analytics and endpoint coverage.
- `003_002` completed cache abstraction, memory provider, Redis provider selection, and related tests.
- `003_003` completed endpoint rate limiting and endpoint tests.
- `003_004` completed the local operational Docker Compose shape and documented Docker daemon blockers separately from repo-side verification.

## Scope

In:

- Add a GitHub Actions workflow for push and pull request validation.
- Set up the .NET SDK version required by the solution.
- Restore, build, test, and pack the solution or packable projects.
- Keep CI independent from live PostgreSQL, Redis, Docker Compose, and local machine state.
- Use repository-native commands already required by Phase 003 verification.
- Add README or task notes only if needed to document the CI command surface.

Out:

- Do not add package publishing to NuGet.
- Do not add deployment, container registry publishing, release automation, or environment secrets.
- Do not require Docker daemon access in CI for this MVP.
- Do not add database service containers unless an existing test truly requires them.
- Do not change product behavior just to satisfy CI.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_005-github-actions-ci-validation-mvp.md`
- `.github/workflows/`
- `README.md`
- `ShortenLink.slnx`

## Acceptance Criteria

- A GitHub Actions workflow exists under `.github/workflows/`.
- The workflow runs on push and pull request.
- The workflow installs the .NET SDK version needed by `ShortenLink.slnx`.
- The workflow restores dependencies.
- The workflow builds the solution.
- The workflow runs the test suite.
- The workflow verifies package creation for packable reusable projects or the solution pack command used locally.
- The workflow does not require Docker, PostgreSQL, Redis, credentials, or publishing permissions.
- The workflow keeps default SQLite-based tests and config-driven optional providers compatible with CI.
- Phase verification passes locally with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

## Foundation for Next Step

This step should close the remaining Phase 003 CI done criterion. After it is complete, Phase 003 can be evaluated for closure against all done criteria before deciding whether to open the next phase.

## Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If local GitHub Actions tooling such as `act` is available, optionally validate the workflow structure. Do not make `act` mandatory for task completion.

## Done Notes

- Completed on 2026-07-12.
- Added `.github/workflows/ci.yml` with push and pull request triggers.
- The workflow uses `actions/checkout@v7`, `actions/setup-dotnet@v5`, and .NET `10.0.x`.
- CI restores `ShortenLink.slnx`, builds the solution, runs the test suite, and packs the solution without requiring Docker, PostgreSQL, Redis, credentials, package publishing, or deployment permissions.
- Verified locally with the same command surface:
  - `dotnet restore ShortenLink.slnx --verbosity minimal`
  - `dotnet build ShortenLink.slnx --no-restore --verbosity minimal`
  - `dotnet test ShortenLink.slnx --no-build --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --no-restore --verbosity minimal`
- Verified required phase commands:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Optional local `act` validation was skipped because `act` is not installed in the current shell.
