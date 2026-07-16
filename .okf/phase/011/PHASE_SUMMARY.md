---
phase: 011
title: Security And Permission-Based Admin Protection
status: complete
created_at: 2026-07-15
updated_at: 2026-07-16
current_task: null
task_count: 4
done_count: 4
depends_on:
  - 010
---

# Phase 011 Summary

## Phase Goal

Add a small, production-aware security foundation for the demo admin surface using permission-based authorization grouped through roles, without turning the product into a full identity platform.

Phase 011 should protect admin management actions, define reusable permission contracts, and leave a clean path for future user/session/JWT integrations.

## Phase Done Criteria

- Admin UI and admin API mutations are protected by an explicit authorization boundary.
- Permissions are modeled as first-class constants or policies and can be grouped by role.
- Default local/demo configuration remains easy to run without external identity infrastructure.
- Unauthorized and forbidden outcomes reuse the Phase 010 status experience.
- Tests cover protected endpoint behavior and permission evaluation.
- Documentation explains the local security model and how to configure it.

## Scope

In:

- Permission catalog for short-link admin actions.
- Role-to-permission grouping for default roles such as Owner, Admin, Editor, and Viewer.
- Minimal local/demo authentication or API-key/session boundary if needed to exercise permissions.
- ASP.NET Core authorization policies for admin endpoints.
- Frontend handling for `401` and `403` outcomes.
- Tests for permission evaluation and endpoint protection.

Out:

- OAuth/OIDC provider integration.
- Multi-tenant user management.
- Billing, invitations, password reset, or account lifecycle.
- Public SaaS security administration.
- Replacing the existing short-link core contracts unless a security boundary requires a narrow extension.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 011_001 | Permission catalog and admin authorization foundation | done | 2026-07-15T20:47:00+07:00 |
| 011_002 | Apply permissions to admin mutation endpoints | done | 2026-07-15T20:58:07+07:00 |
| 011_003 | Admin UI permission-aware credential flow | done | 2026-07-15T21:27:51+07:00 |
| 011_004 | Persist system role security assignments | done | 2026-07-16T22:13:57+07:00 |

## Current Task

No active task. Phase 011 is complete.

## Completed Notes

- `011_001` established permission constants, role bundles, API-key permission evaluation, a protected admin list endpoint, frontend `401`/`403` routing, README security docs, and tests for role bundles plus protected endpoint outcomes.
- `011_002` applied matching permissions to create, update, activate, deactivate, and delete endpoints, with tests for `401` and `403` mutation behavior.
- `011_003` added frontend admin API-key header configuration, frontend permission bundles for Owner/Admin/Editor/Viewer, permission-aware admin mutation controls, and README documentation for local credential setup.
- `011_004` added durable API-key security assignments with hashed credential lookup, enabled/disabled assignment state, built-in system role expansion, config fallback for local bootstrap credentials, persistence/schema tests, endpoint authorization coverage, and README security documentation.

## Next Task Proposal

Phase 011 is complete. Opened Phase 012 for admin analytics insights, starting with `012_001 - Admin analytics summary API MVP`.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 011_001 - Permission Catalog And Admin Authorization Foundation

Source before compaction: `011_001-permission-catalog-and-admin-authorization-foundation.md`

#### Step Goal

Create the first reusable security slice for admin protection by defining a permission catalog, role-to-permission bundles, and a minimal authorization boundary that future admin endpoints and UI checks can reuse.

This step should establish permission-based authorization, not pure role-based authorization.

#### Dependency

- Phase 010 added reusable `401`, `403`, and `404` status experiences that security flows can reuse.
- Existing admin endpoints already support create, list, update, activate, deactivate, delete, and bulk-style UI operations.
- `PRODUCT_VISION.md` identifies admin protection as a P0 product gap.

#### Scope

In:

