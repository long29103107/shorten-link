---
id: 011_002
phase: 011
task: 002
title: Apply permissions to admin mutation endpoints
status: done
created_at: 2026-07-15
completed_at: 2026-07-15T20:58:07+07:00
owner: codex
type: security
priority: high
depends_on:
  - 011_001
tags:
  - security
  - authorization
  - permissions
  - admin
  - phase-11
---

# 011_002 - Apply Permissions To Admin Mutation Endpoints

## Step Goal

Apply the permission boundary from `011_001` across admin mutation endpoints so create, update, activate, deactivate, and delete require the matching permission when security is enabled.

## Dependency

- `011_001` added permission constants, role bundles, API-key permission evaluation, and protected the admin list endpoint with `short_links.read`.

## Scope

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

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_002-apply-permissions-to-admin-mutation-endpoints.md`
- `src\ShortenLink.AspNetCore\ShortenLinkEndpointRouteBuilderExtensions.cs`
- `tests\ShortenLink.Api.Tests\ShortLinkEndpointsTests.cs`

## Acceptance Criteria

- Create requires `short_links.create` when security is enabled.
- Update requires `short_links.update` when security is enabled.
- Activate requires `short_links.activate` when security is enabled.
- Deactivate requires `short_links.deactivate` when security is enabled.
- Delete requires `short_links.delete` when security is enabled.
- Missing credentials return stable `401 unauthorized` before mutation behavior.
- Valid credentials without required permission return stable `403 forbidden` before mutation behavior.
- Existing local behavior remains unchanged when security is disabled.
- Tests cover protected mutation endpoint behavior.

## Foundation for Next Step

This task should leave the backend admin endpoint surface consistently protected so later tasks can add permission-aware UI controls, audit logs, or stronger identity integration without redesigning endpoint authorization.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

## Done Notes

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
