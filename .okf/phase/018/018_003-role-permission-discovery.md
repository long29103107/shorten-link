---
task: 018_003
phase: 018
title: Role permission discovery
status: done
created_at: 2026-07-23
completed_at: 2026-07-23T17:07:35+07:00
depends_on:
  - 018_002
---

# 018_003 - Role Permission Discovery

## Step Goal

Make the fixed-role permission workspace easier to navigate while preserving staged, explicit permission updates.

## Scope

In:

- Keep role definitions read-only and remove role create, edit, and delete entry points.
- Add role and permission search within the Roles workspace.
- Preserve per-permission and permission-group toggles.
- Keep changes staged until the user confirms Save changes.
- Show contextual empty states when discovery filters have no matches.

Out:

- Creating, renaming, deleting, enabling, or disabling role definitions.
- New backend role or permission contracts.
- User-role assignment changes.

## Acceptance Criteria

- Administrators can filter the fixed role list by name.
- Administrators can filter permissions by name, code, or description.
- Permission toggles continue to stage changes without sending a request immediately.
- Save changes remains the only permission mutation boundary.
- Role CRUD controls and dialogs are not rendered.
- Focused frontend tests and the production build pass.

## Foundation for Next Step

Leaves a stable searchable permission workspace for the final permission-catalog consistency task.

## Affected Files

- `.okf/phase/018/PHASE_SUMMARY.md`
- `.okf/phase/018/018_003-role-permission-discovery.md`
- `src/ShortenLink.Web/src/features/short-links/pages/SecurityManagementPage.tsx`
- `src/ShortenLink.Web/src/styles.css`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Removed role create, edit, delete controls and their rendered dialogs so role definitions remain fixed.
- Added role discovery by name or id and permission discovery by group, code, or description.
- Added contextual no-match messages for both discovery surfaces.
- Preserved staged individual/group permission toggles and the explicit Save changes confirmation boundary.
- Verified 36 passing Bun tests and a successful TypeScript/Vite production build.
