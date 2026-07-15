---
phase: 007
title: Local Feed Publish Rehearsal
status: complete
created_at: 2026-07-15
updated_at: 2026-07-15
current_task: null
task_count: 1
done_count: 1
depends_on:
  - 006
---

# Phase 007 Summary

## Phase Goal

Prove the credential-protected publish workflow can be rehearsed safely against a local or internal NuGet-compatible feed before any real external NuGet publish is considered.

Phase 007 should close the gap between package validation and publish operations by exercising the publish command shape, package ordering, duplicate-version behavior, and post-publish consumer installation without using NuGet.org, real API keys, or public package mutation.

## Phase Done Criteria

- Maintainers can run a repeatable publish rehearsal against a local or internal feed from the repository root.
- The rehearsal uses the existing package validation and publish guardrails instead of inventing a parallel release path.
- The rehearsal publishes or copies all three reusable packages into a safe feed target: `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Duplicate-version behavior is documented and verified against the rehearsal feed.
- A clean consumer app can install from the rehearsal feed and verify create, detail, redirect, deactivate, and post-delete redirect behavior.
- No packages are pushed to NuGet.org.
- No real API keys, secrets, tokens, or credentials are committed.

## Scope

In:

- Local or internal NuGet-compatible feed rehearsal script or documented workflow.
- Reuse of `scripts\release-dry-run.ps1`, `scripts\publish-nuget.ps1`, and `scripts\smoke-consumer-package.ps1` where practical.
- Package ordering and duplicate-version rehearsal behavior.
- Clean consumer install verification from the rehearsal feed.
- README updates that explain when to use rehearsal versus real publishing.

Out:

- Do not publish to NuGet.org.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not create automatic publish-on-push behavior.
- Do not require a remote feed unless a later task explicitly chooses one.
- Do not create production deployment automation.
- Do not change package IDs or public APIs unless the rehearsal exposes a concrete blocker.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 007_001 | Local NuGet feed publish rehearsal MVP | done | 2026-07-15 |

## Current Task

No task is active. Phase 007 is complete.

## Completed Notes

- `007_001` completed on 2026-07-15. Added a local feed rehearsal script that validates reusable packages, copies them to `.tmp\local-nuget-feed`, supports duplicate/reset retry behavior, and verifies a clean consumer app from the existing rehearsal feed without publishing to NuGet.org or requiring credentials. Verified reset rehearsal, duplicate fail-closed behavior, skip-duplicate retry, build, test, pack, release dry-run, and standard consumer package smoke.

## Next Task Proposal

Phase 007 is complete. Next, decide whether to add internal-feed credential rehearsal or finalize the current product definition without adding a live external publish task.

## Scan Rule

Agents must read this file before loading any `007_*` task file. Do not activate Phase 007 until Phase 006 is complete.
