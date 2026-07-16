---
task: 014_001
phase: 014
title: Security Identity Domain And Persistence Foundation
status: active
created_at: 2026-07-16
depends_on:
  - 013_001
  - 013_002
---

# 014_001 - Security Identity Domain And Persistence Foundation

## Step Goal

Introduce the backend domain and persistence foundation for user and role based security.

This task should define users, system roles, custom roles, permissions, and user-owned API keys without building the full login UI yet. It should replace the current mental model of "security assignment = credential plus roles" with "user = principal, role = permission bundle, API key = credential owned by a user".

## Dependency

- Phase 013 added persisted security assignments and `security.assignments.manage`.
- The user clarified the desired security model:
  - There are system roles and custom roles.
  - Each role is a bundle of permissions.
  - There is a default admin user with username `admin` and password `admin`.
  - The default admin user should be hidden from user-management UI.
  - New users can be created.
  - Logged-in users can create API keys for API access.

## Scope

In:

- Add core domain models for security users, roles, permissions, and API keys.
- Represent system roles as built-in non-deletable role bundles.
- Represent custom roles as persisted role bundles with supported permissions only.
- Add persistence records and repositories for users, roles, and user-owned API keys.
- Seed or ensure the hidden bootstrap admin user as username `admin` with password `admin` for local/demo setup.
- Hash passwords and API keys; never store raw secrets.
- Define stable backend contracts that later login and UI tasks can consume.
- Keep existing permission strings as the authorization source of truth.
- Add tests for role bundle behavior, custom-role permission validation, hidden bootstrap admin seed, and user-owned API key persistence.

Out:

- Do not build the login page in this task.
- Do not build user-management UI in this task.
- Do not implement OAuth/OIDC.
- Do not add password reset, invitation email, MFA, or public signup.
- Do not expose raw API keys after the creation response.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/014/PHASE_SUMMARY.md`
- `.okf/phase/014/014_001-security-identity-domain-persistence-foundation.md`
- `src\ShortenLink.Core\Security\`
- `src\ShortenLink.Core\Repositories\`
- `src\ShortenLink.Infrastructure\Persistence\`
- `src\ShortenLink.Infrastructure\Repositories\`
- `src\ShortenLink.AspNetCore\ShortenLinkAuthorizationService.cs`
- `src\ShortenLink.AspNetCore\ShortenLinkPermissions.cs`
- `src\ShortenLink.AspNetCore\ShortenLinkServiceCollectionExtensions.cs`
- `tests\ShortenLink.Core.Tests\`
- `tests\ShortenLink.Infrastructure.Tests\`
- `tests\ShortenLink.Api.Tests\`

## Acceptance Criteria

- System roles `Owner`, `Admin`, `Editor`, and `Viewer` are available as built-in permission bundles.
- Custom roles can be persisted with a name and supported permission set.
- Unknown permissions are rejected before persistence.
- Users can be persisted with username, display name, password hash, assigned role identifiers, enabled state, and hidden/bootstrap marker.
- Bootstrap admin user `admin` / `admin` is ensured for local/demo setup and marked hidden from normal user-management lists.
- API keys are persisted as user-owned credentials with hashed key material, display name, enabled state, and creation timestamp.
- Raw password and raw API key values are not stored.
- Existing permission strings continue to be the authorization source of truth.
- Focused backend tests pass.

## Foundation for Next Step

This task should leave stable security identity repositories and contracts so the next task can add login/session or token issuance APIs without redesigning storage.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Core.Tests\ShortenLink.Core.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
```

## Done Notes

Not started.
