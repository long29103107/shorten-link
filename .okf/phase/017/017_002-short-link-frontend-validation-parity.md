---
task: 017_002
phase: 017
title: Short-Link Frontend Validation Parity And Field Mapping
status: done
created_at: 2026-07-21
completed_at: 2026-07-21T09:19:08+07:00
depends_on:
  - 017_001
---

# 017_002 - Short-Link Frontend Validation Parity And Field Mapping

## Step Goal

Consume the additive API `fieldErrors` contract in short-link create and update forms while centralizing deterministic URL and expiration validation shared by both flows.

## Scope

In:

- Preserve `fieldErrors` through the shared frontend HTTP failure model.
- Add shared short-link form validation matching backend absolute HTTP/HTTPS URL and required future-expiration rules.
- Map backend `originalUrl` and `expiredAtUtc` errors to the corresponding create/admin controls.
- Retain a form-level fallback for unknown validation failures.
- Add focused Bun tests for validation parity and field mapping.

Out:

- Identity and security-management forms.
- New form or validation frameworks.
- Server-only or race-sensitive policy duplication.

## Acceptance Criteria

- Create and admin create/update use one deterministic validation helper.
- API `fieldErrors` survives classification and is available on `ApiError`.
- Known short-link backend fields map to exact controls without parsing error messages or switching on error codes.
- Unknown fields and validation failures remain visible as form-level errors.
- Focused Bun tests and the production frontend build pass.

## Foundation for Next Step

Leaves reusable field-error transport and mapping patterns for login and security-management forms.

## Affected Files

- `.okf/phase/017/PHASE_SUMMARY.md`
- `.okf/phase/017/017_002-short-link-frontend-validation-parity.md`
- `src/ShortenLink.Web/src/shared/api/apiFailure.ts`
- `src/ShortenLink.Web/src/features/short-links/api/http.ts`
- `src/ShortenLink.Web/src/features/short-links/types.ts`
- `src/ShortenLink.Web/src/features/short-links/validation.ts`
- `src/ShortenLink.Web/src/features/short-links/components/CreateShortLinkForm.tsx`
- `src/ShortenLink.Web/src/features/short-links/pages/ShortLinkAdminPage.tsx`
- `src/ShortenLink.Web/test/short-link-validation.test.ts`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Preserved optional API `fieldErrors` through shared failure classification and exposed them on `ApiError` without changing legacy error-code or message handling.
- Centralized deterministic absolute HTTP/HTTPS URL and required future-expiration checks for the public create form and admin create/update editor.
- Mapped backend `originalUrl` and `expiredAtUtc` failures to exact local controls while leaving unknown validation failures at form level.
- Added focused validation, mapping, unknown-field fallback, and transport-preservation tests.
- Verified `bun test` with 23 passing tests and `bun run build` with a successful Vite production build.
