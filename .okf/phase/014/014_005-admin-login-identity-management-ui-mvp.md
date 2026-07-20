---
task: 014_005
phase: 014
title: Admin Login And Identity Management UI MVP
status: done
created_at: 2026-07-17
completed_at: 2026-07-17T15:44:56+07:00
depends_on:
  - 014_002
  - 014_003
  - 014_004
---

# 014_005 - Admin Login And Identity Management UI MVP

## Step Goal

Add the frontend login and identity-management experience for users, roles, and personal API keys.

This task should let an operator sign in with the bootstrap admin user, manage normal users and custom roles, and create personal API keys without exposing raw secrets after creation.

## Dependency

- `014_002` provides login/session APIs and current-user metadata.
- `014_003` provides role and user management APIs.
- `014_004` provides user-owned API-key APIs and authorization behavior.

## Scope

In:

- Add an admin login screen and authenticated app bootstrap flow.
- Replace Vite-configured admin API-key usage in the frontend with the authenticated session/token path.
- Add permission-aware navigation and route protection for identity-management screens.
- Add compact UI for listing and managing normal users.
- Add compact UI for listing system roles and managing custom roles.
- Add personal API-key UI with one-time raw key display after creation.
- Handle loading, empty, validation-error, unauthorized, and failure states consistently with existing admin UI patterns.

Out:

- Do not add public signup, invitation emails, password reset, MFA, or OAuth/OIDC UI.
- Do not display the hidden bootstrap admin user in normal user-management lists.
- Do not persist or redisplay raw API keys after the creation response.
- Do not redesign unrelated short-link or analytics screens.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `src\ShortenLink.Web\src\app\`
- `src\ShortenLink.Web\src\features\`
- `src\ShortenLink.Web\src\shared\`
- `src\ShortenLink.Web\src\test\`
- `src\ShortenLink.Web\package.json`
- `README.md`

## Acceptance Criteria

- Admin users can sign in through the frontend using the backend login API.
- Frontend admin API calls use the authenticated session/token path instead of configured admin API keys.
- Navigation and routes honor current-user permissions.
- User-management UI excludes the hidden bootstrap admin user.
- Custom-role UI validates and submits supported permissions.
- Personal API-key UI shows raw key material only immediately after creation.
- Existing admin short-link and analytics workflows remain usable after authentication changes.
- Focused frontend tests cover login success/failure, permission-aware routes, hidden bootstrap list behavior, and one-time API-key display.

## Foundation for Next Step

This task should leave the user-facing identity workflow complete enough for final phase-wide verification, documentation, and cleanup.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
Set-Location src\ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Added frontend login/session storage, Bearer-token admin requests, current-user bootstrap refresh, sign-in/sign-out navigation, and local handling for login auth failures.
- Replaced the legacy security page route with a compact identity-management page for users, system/custom roles, personal API keys, and legacy credential assignments.
- Added user, custom-role, and personal API-key API clients and one-time raw API-key display behavior.
- Added permission-aware security navigation and session panel styling consistent with the existing admin UI.
- Added focused Bun tests for login routing and invalid-login friendly error mapping.
- Verified with `dotnet build ShortenLink.slnx --verbosity minimal`, `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal`, `bun test`, and `bun run build`.
