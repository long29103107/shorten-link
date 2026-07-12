---
phase: 004
title: Release And Consumer Hardening
status: active
created_at: 2026-07-12
updated_at: 2026-07-12
current_task: null
task_count: 1
done_count: 1
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

## Current Task

No active task. Phase 004 is ready for the next planned slice.

## Completed Notes

- `004_001` completed on 2026-07-12. Added a repeatable consumer package smoke that creates a clean ASP.NET Core app, installs `ShortenLink.AspNetCore` from local packed output, avoids demo API internals, runs SQLite default mode, and verifies create, detail, redirect, deactivate, and post-delete redirect behavior. README now documents the smoke command, and repo build, test, and pack verification passed.

## Next Task Proposal

Create `004_002 - release documentation and package metadata hardening` next.

## Scan Rule

Agents must read this file before loading any `004_*` task file. Do not activate Phase 004 until Phase 003 is complete.
