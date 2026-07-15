---
id: 011_001
phase: 011
task: 001
title: Permission catalog and admin authorization foundation
status: planned
created_at: 2026-07-15
completed_at:
owner: codex
type: security
priority: high
depends_on:
  - 010
tags:
  - security
  - authorization
  - permissions
  - admin
  - phase-11
---

# 011_001 - Permission Catalog And Admin Authorization Foundation

## Step Goal

Create the first reusable security slice for admin protection by defining a permission catalog, role-to-permission bundles, and a minimal authorization boundary that future admin endpoints and UI checks can reuse.

This step should establish permission-based authorization, not pure role-based authorization.

## Dependency

- Phase 010 added reusable `401`, `403`, and `404` status experiences that security flows can reuse.
- Existing admin endpoints already support create, list, update, activate, deactivate, delete, and bulk-style UI operations.
- `PRODUCT_VISION.md` identifies admin protection as a P0 product gap.

## Scope

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

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_001-permission-catalog-and-admin-authorization-foundation.md`
- `src\ShortenLink.AspNetCore\`
- `src\ShortenLink.Api\`
- `src\ShortenLink.Web\src\features\short-links\api\`
- `src\ShortenLink.Web\src\app\`
- `tests\ShortenLink.Api.Tests\`

## Acceptance Criteria

- Permission constants or equivalent stable permission identifiers exist for admin short-link actions.
- Default roles are expressed as bundles of permissions, not as hard-coded role checks in business logic.
- At least one admin management endpoint requires an appropriate permission.
- Missing credentials produce a stable `401` response.
- Valid credentials without the required permission produce a stable `403` response.
- Frontend API handling can navigate or surface `401` and `403` using the Phase 010 status pages.
- Local development remains runnable with documented demo credentials or a documented security-disabled mode.
- Tests cover permission bundle evaluation and endpoint `401`/`403` behavior.

## Foundation for Next Step

This task should leave reusable permission identifiers, role bundles, and endpoint policy wiring that later tasks can apply across all admin endpoints and admin UI controls without redesigning authorization.

## Verification

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

## Done Notes

Not started.
