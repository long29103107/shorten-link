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

## Scan Rule

Agents must read this file before loading any `006_*` task file. Do not activate Phase 006 until Phase 005 is complete.
