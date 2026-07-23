---
task: 018_004
phase: 018
title: Reusable role and permission discovery
status: done
created_at: 2026-07-23
completed_at: 2026-07-23T17:11:07+07:00
depends_on:
  - 018_003
---

# 018_004 - Reusable Role and Permission Discovery

## Step Goal

Move role and permission filtering into reusable tested discovery helpers and make the Roles workspace loading/empty behavior explicit.

## Scope

In:

- Extract role-name/id filtering from the Roles page.
- Extract permission group/code/description filtering from the Roles page.
- Add focused tests for matching and no-match behavior.
- Render contextual loading and empty states before the role-permission workspace is available.

Out:

- New routes or backend contracts.
- Role definition CRUD.
- Permission pagination beyond the current compact catalog.

## Acceptance Criteria

- Role filtering is implemented through a reusable pure helper.
- Permission filtering is implemented through a reusable pure helper.
- Helpers have focused tests covering case-insensitive matches and empty results.
- Roles loading and empty states are contextual.
- Existing staged permission toggles remain unchanged.
- Frontend tests and production build pass.

## Foundation for Next Step

Leaves testable discovery contracts that can support pagination or additional security catalogs without page-local filtering logic.

## Affected Files

- `.okf/phase/018/PHASE_SUMMARY.md`
- `.okf/phase/018/018_004-reusable-role-permission-discovery.md`
- `src/ShortenLink.Web/src/features/short-links/securityDiscovery.ts`
- `src/ShortenLink.Web/src/features/short-links/pages/SecurityManagementPage.tsx`
- `src/ShortenLink.Web/test/security-discovery.test.ts`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Extracted reusable, case-insensitive role discovery by name or id.
- Extracted reusable permission discovery by group, permission code, or description while removing empty result groups.
- Replaced page-local filtering with the shared discovery helpers.
- Added contextual initial loading and no-role states to the Roles workspace.
- Added focused discovery tests; verified 38 passing Bun tests and a successful TypeScript/Vite production build.
