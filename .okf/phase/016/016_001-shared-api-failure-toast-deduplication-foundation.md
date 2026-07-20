---
task: 016_001
phase: 016
title: Shared API Failure And Toast Deduplication Foundation
status: done
created_at: 2026-07-20
completed_at: 2026-07-20T08:55:59+07:00
depends_on:
  - 015
---

# 016_001 - Shared API Failure And Toast Deduplication Foundation

## Step Goal

Create a shared frontend contract for classifying API failures and suppressing repeated equivalent error toasts during a bounded outage window.

This task should give later admin workflow tasks one tested source of truth for deciding whether a failure is transient, requires authentication handling, can expose retry, or should remain a field-level validation error.

## Scope

In:

- Represent network, transient server, validation, authentication, authorization, not-found, and unexpected failures with typed frontend metadata.
- Preserve existing backend `errorCode`, status, and message details in the shared failure contract.
- Add a deterministic policy for whether a failure should offer retry or continue through existing auth navigation.
- Add bounded deduplication for equivalent error toasts while allowing distinct errors and later recurrences.
- Keep toast deduplication testable without relying on real timers or browser rendering.
- Add focused Bun tests for classification, retry eligibility, deduplication keys/windows, and auth behavior.

Out:

- Do not retrofit every page or mutation workflow in this task.
- Do not automatically retry create, update, activate, deactivate, delete, or security mutations.
- Do not change backend errors, routes, authentication, or permission contracts.
- Do not add third-party resilience or notification dependencies.
- Do not suppress success, warning, or intentionally distinct error messages.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/016/PHASE_SUMMARY.md`
- `src\ShortenLink.Web\src\features\short-links\api\http.ts`
- `src\ShortenLink.Web\src\shared\api\`
- `src\ShortenLink.Web\src\shared\toast.ts`
- `src\ShortenLink.Web\test\`

## Acceptance Criteria

- Fetch/network failures and HTTP/API failures are converted to a typed shared failure shape.
- The shared policy identifies transient network, timeout, rate-limit, and `5xx` failures as retryable while keeping validation and auth failures non-retryable by default.
- Existing `401` and `403` navigation behavior remains compatible with the new failure contract.
- Equivalent error toasts emitted inside the configured suppression window produce one visible notification.
- Distinct errors, non-error toast variants, and equivalent errors after the suppression window remain visible.
- Deduplication state is bounded and does not grow indefinitely.
- Focused Bun tests cover representative network, `4xx`, `5xx`, auth, and toast-window cases.

## Foundation for Next Step

This task should leave a tested shared failure and notification contract that the next task can apply to the main short-link read and mutation workflows without duplicating retry or deduplication rules.

## Verification

Run after implementation:

```powershell
Set-Location src\ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Added a shared typed failure contract for network, timeout, rate-limit, server, validation, authentication, authorization, not-found, and unexpected failures.
- Preserved HTTP status, backend error code, and message details while exposing retry and auth-navigation policy through `ApiError`.
- Converted rejected fetch calls into retryable typed network or timeout failures without changing existing `401` and `403` navigation behavior.
- Added bounded, deterministic error-toast deduplication with a five-second default suppression window; distinct and non-error notifications remain visible.
- Added focused Bun coverage for network, timeout, `4xx`, `5xx`, auth, retryability, deduplication windows, distinct notifications, and bounded state.
- Verified with `bun test` (11 passed) and `bun run build`.
