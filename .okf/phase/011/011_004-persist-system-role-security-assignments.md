---
id: 011_004
phase: 011
task: 004
title: Persist system role security assignments
status: planned
created_at: 2026-07-15
completed_at:
owner: codex
type: backend-security
priority: medium
depends_on:
  - 011_001
  - 011_002
  - 011_003
tags:
  - security
  - authorization
  - permissions
  - system-roles
  - persistence
  - phase-11
---

# 011_004 - Persist System Role Security Assignments

## Step Goal

Add a durable security-assignment foundation for the local/admin API-key model without introducing custom roles or full user management: keep roles as built-in system bundles, persist which credential receives which system role or explicit permission set, and keep backend permission evaluation as the single authorization source.

This task should turn the current config-only security model into a small production-aware persistence boundary while preserving the demo-friendly setup.

## Dependency

- `011_001` introduced permission constants, role bundles, config-backed API-key authorization, and protected admin list behavior.
- `011_002` applied permissions to admin mutation endpoints.
- `011_003` should make the admin UI credential-aware before this task persists role assignments.

## Scope

In:

- Model built-in system roles such as Owner, Admin, Editor, and Viewer as non-deletable role bundles.
- Persist API-key or local credential assignments to system roles and/or explicit permissions.
- Ensure authorization evaluation can resolve permissions from persisted assignments.
- Keep config-backed demo credentials as a bootstrap or fallback path for local development.
- Add migration/schema coverage if the existing persistence layer requires it.
- Add tests for persisted assignment evaluation, unknown credentials, disabled credentials, and role-to-permission expansion.
- Document how system roles differ from custom roles and why custom role management is intentionally out of scope.

Out:

- Custom role creation/editing/deletion.
- User account lifecycle, invitations, password reset, or profile management.
- OAuth/OIDC/JWT integration.
- Admin UI for managing users or roles.
- Multi-tenant organization security.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/011/PHASE_SUMMARY.md`
- `.okf/phase/011/011_004-persist-system-role-security-assignments.md`
- `src\ShortenLink.AspNetCore\ShortenLinkAuthorizationService.cs`
- `src\ShortenLink.AspNetCore\ShortenLinkPermissions.cs`
- `src\ShortenLink.Infrastructure\`
- `src\ShortenLink.Core\`
- `src\ShortenLink.Api\appsettings.json`
- `tests\ShortenLink.Api.Tests\`
- `tests\ShortenLink.Infrastructure.Tests\`
- `README.md`

## Acceptance Criteria

- Built-in system roles are represented as stable role bundles and cannot be customized through this task.
- Persisted security assignments can grant a credential one or more system roles.
- Permission evaluation expands persisted system roles into effective permissions.
- Missing, disabled, or unknown credentials are rejected consistently.
- Local development can still use documented bootstrap/demo credentials.
- Tests cover persisted role assignment success and failure paths.
- Documentation explains that permissions are the source of truth and system roles are predefined bundles.

## Foundation for Next Step

This task should leave a durable security-assignment boundary that can later support audit logs, admin settings UI, or external identity integration without rewriting permission evaluation.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

Run frontend verification too if any admin credential UI changes are needed:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

## Done Notes

Not started.