- Define short-link/admin permissions such as `short_links.read`, `short_links.create`, `short_links.update`, `short_links.activate`, `short_links.deactivate`, `short_links.delete`, `short_links.export`, `analytics.read`, and `audit_logs.read`.
- Define default role bundles such as Owner, Admin, Editor, and Viewer.
- Add an authorization evaluation service or policy layer that checks permissions, while roles remain only permission bundles.
- Protect the smallest meaningful admin API surface needed to prove the boundary.
- Return stable `401` or `403` outcomes that the frontend can route to Phase 010 status pages.
- Add focused tests for permission mapping and protected endpoint behavior.
- Document local/demo security behavior.

Out:

- OAuth/OIDC/JWT provider integration.
- Database-backed users or role assignment UI.
- Password login, account creation, invitations, or user lifecycle.
- Multi-tenant isolation.
- Large audit-log or analytics UI work.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_001-permission-catalog-and-admin-authorization-foundation.md`
- `src\ShortenLink.AspNetCore\`
- `src\ShortenLink.Api\`
- `src\ShortenLink.Web\src\features\short-links\api\`
- `src\ShortenLink.Web\src\app\`
- `tests\ShortenLink.Api.Tests\`

#### Acceptance Criteria

- Permission constants or equivalent stable permission identifiers exist for admin short-link actions.
- Default roles are expressed as bundles of permissions, not as hard-coded role checks in business logic.
- At least one admin management endpoint requires an appropriate permission.
- Missing credentials produce a stable `401` response.
- Valid credentials without the required permission produce a stable `403` response.
- Frontend API handling can navigate or surface `401` and `403` using the Phase 010 status pages.
- Local development remains runnable with documented demo credentials or a documented security-disabled mode.
- Tests cover permission bundle evaluation and endpoint `401`/`403` behavior.

#### Foundation for Next Step

This task should leave reusable permission identifiers, role bundles, and endpoint policy wiring that later tasks can apply across all admin endpoints and admin UI controls without redesigning authorization.

#### Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

Run frontend verification if API handling or routing changes:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

#### Done Notes

- Added `ShortenLinkPermissions` constants and `ShortenLinkRoles` permission bundles for Owner, Admin, Editor, and Viewer.
- Added API-key based `IShortenLinkAuthorizationService` that evaluates permissions, with roles acting only as permission bundles.
- Added `ShortenLink:Security` options with local-demo defaults and validation when security is enabled.
- Protected `GET /api/short-links` with `short_links.read` when security is enabled.
- Added stable JSON `401 unauthorized` and `403 forbidden` responses for missing or under-permissioned API keys.
- Updated frontend fetch handling so `401` routes to `/unauthorized` and `403` routes to `/forbidden`.
- Documented the local permission-based security model in README.
- Verification:
  - `npm run build` passed in `src/ShortenLink.Web`.
  - `dotnet build ShortenLink.slnx --verbosity minimal` passed.
  - `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal` passed.
  - `dotnet test ShortenLink.slnx --verbosity minimal` passed.

### 011_002 - Apply Permissions To Admin Mutation Endpoints

Source before compaction: `011_002-apply-permissions-to-admin-mutation-endpoints.md`

#### Step Goal

Apply the permission boundary from `011_001` across admin mutation endpoints so create, update, activate, deactivate, and delete require the matching permission when security is enabled.

#### Dependency

- `011_001` added permission constants, role bundles, API-key permission evaluation, and protected the admin list endpoint with `short_links.read`.

#### Scope

In:

- Require `short_links.create` for `POST /api/short-links`.
- Require `short_links.update` for `PUT /api/short-links/{code}`.
- Require `short_links.activate` for `POST /api/short-links/{code}/activate`.
- Require `short_links.deactivate` for `POST /api/short-links/{code}/deactivate`.
- Require `short_links.delete` for `DELETE /api/short-links/{code}`.
- Ensure authorization runs before mutation service behavior.
- Add tests for `401` and `403` behavior across mutation endpoints.

Out:

- OAuth/OIDC/JWT provider integration.
- Admin user management UI.
- Hiding frontend buttons by permission.
- Audit log persistence.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_002-apply-permissions-to-admin-mutation-endpoints.md`
- `src\ShortenLink.AspNetCore\ShortenLinkEndpointRouteBuilderExtensions.cs`
- `tests\ShortenLink.Api.Tests\ShortLinkEndpointsTests.cs`

