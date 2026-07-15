---
phase: 010
title: HTTP Status Experience
status: planned
created_at: 2026-07-15
updated_at: 2026-07-15
current_task: 010_001
task_count: 1
done_count: 0
depends_on:
  - 008
---

# Phase 010 Summary

## Phase Goal

Add a small, reusable HTTP status experience for the demo app and integration surface so unauthorized, forbidden, and not-found outcomes are handled consistently.

Phase 010 should make `401`, `403`, and `404` visible as deliberate product states instead of incidental browser or JSON failures, while preserving the reusable package boundary.

## Phase Done Criteria

- The React demo has explicit user-facing routes or states for `401`, `403`, and `404`.
- API or endpoint behavior that returns `401`, `403`, or `404` has stable response expectations.
- The existing short-link fallback behavior remains compatible with the new `404` experience.
- The implementation does not introduce authentication as a product feature unless a task explicitly scopes it.
- Verification covers routing/status behavior with focused frontend and/or API checks.

## Scope

In:

- Demo UI status pages or shared status component for `401`, `403`, and `404`.
- Route parsing/navigation support for those status pages.
- Stable API/error response expectations where the backend already emits these statuses.
- README or developer notes only if the new status routes need documentation.

Out:

- Do not add login, users, roles, JWT, OAuth, or tenant authorization.
- Do not change package IDs or release automation.
- Do not publish packages.
- Do not replace the existing short-link create/detail/redirect/deactivate flows.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 010_001 | HTTP 401 403 404 status pages MVP | planned | |

## Current Task

`010_001 - HTTP 401 403 404 status pages MVP`

## Completed Notes

- None yet.

## Next Task Proposal

After `010_001`, decide whether any protected admin/auth flow is actually needed. Do not add authentication unless there is a concrete product requirement.

## Scan Rule

Agents must read this file before loading any `010_*` task file. Do not activate Phase 010 implementation until prior active phase work is intentionally paused, completed, or superseded by user direction.
