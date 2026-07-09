---
phase: 002
title: PostgreSQL Provider Toggle
status: planned
created_at: 2026-07-09
updated_at: 2026-07-09
current_task: null
task_count: 0
done_count: 0
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

## Current Task

No task is active. Phase 002 starts after Phase 001 done criteria are verified.

## Completed Notes

No phase tasks are complete yet.

## Next Task Proposal

Create `002_001 - PostgreSQL provider toggle MVP` after Phase 001 is complete.

## Scan Rule

Agents must read this file before loading any `002_*` task file. Do not activate Phase 002 until Phase 001 is complete.

