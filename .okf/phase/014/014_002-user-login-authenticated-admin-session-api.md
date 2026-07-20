---
task: 014_002
phase: 014
title: User Login And Authenticated Admin Session API
status: done
created_at: 2026-07-17
completed_at: 2026-07-17T15:10:32+07:00
depends_on:
  - 014_001
---

# 014_002 - User Login And Authenticated Admin Session API

## Step Goal

Add backend login and authenticated admin-session APIs that authenticate persisted users and replace the frontend's reliance on configured admin API keys.

This task should prove the hidden bootstrap admin user from `014_001` can sign in with `admin` / `admin`, receive a credential suitable for admin API calls, and resolve permissions through the user and role model.

## Dependency

- `014_001` provides persisted users, password hashes, built-in role bundles, custom role storage contracts, and the hidden bootstrap admin user.

## Scope

In:

- Add login request/response contracts for username and password authentication.
- Verify password hashes without exposing raw password material.
- Issue a local/demo admin session or token suitable for API authorization.
- Add a current-user or session introspection endpoint for frontend bootstrap.
- Resolve effective permissions from enabled user role assignments.
- Reject disabled, hidden-only-invalid, unknown, or bad-password login attempts consistently.
- Add focused API tests for bootstrap login, bad credentials, disabled users, and permission resolution.

Out:

- Do not build the frontend login page in this task.
- Do not add OAuth/OIDC/SAML, refresh-token rotation, MFA, password reset, or public signup.
- Do not expose raw password hashes, API-key hashes, or implementation secrets in responses.
- Do not redesign the `014_001` persistence contracts unless implementation uncovers a blocking defect.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `src\ShortenLink.Core\Security\`
- `src\ShortenLink.AspNetCore\`
- `src\ShortenLink.Api\`
- `tests\ShortenLink.Core.Tests\`
- `tests\ShortenLink.Api.Tests\`
- `README.md`

## Acceptance Criteria

- `admin` / `admin` can authenticate against the seeded bootstrap admin user in local/demo setup.
- Login failures do not disclose whether username or password was wrong.
- Disabled users cannot authenticate.
- Successful login returns only safe user/session metadata and never returns password hashes.
- Authenticated admin requests can derive permissions from the logged-in user's assigned roles.
- Existing permission-protected endpoints remain protected.
- Focused API tests cover successful bootstrap login, failed login, disabled-user rejection, and current-user permission metadata.

## Foundation for Next Step

This task should leave a stable authenticated-user API boundary so role/user management endpoints can require user-derived permissions instead of configured API keys.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Core.Tests\ShortenLink.Core.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
```

## Done Notes

- Added password hash verification for the Core security credential hasher.
- Added a signed local/demo Bearer session service for persisted security users, with safe current-user principals and effective permissions resolved from system and custom roles.
- Added `POST /api/security/login` and `GET /api/security/me` contracts.
- Extended authorization so protected admin endpoints can authorize either user session Bearer tokens or the existing configured/persisted API-key credentials.
- Documented local bootstrap login, safe response metadata, session configuration, and credential error behavior in README/appsettings.
- Added focused Core and API tests for bootstrap admin login, generic login failure, disabled-user rejection, current-user metadata, and permission enforcement through logged-in user roles.
