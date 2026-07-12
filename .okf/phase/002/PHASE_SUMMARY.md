---
phase: 002
title: PostgreSQL Provider Toggle
created_at: 2026-07-09
updated_at: 2026-07-12
status: complete
current_task: null
task_count: 2
done_count: 2
depends_on:
  - 001
---

# Phase 002 Summary

## Phase Goal

Allow the same reusable library and demo API to switch between SQLite and PostgreSQL by configuration only, without changing application code or weakening the NuGet package boundary.

## Phase Done Criteria

- SQLite remains the default provider.
- PostgreSQL can be enabled with `ShortenLink:Database:UsePostgres = true`.
- Provider selection is controlled by configuration, not code changes.
- Repository/service/API contracts remain unchanged.
- Required indexes exist for both providers where practical.
- README documents PostgreSQL setup, configuration, and migration/update commands.
- Verification covers provider selection and PostgreSQL setup where the local environment allows.

## Scope

In:

- PostgreSQL EF Core provider support.
- Configuration toggle and options.
- Migration/update database guidance.
- Provider-selection tests or verification.
- README updates.

Out:

- Redis cache provider.
- Analytics worker.
- Rate limiting.
- Docker Compose unless needed only as local PostgreSQL helper documentation.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 002_001 | PostgreSQL provider toggle MVP | done | 2026-07-12 |
| 002_002 | PostgreSQL live host smoke and setup guide | done | 2026-07-12 |

## Current Task

No task is active. Phase 002 is complete.

## Completed Notes

- `002_001` completed on 2026-07-12. Added configuration-driven PostgreSQL provider selection to the reusable library boundary, extended options validation and DbContext wiring, added provider-selection and PostgreSQL-model tests, and updated README PostgreSQL guidance while keeping SQLite as the default path.
- `002_002` completed on 2026-07-12. Added a reusable PostgreSQL host smoke script, documented the PostgreSQL prerequisites and run path, verified the exact local blocker when no PostgreSQL instance was reachable, and confirmed the repo-side Phase 002 verification set still passed.

## Next Task Proposal

Create `003_001 - async click analytics MVP` next.

## Scan Rule

Agents must read this file before loading any `002_*` task file. Do not activate Phase 002 until Phase 001 is complete.