#### Acceptance Criteria

- Create requires `short_links.create` when security is enabled.
- Update requires `short_links.update` when security is enabled.
- Activate requires `short_links.activate` when security is enabled.
- Deactivate requires `short_links.deactivate` when security is enabled.
- Delete requires `short_links.delete` when security is enabled.
- Missing credentials return stable `401 unauthorized` before mutation behavior.
- Valid credentials without required permission return stable `403 forbidden` before mutation behavior.
- Existing local behavior remains unchanged when security is disabled.
- Tests cover protected mutation endpoint behavior.

#### Foundation for Next Step

This task should leave the backend admin endpoint surface consistently protected so later tasks can add permission-aware UI controls, audit logs, or stronger identity integration without redesigning endpoint authorization.

#### Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

#### Done Notes

- Applied `short_links.create` to `POST /api/short-links`.
- Applied `short_links.update` to `PUT /api/short-links/{code}`.
- Applied `short_links.activate` to `POST /api/short-links/{code}/activate`.
- Applied `short_links.deactivate` to `POST /api/short-links/{code}/deactivate`.
- Applied `short_links.delete` to `DELETE /api/short-links/{code}`.
- Added endpoint tests proving missing credentials return `401 unauthorized` before mutation behavior.
- Added endpoint tests proving valid credentials without mutation permissions return `403 forbidden` before mutation behavior.
- Verification:
  - `dotnet build ShortenLink.slnx --verbosity minimal` passed.
  - `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal` passed.
  - `dotnet test ShortenLink.slnx --verbosity minimal` passed.

### 011_003 - Admin UI Permission-Aware Credential Flow

Source before compaction: `011_003-admin-ui-permission-aware-credential-flow.md`

#### Step Goal

Make the admin UI usable with the permission-based API-key boundary from `011_001` and `011_002`: configure/send a local admin API key, route `401` and `403` outcomes to the standalone status pages, and avoid presenting mutation controls when the current permission set cannot use them.

This task should keep the demo local-friendly while making protected admin behavior intentional and visible.

#### Dependency

- `011_001` added permission constants, system-style role bundles, API-key permission evaluation, and frontend `401`/`403` routing.
- `011_002` applied matching permissions to create, update, activate, deactivate, and delete endpoints.
- Phase 010 added standalone `/unauthorized`, `/forbidden`, and `/not-found` status pages.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_003-admin-ui-permission-aware-credential-flow.md`
- `src\ShortenLink.Web\src\features\short-links\api\http.ts`
- `src\ShortenLink.Web\src\features\short-links\api\shortLinksApi.ts`
- `src\ShortenLink.Web\src\features\short-links\pages\ShortLinkAdminPage.tsx`
- `src\ShortenLink.Web\src\features\short-links\types.ts`
- `src\ShortenLink.Web\src\app\`
- `README.md`

#### Acceptance Criteria

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

#### Foundation for Next Step

This task should leave the browser-side admin permission experience aligned with the backend permission boundary so later tasks can add persisted system-role assignments or audit logs without redesigning admin UI authorization.

#### Verification

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

#### Done Notes

- Added Vite environment support for `VITE_SHORTENLINK_ADMIN_API_KEY`, `VITE_SHORTENLINK_ADMIN_API_KEY_HEADER`, `VITE_SHORTENLINK_ADMIN_ROLE`, and `VITE_SHORTENLINK_ADMIN_PERMISSIONS`.
- API requests now include the configured admin API-key header when present.
- Admin controls now use frontend permission bundles that mirror backend permission names and built-in system roles.
- Create, edit, activate, deactivate, and delete actions are unavailable when the configured frontend permission set lacks the matching permission.
- README documents local frontend credential setup and clarifies that backend authorization remains the source of enforcement.
- Verified with `npm run build` in `src\ShortenLink.Web`.

## Scan Rule
Read this file before working on any `011_*` task note. Keep implementation permission-based, with roles acting as permission bundles rather than hard-coded authorization logic.
