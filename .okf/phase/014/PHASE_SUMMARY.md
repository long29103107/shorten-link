---
phase: 014
title: User And Role Based Security
status: active
created_at: 2026-07-16
updated_at: 2026-07-16
current_task: 014_001
task_count: 1
done_count: 0
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
| 014_001 | Security identity domain and persistence foundation | active |  |

## Current Task

`014_001` is active.

## Completed Notes

- None yet.

## Next Task Proposal

After `014_001`, add login/session or token issuance APIs so frontend authentication can stop relying on Vite-configured admin API keys.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files. Done task notes can be compacted into this summary later.

## Scan Rule

Read this file before working on any `014_*` task note. Keep permissions as the source of truth. Treat system roles as built-in role bundles, custom roles as persisted role bundles, users as assignable principals, and API keys as user-owned credentials.
