---
task: 014_003
phase: 014
title: Role And User Management API MVP
status: done
created_at: 2026-07-17
completed_at: 2026-07-17T15:19:33+07:00
depends_on:
  - 014_001
  - 014_002
---

# 014_003 - Role And User Management API MVP

## Step Goal

Expose admin APIs to manage custom roles and non-bootstrap users while keeping permissions as the authorization source of truth.

This task should make system roles visible as non-deletable built-in bundles, allow custom roles to be created with supported permissions, and allow operators to create or update normal users with assigned roles.

## Dependency

- `014_001` provides the security identity domain and persistence contracts.
- `014_002` provides authenticated admin requests and user-derived permission evaluation.

## Scope

In:

- Add admin endpoints to list system roles and custom roles.
- Add admin endpoints to create, update, enable/disable, or delete custom roles where safe.
- Validate custom-role permissions against the supported permission catalog.
- Add admin endpoints to list, create, update, enable/disable, and assign roles to users.
- Hide the bootstrap admin user from normal user-management lists.
- Protect management endpoints with high-trust permissions such as security/user/role management permissions.
- Add focused API and repository tests for role validation, system-role immutability, user assignment, and hidden bootstrap behavior.

Out:

- Do not build frontend user or role management UI in this task.
- Do not allow system roles to be deleted or edited.
- Do not expose password hashes or raw credentials.
- Do not add invitation emails, password reset, MFA, or public signup.
- Do not add organization or tenant hierarchy.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `src\ShortenLink.Core\Security\`
- `src\ShortenLink.Core\Repositories\`
- `src\ShortenLink.Infrastructure\Repositories\`
- `src\ShortenLink.AspNetCore\`
- `src\ShortenLink.Api\`
- `tests\ShortenLink.Core.Tests\`
- `tests\ShortenLink.Infrastructure.Tests\`
- `tests\ShortenLink.Api.Tests\`
- `README.md`

## Acceptance Criteria

- System roles `Owner`, `Admin`, `Editor`, and `Viewer` are returned as built-in, non-deletable role bundles.
- Custom roles can be created or updated only with known permissions.
- Invalid permissions are rejected with validation errors before persistence.
- Normal users can be created, updated, enabled/disabled, and assigned system and/or custom roles.
- The hidden bootstrap admin user is excluded from normal user-management list responses.
- Role and user management APIs require appropriate security-management permissions.
- Focused tests cover custom-role validation, system-role immutability, hidden bootstrap behavior, and user role assignment.

## Foundation for Next Step

This task should leave stable role and user management APIs so user-owned API keys can be created against real users and authorized by effective role permissions.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Core.Tests\ShortenLink.Core.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
```

## Done Notes

- Added protected role-management APIs to list built-in system role bundles, upsert custom roles, reject invalid permissions, reject system-role mutation, and disable custom roles.
- Added protected user-management APIs to list non-hidden users, create/update normal users with system and/or custom role ids, reject unknown roles, preserve existing password hashes when password is omitted, and disable normal users.
- Kept the hidden bootstrap admin user out of normal user-management list responses and blocked bootstrap updates/disables through user-management APIs.
- Reused the existing high-trust `security.assignments.manage` permission for role and user management endpoints.
- Added repository tests for custom-role disable and normal-user disable behavior.
- Added API tests for system-role list metadata, custom-role validation/disable, normal-user create/list/update/disable, hidden bootstrap behavior, unknown-role validation, and permission enforcement.
