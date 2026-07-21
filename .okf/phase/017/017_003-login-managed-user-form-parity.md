---
task: 017_003
phase: 017
title: Login And Managed User Form Parity
status: done
created_at: 2026-07-21
completed_at: 2026-07-21T10:15:00+07:00
depends_on:
  - 017_002
---

# 017_003 - Login And Managed User Form Parity

## Step Goal

Present login and admin-managed user registration as explicit card forms and map deterministic client/API validation failures to their exact controls.

## Scope

In:

- Keep login in the same centered card shell used by the 401 status page.
- Present admin-managed user creation/update as a structured card form within the protected Security workspace.
- Add shared deterministic required-field validation for login and managed users.
- Map API `username`, `password`, `id`, `displayName`, and `roleIds` metadata to exact controls or field groups.
- Preserve safe form-level fallback behavior for invalid credentials, conflicts without field metadata, and unknown failures.
- Add focused Bun tests.

Out:

- Public self-registration; the current security model keeps user creation behind `security.assignments.manage`.
- Role, personal API-key, and assignment form mapping; these remain for the next task.
- New form frameworks or backend contract changes.

## Acceptance Criteria

- Login and managed-user registration/update use card-based form structure consistent with the 401 page.
- Empty login and managed-user values are rejected at the matching controls.
- Creating a managed user requires a password; updating one allows the password to remain empty.
- Known API fields map without parsing messages or switching on error codes.
- Unknown or credential-level login failures remain visible at form level.
- Backend array-valued `fieldErrors` are normalized safely for controls.
- Focused Bun tests and the production frontend build pass.

## Foundation for Next Step

Leaves reusable identity validation and field-mapping helpers for the remaining role, API-key, and assignment forms.

## Affected Files

- `.okf/phase/017/PHASE_SUMMARY.md`
- `.okf/phase/017/017_003-login-managed-user-form-parity.md`
- `src/ShortenLink.Web/src/shared/api/apiFailure.ts`
- `src/ShortenLink.Web/src/features/short-links/identityValidation.ts`
- `src/ShortenLink.Web/src/features/short-links/pages/LoginPage.tsx`
- `src/ShortenLink.Web/src/features/short-links/pages/SecurityManagementPage.tsx`
- `src/ShortenLink.Web/src/styles.css`
- `src/ShortenLink.Web/test/identity-validation.test.ts`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Kept login in the centered status shell and added exact username/password client and API field-error rendering.
- Reworked the protected managed-user create/update UI as a structured card form consistent with the 401/login card language; public signup remains out of scope.
- Added create-versus-update password parity, exact `id`, `username`, `displayName`, `password`, and `roleIds` mappings, and safe form-level fallback behavior.
- Normalized the backend array-valued `fieldErrors` payload into the frontend control-message model.
- Verified 27 passing Bun tests and a successful Vite production build.
