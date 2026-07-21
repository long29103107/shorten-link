---
phase: 017
title: Validation Parity And Field Error Mapping
status: done
created_at: 2026-07-20
updated_at: 2026-07-21
current_task: null
task_count: 4
done_count: 4
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
| 017_002 | Short-link frontend validation parity and field mapping | done | 2026-07-21T09:19:08+07:00 |
| 017_003 | Login and managed user form parity | done | 2026-07-21T10:15:00+07:00 |
| 017_004 | Security identity tab workflow and validation parity | done | 2026-07-21T13:30:55+07:00 |

## Current Task

No active task. Phase 017 is complete.

## Completed Notes

- `017_001` added a backward-compatible field-aware API validation error contract across short-link and security requests, documented it, and verified it with the backend build and 67 API tests.
- `017_002` centralized deterministic short-link form validation, preserved API `fieldErrors` through the frontend failure model, mapped URL and expiration failures to exact create/update controls, and verified the result with 23 Bun tests and a production frontend build.
- `017_003` aligned login and protected managed-user registration/update with the 401-style card language, added exact identity field validation/mapping, normalized array-valued API field metadata, and verified the result with 27 Bun tests and a production frontend build.
- `017_004` reduced registration to email/display name/password, separated password reset and role assignment, reorganized Security into Users/Roles/Permissions, and verified field mapping with 29 Bun tests and a production frontend build.

## Next Task Proposal

Phase 017 is complete. Propose phase 018 from the next ordered product-vision gap before creating it.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files. Done task notes can be compacted into this summary later.

## Scan Rule

Read this file before working on any `017_*` task note. Preserve `errorCode` and `message` compatibility; field metadata must be additive and server validation remains authoritative.
