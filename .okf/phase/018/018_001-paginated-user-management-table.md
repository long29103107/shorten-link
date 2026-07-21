---
task: 018_001
phase: 018
title: Paginated User Management Table
status: done
created_at: 2026-07-21
completed_at: 2026-07-21T13:52:24+07:00
depends_on:
  - 017_004
---

# 018_001 - Paginated User Management Table

## Step Goal

Replace the Users card grid with an Admin URLs-style searchable, filterable, sortable, paginated table and row-driven management actions.

## Scope

In:

- Users toolbar with search, status filter, sort field, and direction.
- Client pagination over the existing protected user list contract.
- Table columns for email, display name, roles, created date, status, and actions.
- Create action plus row actions for manage/update, password reset, role assignment, and deactivate.
- Reuse existing validation, recovery, and mutation behavior.
- Focused tests for discovery and pagination helpers.

Out:

- Backend list-query contract changes.
- Hard delete or reactivation endpoints not supported by the current API.
- Roles and Permissions table conversion; later tasks build on this foundation.

## Acceptance Criteria

- Users visually follow Admin URLs table and pagination conventions.
- Search/filter/sort reset pagination and produce deterministic rows.
- Page size and numbered navigation work for filtered results.
- Create and manage forms are separate from the table surface.
- Existing password reset, role assignment, and deactivate behavior remains reachable.
- Focused Bun tests and production frontend build pass.

## Foundation for Next Step

Leaves reusable Security discovery and pagination helpers/styles for Roles and Permissions.

## Affected Files

- `.okf/phase/018/PHASE_SUMMARY.md`
- `.okf/phase/018/018_001-paginated-user-management-table.md`
- `src/ShortenLink.Web/src/features/short-links/securityDiscovery.ts`
- `src/ShortenLink.Web/src/features/short-links/pages/SecurityManagementPage.tsx`
- `src/ShortenLink.Web/src/styles.css`
- `src/ShortenLink.Web/test/security-discovery.test.ts`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Replaced the Users card list with an Admin URLs-style header, Create action, discovery toolbar, table, status/actions column, and sticky pagination footer.
- Added deterministic client search by email/display name, enabled/disabled filtering, email/display-name/created sorting, direction, page size, and compact numbered navigation.
- Kept registration separate from the list and retained row-driven profile update, password reset, role assignment, and deactivate workflows.
- Added focused discovery/pagination tests.
- Verified 31 passing Bun tests and a successful Vite production build.
