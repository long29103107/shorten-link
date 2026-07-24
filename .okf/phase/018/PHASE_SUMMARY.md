---
phase: 018
title: Admin Workspace and Security Management
status: complete
created_at: 2026-07-21
updated_at: 2026-07-24
current_task: null
task_count: 5
done_count: 5
completed_at: 2026-07-23T18:28:56+07:00
depends_on:
  - 017
---

# Phase 018 Summary

## Phase Goal

Deliver a coherent authenticated workspace for owned/shared short-link operations, user lifecycle management, and persistent two-role system permission control.

## Phase Done Criteria

- Public home, Short Links, Admin Dashboard, Users, and Roles have stable routes and context-specific navigation.
- Admin navigation is role-gated; authenticated surfaces expose account identity, workspace navigation, and sign-out.
- Shared DataTable, pagination, refresh, row-action, form-dialog, discovery, and bulk-selection primitives drive list workflows.
- Users support passwordless creation, immutable email during edit, display-name updates, confirmed password reset, role assignment, single deactivate, and bulk deactivate.
- System roles are fixed to `Admin` and `User`; `View` and `Edit` are per-link share levels rather than global roles.
- Security administration is enforced directly by the Admin role rather than a toggleable permission; audit visibility is available to Admin and User.
- Link lifecycle is represented by `short_links.status`, export follows `short_links.read`, and import uses `short_links.import`.
- Admin bypasses ownership/share checks and has full system access; User manages owned links and accesses another user's link only through a sufficient `View` or `Edit` share.
- Individual and group permission toggles stage immediately without mutation confirmation, one enabled-on-dirty Save changes action persists all role drafts, and effective overrides apply consistently during authorization and session issuance.
- Bootstrap startup guarantees an Admin system role with the full permission catalog and an `admin@shortenlink.local`/`admin` bootstrap identity assigned to it.
- Every newly created short link records its creator; Short Links scopes discovery and permitted actions by Admin, Owner, Edit, or View access.
- Owners/Admins can grant or revoke per-link View/Edit shares; View allows inspection/analytics and Edit additionally allows link mutation, while destructive/share management remains Owner/Admin-only.
- Short Links uses the shared management primitives, modal create/edit/share flows, ownership-aware bulk actions, and Active/Deactivated discovery.
- Empty, loading, retry, validation, mutation feedback, sticky breadcrumb behavior, and Stay/Discard navigation guards remain contextual.
- Focused frontend tests and production build pass.
- Relevant backend/core/infrastructure builds and tests pass without relying on a running process that locks output assemblies.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 018_001 | Shared user management table and modal workflows | done | 2026-07-21T13:52:24+07:00 |
| 018_002 | Admin workspace routing and security navigation | done | 2026-07-21T14:25:15+07:00 |
| 018_003 | Fixed-role permission override management | done | 2026-07-23T17:07:35+07:00 |
| 018_004 | Reusable role and permission discovery | done | 2026-07-23T17:11:07+07:00 |
| 018_005 | Cross-layer verification and generated-artifact review | done | 2026-07-23T18:28:56+07:00 |

## Current Task

No active task. Phase 018 is complete with all five tasks verified.

## Completed Notes

- `018_001` established shared DataTable/pagination/row-action/bulk-selection patterns and converted Users to searchable, paginated modal workflows for create, edit, password reset, role assignment, deactivate, and bulk deactivate.
- `018_002` established `/short-links`, `/admin/dashboard`, `/admin/security/users`, and `/admin/security/roles`; separated workspace/admin sidebars; added role-gated admin entry, account identity menus, home navigation, and sign-out.
- `018_003` protected fixed role definitions while adding persisted permission overrides across Core, EF Core, API, authorization, session issuance, and the staged toggle/save UI.
- `018_004` extracted tested role/permission discovery helpers, added explicit loading/no-match states, and brought the latest frontend evidence to 38 passing Bun tests plus a successful production build.
- `018_005` classified source/runtime artifacts and completed isolated cross-layer verification: 44 Core, 31 Infrastructure, 70 API, and 38 frontend tests passed; backend and frontend production builds succeeded.

## Audited Worktree Snapshot

- Startup/bootstrap behavior seeds the Admin system role with every supported permission and assigns the bootstrap `admin` identity.
- Login uses email semantics; a clean startup seeds `admin@shortenlink.local`, and existing bootstrap records migrate by stable ID.
- The system-role catalog now contains only Admin and User; newly managed identities default to User when no explicit role is supplied.
- Creator identity is persisted with each link and returned to the management UI.
- Per-link shares are persisted separately with View/Edit access and enforced by repository scoping plus endpoint authorization.
- All eight EF DbSet entity types derive from `BaseEntity<Guid>` and use UUIDv7 Guid surrogate primary keys; previous natural/composite keys remain unique business constraints.
- DbSet POCOs are flattened directly under `ShortenLink.Core/Domain` as explicit `*Entity` types; Infrastructure owns only EF mapping, provider behavior, and repositories.
- Clean database startup now uses the EF model as the sole schema source; legacy runtime schema-patch SQL was removed.
- Admin receives unrestricted cross-owner access; User access is the union of owned links and explicitly shared links.
- Managed users can be created without a password; the account cannot authenticate until an administrator completes the separate confirmed password flow.
- Shared UI components now include `DataTable`, `Pagination`, `RowActionsMenu`, `RefreshButton`, and `FormDialog`.
- Short Links and Users consume the shared table/action patterns; Short Links defaults to Active and exposes only Active/Deactivated status discovery.
- Roles auto-select the first available role, retain drafts while switching roles, stage individual/group toggles without intermediate confirmation, and persist all dirty roles through one confirmed Save changes boundary.
- The admin dashboard route provides the initial operational summary surface.
- Breadcrumb freezing, sidebar/account dropdown placement, responsive behavior, cursor affordances, and modal consistency were refined across the workspace.

## Verification and Closure State

- Passed: isolated `dotnet build --no-restore --artifacts-path .artifacts/verify` with 0 warnings and 0 errors.
- Passed after the Guid entity/schema update: 45 Core, 35 Infrastructure, and 71 API tests (151 backend tests total).
- Passed: `bun test` with 40 tests.
- Passed: `bun run build` (TypeScript and Vite production build).
- Fresh-database startup recreated the schema, seeded system roles `Admin`/`User`, and assigned the bootstrap `admin` identity to Admin.
- A second clean reset verified the compact permission catalog contains read/create/update/status/delete/import/analytics/audit only; obsolete security, activate, deactivate, and export permissions are absent.
- A clean Guid-schema reset verified health, bootstrap email login, Admin/User seeding, and EF creation without legacy schema patch helpers.
- Runtime closure: the fresh-database verification process was stopped after bootstrap/login/schema checks; no verification API process was intentionally left running.
- Artifact classification: the recreated SQLite database and its transient WAL/SHM state are local runtime artifacts; `.artifacts/verify` is ignored verification output.

## Next Task Proposal

Continue Phase 019 with operational activity/audit visibility built on the verified Admin/User ownership and sharing boundary.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files.

## Scan Rule

Reuse the Admin URLs interaction language and existing security contracts before adding new management primitives.
