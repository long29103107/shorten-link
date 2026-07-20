---
task: 014_004
phase: 014
title: User-Owned API Key Management And Authorization
status: done
created_at: 2026-07-17
completed_at: 2026-07-17T15:27:21+07:00
depends_on:
  - 014_001
  - 014_002
  - 014_003
---

# 014_004 - User-Owned API Key Management And Authorization

## Step Goal

Allow logged-in users to create and manage their own API keys, and allow API authorization to evaluate permissions from user-owned API keys.

This task should replace the legacy credential-assignment mental model with API keys as credentials owned by users whose effective permissions come from assigned roles.

## Dependency

- `014_001` provides user-owned API key persistence and hashing contracts.
- `014_002` provides authenticated users and effective permission resolution.
- `014_003` provides role/user management APIs and assigned roles.

## Scope

In:

- Add authenticated endpoints for a logged-in user to list, create, disable, and rename their own API keys.
- Return raw API-key material only once in the create response.
- Store only hashed API-key material.
- Update authorization so API-key requests resolve the owning user and effective role permissions.
- Preserve compatibility for existing protected short-link and security endpoints.
- Add tests for one-time raw key display, hash-only storage, disabled-key rejection, and user-owned permission evaluation.

Out:

- Do not build frontend API-key UI in this task.
- Do not add key expiration policy, rotation reminders, audit logs, or per-key permission narrowing unless needed by existing contracts.
- Do not expose API keys across users.
- Do not continue relying on raw configured admin API keys for the new user-owned key path.

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
- `src\ShortenLink.AspNetCore\ShortenLinkAuthorizationService.cs`
- `src\ShortenLink.AspNetCore\`
- `src\ShortenLink.Api\`
- `tests\ShortenLink.Core.Tests\`
- `tests\ShortenLink.Infrastructure.Tests\`
- `tests\ShortenLink.Api.Tests\`
- `README.md`

## Acceptance Criteria

- A logged-in user can list only their own API-key metadata.
- A logged-in user can create an API key and see the raw key only in the create response.
- Raw API-key material is never stored or returned by list/detail responses.
- Disabled API keys are rejected by authorization.
- API-key authorization evaluates permissions through the owning user's assigned system and custom roles.
- Existing permission-protected endpoints accept valid user-owned API keys and reject insufficient permissions.
- Focused tests cover create/list/disable behavior, hash-only persistence, and protected endpoint access through user-owned API keys.

## Foundation for Next Step

This task should leave a complete backend identity and credential API surface for the frontend to implement login, role/user management, and personal API-key workflows.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Core.Tests\ShortenLink.Core.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
```

## Done Notes

- Added Bearer-authenticated personal API-key endpoints for logged-in users to list, create, rename, and disable only their own keys.
- API-key create responses return raw key material once; list, rename, and disable responses return metadata only and never expose raw keys or key hashes.
- Added hash-only persistence checks and ownership checks before rename/disable operations.
- Updated authorization so raw API keys sent through the configured API-key header can resolve user-owned API keys, reject disabled keys/users, and evaluate permissions through the owning user's enabled roles.
- Preserved configured and persisted legacy API-key assignment compatibility as a fallback path.
- Documented personal API-key APIs and one-time raw key display behavior in README.
- Added repository/API tests covering create/list/rename/disable, hash-only persistence, own-key isolation, disabled-key rejection, and endpoint authorization through user-owned API keys.
