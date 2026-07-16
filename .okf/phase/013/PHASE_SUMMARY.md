---
phase: 013
title: Admin Security Management
status: complete
created_at: 2026-07-16
updated_at: 2026-07-16
current_task: null
task_count: 2
done_count: 2
depends_on:
  - 011
---

# Phase 013 Summary

## Phase Goal

Add an admin-managed security configuration surface for the existing permission-based API-key model, starting with safe management of persisted credential assignments to built-in system roles and explicit permissions.

Phase 013 should turn the Phase 011 backend security foundation into something an operator can manage deliberately, while still avoiding custom roles, full user lifecycle, OAuth/OIDC, or SaaS identity scope.

## Phase Done Criteria

- Admin API can list persisted credential/security assignments without exposing raw API keys.
- Admin API can create or update an assignment using built-in system roles and/or explicit permissions.
- Admin API can disable an assignment so the credential is rejected consistently.
- Management actions require a high-trust permission such as `security.assignments.manage`.
- Built-in roles remain non-customizable bundles; permissions remain the source of truth.
- Local/demo bootstrap credentials remain usable for initial setup.
- Tests cover list, create/update, disable, validation, and protected access.
- README documents the security-management model and scope boundaries.

## Scope

In:

- Permission constant for managing security assignments.
- Backend request/response contracts for assignment management.
- Admin API endpoints for list, upsert, and disable assignment.
- Validation that roles are built-in and permissions are known.
- Protection of management endpoints through the existing authorization boundary.
- Focused tests and README documentation.

Out:

- Custom role creation/editing/deletion.
- User accounts, invitations, password reset, profiles, or session management.
- OAuth/OIDC/JWT provider integration.
- Frontend management UI until a later task.
- Multi-tenant organization security.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 013_001 | Security assignment management API MVP | done | 2026-07-16T22:47:04+07:00 |
| 013_002 | Admin security assignment UI MVP | done | 2026-07-16T22:59:02+07:00 |

## Current Task

No active task. Phase 013 is complete.

## Completed Notes

- `013_001` added the `security.assignments.manage` permission, protected backend APIs to list, upsert, and disable persisted security assignments without returning raw API keys, validation for built-in roles/known permissions, README documentation, and repository/API tests.
- `013_002` added a permission-aware React admin security dialog for listing, creating/updating, and disabling persisted credential assignments without exposing raw API keys.

## Next Task Proposal

Phase 013 is complete. Next, decide whether to add audit logs for security changes, key-rotation guidance, or another product gap phase.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files. Done task notes can be compacted into this summary later.

## Scan Rule

Read this file before working on any `013_*` task note. Keep security management permission-based, system-role-only, and credential-safe; do not introduce custom role management or user lifecycle unless a later phase explicitly changes scope.
