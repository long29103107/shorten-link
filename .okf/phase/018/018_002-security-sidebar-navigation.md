---
task: 018_002
phase: 018
title: Security Sidebar Navigation
status: done
created_at: 2026-07-21
completed_at: 2026-07-21T14:25:15+07:00
depends_on:
  - 018_001
---

# 018_002 - Security Sidebar Navigation

## Step Goal

Move Users, Roles, and Permissions into a persistent Security sidebar group and route each management surface directly.

## Scope

In:

- Add `/security/users`, `/security/roles`, and `/security/permissions` routes.
- Keep `/security` backward-compatible by selecting Users.
- Show Users, Roles, and Permissions as nested sidebar items under Security.
- Rename the generic Security page identity to `Identity & Access`.
- Remove duplicate in-content tab navigation.
- Add focused routing tests.

Out:

- Roles/Permissions table conversion.
- Authorization policy or backend endpoint changes.

## Acceptance Criteria

- All three destinations remain visible in the Security sidebar group for authorized sessions.
- Active child navigation is highlighted independently.
- Direct URLs render the corresponding management content.
- `/security` still opens Users.
- Production frontend build and focused tests pass.

## Foundation for Next Step

Leaves stable direct routes for applying Admin URLs-style list management to Roles and Permissions.

## Affected Files

- `.okf/phase/018/PHASE_SUMMARY.md`
- `.okf/phase/018/018_002-security-sidebar-navigation.md`
- `src/ShortenLink.Web/src/app/App.tsx`
- `src/ShortenLink.Web/src/app/router.ts`
- `src/ShortenLink.Web/src/features/short-links/types.ts`
- `src/ShortenLink.Web/src/features/short-links/pages/SecurityManagementPage.tsx`
- `src/ShortenLink.Web/src/styles.css`
- `src/ShortenLink.Web/test/security-routing.test.ts`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Added persistent Security sidebar grouping with Users, Roles, and Permissions child destinations.
- Added direct `/security/users`, `/security/roles`, and `/security/permissions` routes while keeping `/security` mapped to Users.
- Renamed the generic page identity to `Identity & Access` and removed duplicate content tabs.
- Added independent active-child highlighting and signed-in navigation to Users.
- Verified 32 passing Bun tests and a successful Vite production build.
