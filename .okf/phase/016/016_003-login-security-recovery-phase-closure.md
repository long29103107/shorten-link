---
task: 016_003
phase: 016
title: Login And Security Recovery Phase Closure
status: done
created_at: 2026-07-20
completed_at: 2026-07-20T09:56:06+07:00
depends_on:
  - 016_001
  - 016_002
---

# 016_003 - Login And Security Recovery Phase Closure

## Step Goal

Apply the shared failure contract to login and security-management workflows, then verify the complete robust admin error UX across phase 016.

This task should let operators recover from transient identity and security API failures without losing safe input or screen context, while preserving authentication redirects, permission behavior, and one-time secret handling.

## Scope

In:

- Use typed `ApiError` failure metadata for login and security-management reads and mutations.
- Preserve entered login credentials after transient failures so the user can explicitly submit again.
- Keep invalid-login failures contextual and non-retryable without changing backend authentication behavior.
- Add explicit retry for transient security-management reads while preserving the active tab and previously loaded data.
- Preserve user, role, API-key name, and assignment form state after retryable mutation failures.
- Preserve one-time raw API-key display guarantees and never restore or redisplay cleared secrets.
- Keep `401/403` navigation and permission-aware sections unchanged.
- Add focused compatibility tests and run phase-wide frontend verification.
- Evaluate all phase done criteria and close phase 016 only when they are satisfied.

Out:

- Do not automatically retry login or any security mutation.
- Do not persist passwords, credential keys, or raw API keys beyond their existing in-memory/session boundaries.
- Do not change backend identity, role, API-key, assignment, auth, or permission contracts.
- Do not add password reset, MFA, OAuth/OIDC, offline storage, or third-party resilience dependencies.
- Do not redesign login or security-management information architecture.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/016/PHASE_SUMMARY.md`
- `src\ShortenLink.Web\src\features\short-links\pages\LoginPage.tsx`
- `src\ShortenLink.Web\src\features\short-links\pages\SecurityManagementPage.tsx`
- `src\ShortenLink.Web\src\shared\api\`
- `src\ShortenLink.Web\src\shared\components\`
- `src\ShortenLink.Web\test\`

## Acceptance Criteria

- Transient login failures keep username and password input intact and allow explicit resubmission.
- Invalid credentials remain a clear non-retryable login error and do not trigger an auth redirect loop.
- Transient security read failures expose retry while keeping the current tab and any previously loaded data visible.
- Retryable security mutation failures preserve the relevant form values for explicit resubmission.
- Non-retryable security validation failures remain contextual and do not show misleading outage recovery actions.
- Raw API-key material remains visible only after a successful creation response and is never reconstructed after failure, refresh, or dismissal.
- Existing `401/403` navigation, stored-session behavior, and permission-aware sections remain unchanged.
- Focused Bun tests cover login retry classification, security read recovery, mutation state preservation, and one-time secret compatibility.
- All frontend tests and the production frontend build pass.
- Every phase 016 done criterion is verified before the phase is marked done.

## Foundation for Next Step

This task should leave robust admin server-error UX complete across the short-link, analytics, login, and security-management workflows so the next phase can address validation parity from the ordered P0 product gaps.

## Verification

Run after implementation:

```powershell
Set-Location src\ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Applied typed recovery notices to login while preserving entered credentials for explicit resubmission and keeping invalid-login failures non-retryable.
- Added retryable security-read notices that preserve the active tab and previously loaded data.
- Kept user, role, API-key name, and assignment forms intact after failed mutations; wrapped disable and rename actions to prevent unhandled API failures.
- Preserved existing `401/403` navigation, stored-session behavior, and permission-aware security sections.
- Enforced one-time raw API-key boundaries by revealing only successful creation results and clearing secrets on a new request, refresh, or dismissal.
- Added focused compatibility tests for transient login behavior, invalid login, security mutation preservation, and one-time secret transitions.
- Verified the complete phase with `bun test` (18 passed) and `bun run build`.
