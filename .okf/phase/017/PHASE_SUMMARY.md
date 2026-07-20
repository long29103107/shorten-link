---
phase: 017
title: Validation Parity And Field Error Mapping
status: active
created_at: 2026-07-20
updated_at: 2026-07-20
current_task: null
task_count: 1
done_count: 1
depends_on:
  - 016
---

# Phase 017 Summary

## Phase Goal

Keep frontend and backend validation behavior aligned and map rejected inputs to the correct controls across short-link and security-management workflows.

## Phase Done Criteria

- API validation failures expose stable field-level metadata without breaking the existing `errorCode` and `message` contract.
- Short-link create and update frontend validation matches backend URL and expiration rules.
- Login and security-management forms map backend validation failures to the relevant fields or field groups.
- Client-side checks do not claim acceptance for values the backend deterministically rejects.
- Server-only or race-sensitive validation remains authoritative and is presented contextually.
- Unknown validation errors retain a safe form-level fallback.
- Focused backend and frontend tests cover field metadata, validation parity, error mapping, and compatibility.
- README documents the field-aware validation error shape and supported behavior.
- Backend and production frontend builds succeed.

## Scope

In:

- Backward-compatible API field-error metadata for validation failures.
- Shared frontend validation and field-error mapping helpers.
- Short-link, login, user, role, API-key, and assignment form parity.
- Focused backend/frontend tests and documentation.

Out:

- Schema-driven form generation or a new validation framework.
- Localization of validation messages.
- Remote URL reachability checks or private-network policy changes.
- Password-strength scoring, breached-password services, MFA, or OAuth/OIDC.
- Changes to non-validation authorization, conflict, or outage behavior.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 017_001 | Field-aware API validation error contract | done | 2026-07-20T16:23:23+07:00 |

## Current Task

No active task. `017_001` is complete.

## Completed Notes

- `017_001` added a backward-compatible field-aware API validation error contract across short-link and security requests, documented it, and verified it with the backend build and 67 API tests.

## Next Task Proposal

Create `017_002` to consume `fieldErrors` in the short-link create/update frontend, centralize matching client validation rules, and add focused Bun tests.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files. Done task notes can be compacted into this summary later.

## Scan Rule

Read this file before working on any `017_*` task note. Preserve `errorCode` and `message` compatibility; field metadata must be additive and server validation remains authoritative.
