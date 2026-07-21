---
task: 017_004
phase: 017
title: Security Identity Tab Workflow And Validation Parity
status: done
created_at: 2026-07-21
completed_at: 2026-07-21T13:30:55+07:00
depends_on:
  - 017_003
---

# 017_004 - Security Identity Tab Workflow And Validation Parity

## Step Goal

Restructure Security into Users, Roles, and Permissions tabs, with minimal user creation and separate password-reset and role-assignment workflows.

## Scope

In:

- Limit managed-user creation inputs to email, display name, and password; generate the internal user id and default state outside the form.
- Move password reset and role assignment into separate sections for an existing selected user.
- Present Security as exactly three primary tabs: Users, Roles, and Permissions.
- Keep custom-role management under Roles and present the supported permission catalog under Permissions.
- Centralize deterministic validation for managed-user creation, password reset, role assignment, and custom-role forms.
- Map backend `username`, `displayName`, `password`, `roleIds`, `id`, `name`, and `permissions` metadata to the matching controls or field groups.
- Clear a field error when that field is edited without discarding unrelated errors.
- Keep conflicts, immutable-system-role failures, authorization failures, and unknown fields visible through a safe form-level fallback.
- Preserve retryable mutation values.
- Add focused Bun tests for validation parity, API mapping, unknown-field fallback, and compatibility.

Out:

- Public user registration; user creation remains protected by `security.assignments.manage`.
- Backend validation contract changes.
- New form/validation frameworks.
- Changes to role or permission semantics.
- Removal of API-key or legacy-assignment backend APIs; they are only removed from the primary Security tab group.

## Acceptance Criteria

- User creation exposes only email, display name, and password inputs.
- Reset password and assign roles are separate selected-user sections.
- Security exposes Users, Roles, and Permissions as its primary tab group.
- Empty or invalid identity/custom-role values are rejected at their exact controls or field groups.
- Known user, role, and permission failures map without parsing error messages or switching on error codes.
- Unknown and non-field failures remain visible at form level.
- Retryable failures preserve current form values.
- Existing login and managed-user behavior remains unchanged.
- Focused Bun tests and the production frontend build pass.

## Foundation for Next Step

Leaves a focused identity-management workflow with field-aware validation so phase 017 can be evaluated for closure.

## Affected Files

- `.okf/phase/017/PHASE_SUMMARY.md`
- `.okf/phase/017/017_004-remaining-security-form-validation-parity.md`
- `src/ShortenLink.Web/src/features/short-links/securityValidation.ts`
- `src/ShortenLink.Web/src/features/short-links/identityValidation.ts`
- `src/ShortenLink.Web/src/features/short-links/pages/SecurityManagementPage.tsx`
- `src/ShortenLink.Web/test/security-validation.test.ts`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Replaced the Security primary tab group with exactly Users, Roles, and Permissions.
- Reduced protected user creation to email, display name, and password while generating the internal id and default state outside the form.
- Added selected-user sections for password reset and role assignment, keeping those mutations separate from registration.
- Kept custom-role management under Roles and added a read-only supported permission catalog under Permissions.
- Added deterministic email, password-reset, managed-user, custom-role, and role/permission field mapping with safe form-level fallbacks.
- Removed personal API keys and legacy assignments from the primary Security tabs without removing their backend APIs.
- Verified 29 passing Bun tests and a successful Vite production build.
- Browser visual verification was skipped because the in-app browser could not reach the local Vite server in this environment.
