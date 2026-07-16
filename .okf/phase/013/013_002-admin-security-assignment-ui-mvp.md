---
task: 013_002
phase: 013
title: Admin Security Assignment UI MVP
status: done
created_at: 2026-07-16
completed_at: 2026-07-16T22:59:02+07:00
depends_on:
  - 013_001
  - 011_003
  - 010_001
---

# 013_002 - Admin Security Assignment UI MVP

## Step Goal

Add a React admin surface for managing persisted security assignments using the backend API from `013_001`: operators with `security.assignments.manage` can list assignments, create or update an assignment, and disable an assignment without exposing raw API keys.

This task should make the existing system-role security model manageable from the browser while keeping custom roles and full user lifecycle out of scope.

## Dependency

- `013_001` added protected APIs for listing, upserting, and disabling persisted security assignments.
- `011_003` added frontend API-key configuration and permission-aware control behavior.
- Phase 010 added `/unauthorized` and `/forbidden` status pages for protected API outcomes.

## Scope

In:

- Add frontend API types/client functions for security assignments.
- Add a permission-aware admin UI entry point for security assignment management.
- Display persisted assignments without raw API keys.
- Add create/update form for assignment name, credential key, built-in roles, explicit permissions, and enabled state.
- Add disable action with confirmation.
- Route `401` and `403` outcomes through existing status-page handling.
- Hide or disable the security-management UI when frontend permissions lack `security.assignments.manage`.
- Keep built-in roles non-customizable and permissions source-of-truth.

Out:

- Do not create custom roles.
- Do not add user accounts, invitations, password reset, profile management, sessions, OAuth/OIDC, or JWT provider integration.
- Do not display raw API keys after submission.
- Do not change backend contracts unless the UI exposes a verified mismatch.
- Do not add new frontend dependencies unless strictly necessary.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/013/PHASE_SUMMARY.md`
- `.okf/phase/013/013_002-admin-security-assignment-ui-mvp.md`
- `src\ShortenLink.Web\src\features\short-links\api\adminSecurity.ts`
- `src\ShortenLink.Web\src\features\short-links\api\shortLinksApi.ts`
- `src\ShortenLink.Web\src\features\short-links\types.ts`
- `src\ShortenLink.Web\src\features\short-links\pages\ShortLinkAdminPage.tsx`
- `src\ShortenLink.Web\src\features\short-links\components\`
- `src\ShortenLink.Web\src\styles.css`
- `README.md` only if frontend setup guidance changes

## Acceptance Criteria

- Frontend can list security assignments through `GET /api/security/assignments`.
- Admin UI shows assignment name, credential hash, built-in roles, explicit permissions, enabled state, and created timestamp without raw API keys.
- Admin UI can create or update an assignment using a raw credential key input that is not displayed after submission.
- Admin UI can disable an assignment after confirmation.
- Unknown roles/permissions or validation failures are shown as user-friendly errors.
- Security-management UI is unavailable without `security.assignments.manage` in configured frontend permissions.
- API `401` navigates to `/unauthorized`; `403` navigates to `/forbidden`.
- Existing short-link admin behavior remains intact.
- Frontend build passes.

## Foundation for Next Step

This task should leave a browser-manageable security assignment workflow that later work can extend with audit logs, safer key rotation affordances, or external identity integration without redesigning the basic assignment UI.

## Verification

Run after implementation:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Run backend verification only if backend contracts change:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

## Done Notes

Completed on 2026-07-16T22:59:02+07:00.

Implemented:

- Added frontend security assignment response/request types.
- Added API client functions for listing, upserting, and disabling security assignments.
- Added `canManageSecurityAssignments` to frontend permission state using `security.assignments.manage`.
- Added a permission-aware Security action in the admin page header.
- Added a security assignment management dialog with list, create/update form, built-in role checkboxes, explicit permission checkboxes, enabled toggle, refresh, edit, and disable actions.
- The assignment list shows name, credential hash, roles, explicit permissions, enabled state, and created timestamp without exposing raw API keys.
- Raw credential keys are accepted only in the create/update form and are cleared after save.
- Disable uses the existing confirmation dialog and updates local UI state.
- Existing `fetchJson` behavior continues routing `401` and `403` responses to `/unauthorized` and `/forbidden`.
- Kept existing short-link admin, analytics, copy, edit, activate/deactivate, delete, bulk, and pagination behavior intact.

Verification:

- `npm run build` passed in `src\ShortenLink.Web`.
- Backend verification was not rerun because this task only changed frontend files and consumed the already verified `013_001` backend contract.
