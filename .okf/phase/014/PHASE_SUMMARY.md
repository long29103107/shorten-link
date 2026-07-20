---
phase: 014
title: User And Role Based Security
status: done
created_at: 2026-07-16
updated_at: 2026-07-17
completed_at: 2026-07-17T15:48:54+07:00
current_task: null
task_count: 6
done_count: 6
depends_on:
  - 013
---

# Phase 014 Summary

## Phase Goal

Replace the credential-assignment-only security model with a user and role based security model.

Phase 014 should support built-in system roles and custom roles as permission bundles, a hidden default admin user for bootstrap access, user login, and per-user API keys that can be used by API clients without exposing raw secrets after creation.

## Phase Done Criteria

- Permissions remain the source of truth for authorization decisions.
- System roles exist as built-in role bundles and cannot be deleted.
- Custom roles can be created and assigned a set of supported permissions.
- A bootstrap admin user exists with username `admin` and password `admin` for local/demo setup.
- The bootstrap admin user is hidden from normal user-management UI.
- Users can be created and assigned system and/or custom roles.
- Users can log in through the admin UI.
- A logged-in user can create and manage their own API keys.
- Raw passwords and raw API keys are never stored or displayed after creation.
- API authorization can evaluate permissions from the logged-in user or from a user-owned API key.
- Existing protected short-link and security endpoints remain protected by permissions.
- Tests cover role bundles, custom-role permission validation, hidden bootstrap admin behavior, login, and user API-key authorization.
- README documents local bootstrap credentials and the security model boundaries.

## Scope

In:

- Security domain model for users, roles, permissions, and user-owned API keys.
- Built-in system roles: `Owner`, `Admin`, `Editor`, and `Viewer`.
- Custom role persistence with a permission bundle.
- Bootstrap admin user seeded as `admin` / `admin`.
- Password hashing and API-key hashing.
- Backend APIs needed to manage users, roles, and API keys.
- Frontend login and security-management UI.
- Migration path from Phase 013 credential assignments where practical.

Out:

- OAuth/OIDC/SAML integration.
- Password reset email flows.
- Multi-factor authentication.
- Multi-tenant organization hierarchy.
- Public signup.
- Billing or SaaS account lifecycle.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 014_001 | Security identity domain and persistence foundation | done | 2026-07-17T14:33:19+07:00 |
| 014_002 | User login and authenticated admin session API | done | 2026-07-17T15:10:32+07:00 |
| 014_003 | Role and user management API MVP | done | 2026-07-17T15:19:33+07:00 |
| 014_004 | User-owned API key management and authorization | done | 2026-07-17T15:27:21+07:00 |
| 014_005 | Admin login and identity management UI MVP | done | 2026-07-17T15:44:56+07:00 |
| 014_006 | Security model verification and documentation closure | done | 2026-07-17T15:48:54+07:00 |

## Current Task

No active task. Phase 014 is complete.

## Completed Notes

- `014_001` added Core security identity contracts for permissions, system role bundles, custom roles, users, user-owned API keys, and credential hashing; added EF Core persistence/repositories/schema initialization; registered the repositories in DI; and ensured a hidden bootstrap admin user is seeded with hashed password material.
- `014_002` added password verification, signed local/demo user session tokens, login/current-user APIs, Bearer-token authorization for protected admin endpoints, README/appsettings documentation, and tests for bootstrap login, failed login, disabled users, current-user permissions, and role-derived endpoint authorization.
- `014_003` added protected backend APIs for listing system/custom roles, creating/updating/disabling custom roles, listing non-hidden users, creating/updating/disabling normal users, rejecting invalid permissions or roles, protecting bootstrap admin from normal management APIs, README documentation, and focused repository/API tests.
- `014_004` added logged-in user API-key list/create/rename/disable APIs, one-time raw key create responses, hash-only key persistence, ownership checks, user-owned API-key authorization through owner role permissions, README documentation, and tests for key metadata isolation, disabled-key rejection, and protected endpoint access.
- `014_005` added the frontend login/session bootstrap, Bearer-token admin request path, permission-aware navigation, identity-management UI for users/roles/API keys, one-time raw API-key display, focused Bun tests, and successful frontend/backend verification.
- `014_006` closed Phase 014 by auditing coverage, tightening README security documentation for local/demo identity workflows, and passing final backend/frontend verification.

## Next Task Proposal

Phase 014 is complete. Proposed next phase: `015 - Admin discovery and filtering`, focused on search/filter/sort for the protected admin short-link list.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files. Done task notes can be compacted into this summary later.

## Scan Rule

Read this file before working on any `014_*` task note. Keep permissions as the source of truth. Treat system roles as built-in role bundles, custom roles as persisted role bundles, users as assignable principals, and API keys as user-owned credentials.
