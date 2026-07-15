---
id: 011_003
phase: 011
task: 003
title: Admin UI permission-aware credential flow
status: done
created_at: 2026-07-15
completed_at: 2026-07-15T21:27:51+07:00
owner: codex
type: frontend-api
priority: high
depends_on:
  - 011_001
  - 011_002
tags:
  - security
  - authorization
  - permissions
  - frontend
  - admin
  - phase-11
---

# 011_003 - Admin UI Permission-Aware Credential Flow

## Step Goal

Make the admin UI usable with the permission-based API-key boundary from `011_001` and `011_002`: configure/send a local admin API key, route `401` and `403` outcomes to the standalone status pages, and avoid presenting mutation controls when the current permission set cannot use them.

This task should keep the demo local-friendly while making protected admin behavior intentional and visible.

## Dependency

- `011_001` added permission constants, system-style role bundles, API-key permission evaluation, and frontend `401`/`403` routing.
- `011_002` applied matching permissions to create, update, activate, deactivate, and delete endpoints.
- Phase 010 added standalone `/unauthorized`, `/forbidden`, and `/not-found` status pages.

## Scope

In:

- Add a local/demo way for the frontend to send the configured admin API key, without hard-coding production secrets.
- Ensure admin list failures for `401` and `403` navigate to `/unauthorized` and `/forbidden` cleanly without duplicate error toasts.
- Add a small frontend permission model that mirrors backend permission names.
- Hide or disable create, edit, activate, deactivate, and delete controls when the current frontend permission set lacks the matching permission.
- Keep roles as permission bundles conceptually; do not implement custom role management.
- Document the local frontend credential/permission behavior if needed.

Out:

- OAuth/OIDC/JWT provider integration.
- Database-backed users, API keys, or role assignments.
- Custom roles.
- User management UI.
- Audit log persistence.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_003-admin-ui-permission-aware-credential-flow.md`
- `src\ShortenLink.Web\src\features\short-links\api\http.ts`
- `src\ShortenLink.Web\src\features\short-links\api\shortLinksApi.ts`
- `src\ShortenLink.Web\src\features\short-links\pages\ShortLinkAdminPage.tsx`
- `src\ShortenLink.Web\src\features\short-links\types.ts`
- `src\ShortenLink.Web\src\app\`
- `README.md`

## Acceptance Criteria

- Frontend API requests can include the configured local admin API key header.
- Missing or invalid admin credentials for admin list routes navigate to `/unauthorized`.
- Insufficient permissions navigate to `/forbidden`.
- Create control is unavailable without `short_links.create`.
- Edit/update control is unavailable without `short_links.update`.
- Activate control is unavailable without `short_links.activate`.
- Deactivate control is unavailable without `short_links.deactivate`.
- Delete control is unavailable without `short_links.delete`.
- Local development remains convenient with documented demo credentials or a documented disabled-security mode.
- Frontend build passes.

## Foundation for Next Step

This task should leave the browser-side admin permission experience aligned with the backend permission boundary so later tasks can add persisted system-role assignments or audit logs without redesigning admin UI authorization.

## Verification

Run after implementation:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Run backend verification if endpoint or config behavior changes:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

## Done Notes

- Added Vite environment support for `VITE_SHORTENLINK_ADMIN_API_KEY`, `VITE_SHORTENLINK_ADMIN_API_KEY_HEADER`, `VITE_SHORTENLINK_ADMIN_ROLE`, and `VITE_SHORTENLINK_ADMIN_PERMISSIONS`.
- API requests now include the configured admin API-key header when present.
- Admin controls now use frontend permission bundles that mirror backend permission names and built-in system roles.
- Create, edit, activate, deactivate, and delete actions are unavailable when the configured frontend permission set lacks the matching permission.
- README documents local frontend credential setup and clarifies that backend authorization remains the source of enforcement.
- Verified with `npm run build` in `src\ShortenLink.Web`.
