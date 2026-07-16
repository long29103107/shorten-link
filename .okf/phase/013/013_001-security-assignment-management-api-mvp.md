---
task: 013_001
phase: 013
title: Security Assignment Management API MVP
status: done
created_at: 2026-07-16
completed_at: 2026-07-16T22:47:04+07:00
depends_on:
  - 011_001
  - 011_002
  - 011_004
---

# 013_001 - Security Assignment Management API MVP

## Step Goal

Add the first operator-facing backend API for managing persisted security assignments: an authorized admin can list assignments, create or update an assignment for a credential, and disable an assignment without exposing raw API keys or introducing custom roles.

This task should provide the contract that a later React admin security UI can use safely.

## Dependency

- `011_001` defined permission constants and built-in role bundles.
- `011_002` protected admin mutation endpoints.
- `011_004` added persisted credential assignments with hashed key lookup and enabled/disabled state.

## Scope

In:

- Add a stable permission such as `security.assignments.manage`.
- Extend built-in high-trust role bundles to include the management permission where appropriate.
- Add reusable repository support for listing and disabling persisted security assignments.
- Add admin API endpoints for:
  - list assignments
  - create or update assignment from credential key, built-in roles, and explicit permissions
  - disable assignment
- Never return raw API keys from list/detail responses.
- Validate that supplied roles are built-in system roles and supplied permissions are known.
- Protect management endpoints with the new management permission.
- Add tests for list, create/update, disable, validation, `401`, and `403`.
- Document the backend security-management API and its limits.

Out:

- Do not create custom role management.
- Do not add user account lifecycle, invitations, sessions, password login, or profiles.
- Do not add OAuth/OIDC/JWT provider integration.
- Do not build the React UI in this task.
- Do not expose raw API keys after creation/update.
- Do not remove local/demo bootstrap config fallback.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/013/PHASE_SUMMARY.md`
- `.okf/phase/013/013_001-security-assignment-management-api-mvp.md`
- `src\ShortenLink.AspNetCore\ShortenLinkPermissions.cs`
- `src\ShortenLink.AspNetCore\ShortenLinkEndpointRouteBuilderExtensions.cs`
- `src\ShortenLink.Core\Repositories\IShortenLinkSecurityAssignmentRepository.cs`
- `src\ShortenLink.Infrastructure\Repositories\EfCoreShortenLinkSecurityAssignmentRepository.cs`
- `tests\ShortenLink.Api.Tests\`
- `tests\ShortenLink.Infrastructure.Tests\`
- `README.md`

## Acceptance Criteria

- A stable management permission exists for security assignment administration.
- Owner/Admin-style high-trust roles include the management permission; lower-trust roles do not.
- API can list persisted assignments without raw credential keys.
- API can create or update an assignment from a raw credential key by storing only the credential hash.
- API can disable an assignment and disabled credentials are rejected by existing authorization evaluation.
- API rejects unknown system roles and unknown permissions with stable client errors.
- Missing management credentials return stable `401 unauthorized`.
- Valid credentials without management permission return stable `403 forbidden`.
- Tests cover successful and failure paths.
- README documents the API and clarifies that custom roles/user lifecycle remain out of scope.

## Foundation for Next Step

This task should leave a stable security-management backend contract so the next task can build a React admin UI for assignment management without inventing API behavior in the browser.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

Run frontend verification only if frontend files change:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

## Done Notes

Completed on 2026-07-16T22:47:04+07:00.

Implemented:

- Added `security.assignments.manage` as a stable permission for security assignment administration.
- Included the management permission in high-trust built-in role bundles through Owner/All and Admin.
- Extended `IShortenLinkSecurityAssignmentRepository` with list and disable operations.
- Implemented EF Core list and disable support for persisted security assignments.
- Added protected backend endpoints:
  - `GET /api/security/assignments`
  - `PUT /api/security/assignments`
  - `POST /api/security/assignments/{credentialKeyHash}/disable`
- Upsert stores only the hashed credential key and never returns raw API keys in responses.
- Added validation for unknown system roles, unknown permissions, missing assignment name, missing credential key, and malformed credential hashes.
- Protected management endpoints with `security.assignments.manage`, returning stable `401 unauthorized` and `403 forbidden` outcomes through the existing authorization result shape.
- Updated README with the security assignment management API, credential-hash behavior, and scope boundaries.
- Added Infrastructure tests for list and disable behavior.
- Added API tests for upsert/list/no raw key exposure, disable behavior, `401`, `403`, and validation failures.

Verification:

- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --no-build --verbosity minimal` passed with 22 tests.
- `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --no-build --verbosity minimal` passed with 50 tests.
- `dotnet test ShortenLink.slnx --no-build --verbosity minimal` passed with 104 total tests.
- Frontend build was not run because this task did not change frontend files.
