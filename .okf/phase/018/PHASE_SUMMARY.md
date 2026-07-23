---
phase: 018
title: Admin Workspace and Security Management
status: active
created_at: 2026-07-21
updated_at: 2026-07-23
current_task: null
task_count: 4
done_count: 4
depends_on:
  - 017
---

# Phase 018 Summary

## Phase Goal

Deliver a coherent authenticated admin workspace for short-link operations, user lifecycle management, and persistent fixed-role permission control.

## Phase Done Criteria

- Public home, Short Links, Admin Dashboard, Users, and Roles have stable routes and context-specific navigation.
- Admin navigation is role-gated; authenticated surfaces expose account identity, workspace navigation, and sign-out.
- Shared DataTable, pagination, refresh, row-action, form-dialog, discovery, and bulk-selection primitives drive list workflows.
- Users support passwordless creation, immutable email during edit, display-name updates, confirmed password reset, role assignment, single deactivate, and bulk deactivate.
- System roles remain fixed; persisted permission overrides are staged in the UI, saved explicitly, and applied consistently during authorization and session issuance.
- Bootstrap startup guarantees an Admin system role with the full permission catalog and an `admin`/`admin` bootstrap identity assigned to it.
- Short Links uses the shared management primitives, modal create/edit flows, bulk actions, and Active/Deactivated discovery.
- Empty, loading, retry, validation, confirmation, mutation feedback, and sticky breadcrumb behavior remain contextual.
- Focused frontend tests and production build pass.
- Relevant backend/core/infrastructure builds and tests pass without relying on a running process that locks output assemblies.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 018_001 | Shared user management table and modal workflows | done | 2026-07-21T13:52:24+07:00 |
| 018_002 | Admin workspace routing and security navigation | done | 2026-07-21T14:25:15+07:00 |
| 018_003 | Fixed-role permission override management | done | 2026-07-23T17:07:35+07:00 |
| 018_004 | Reusable role and permission discovery | done | 2026-07-23T17:11:07+07:00 |

## Current Task

No active task. The four implementation tasks are done, but Phase 018 remains active until cross-layer verification and generated-artifact review close the audited worktree.

## Completed Notes

- `018_001` established shared DataTable/pagination/row-action/bulk-selection patterns and converted Users to searchable, paginated modal workflows for create, edit, password reset, role assignment, deactivate, and bulk deactivate.
- `018_002` established `/short-links`, `/admin/dashboard`, `/admin/security/users`, and `/admin/security/roles`; separated workspace/admin sidebars; added role-gated admin entry, account identity menus, home navigation, and sign-out.
- `018_003` protected fixed role definitions while adding persisted permission overrides across Core, EF Core, API, authorization, session issuance, and the staged toggle/save UI.
- `018_004` extracted tested role/permission discovery helpers, added explicit loading/no-match states, and brought the latest frontend evidence to 38 passing Bun tests plus a successful production build.

## Audited Worktree Snapshot

- Startup/bootstrap behavior seeds the Admin system role with every supported permission and assigns the bootstrap `admin` identity.
- Managed users can be created without a password; the account cannot authenticate until an administrator completes the separate confirmed password flow.
- Shared UI components now include `DataTable`, `Pagination`, `RowActionsMenu`, `RefreshButton`, and `FormDialog`.
- Short Links and Users consume the shared table/action patterns; Short Links defaults to Active and exposes only Active/Deactivated status discovery.
- Roles expose fixed definitions, searchable grouped permissions, staged individual/group toggles, and one confirmed Save changes boundary.
- The admin dashboard route provides the initial operational summary surface.
- Breadcrumb freezing, sidebar/account dropdown placement, responsive behavior, cursor affordances, and modal consistency were refined across the workspace.

## Verification and Closure State

- Passed: `bun test` with 38 tests.
- Passed: `bun run build` (TypeScript and Vite production build).
- Not closed: the relevant .NET test run was blocked by the currently running `ShortenLink.Api`/Visual Studio process locking output DLLs.
- Review before commit: SQLite database/WAL/SHM files and `tsconfig.tsbuildinfo` are dirty generated artifacts and are not treated as verified source deliverables by this summary.

## Next Task Proposal

Create `018_005` for cross-layer backend verification and generated-artifact review, then close Phase 018 only when those checks pass. Do not create it until requested.

## Task Notes

Active and planned task notes live in separate `PPP_TTT-*.md` files.

## Scan Rule

Reuse the Admin URLs interaction language and existing security contracts before adding new management primitives.
