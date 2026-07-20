---
task: 016_002
phase: 016
title: Short Link Admin Recovery States
status: done
created_at: 2026-07-20
completed_at: 2026-07-20T09:45:19+07:00
depends_on:
  - 016_001
---

# 016_002 - Short Link Admin Recovery States

## Step Goal

Apply the shared `016_001` failure contract to the main short-link list, analytics read, and short-link mutation workflows.

This task should let operators recover from transient read failures explicitly and safely retry failed mutations themselves without losing relevant form, selection, filter, or dialog context.

## Scope

In:

- Use typed `ApiError` failure metadata when presenting main list, analytics, and mutation failures.
- Keep the current discovery query, page, and page-size context when retrying a failed list read.
- Add an explicit retry action to transient analytics failures while keeping the analytics dialog open.
- Preserve create/edit form values and the open editor after retryable save failures.
- Preserve selected rows after failed bulk activation, deactivation, or deletion.
- Keep row-action context usable after failed activate, deactivate, update, or delete operations.
- Ensure non-retryable validation failures remain field-level or contextual instead of being presented as outage recovery.
- Add focused frontend tests for retry presentation policy and state-preservation decisions.

Out:

- Do not automatically retry create, update, activate, deactivate, delete, or bulk mutations.
- Do not change backend APIs, auth redirects, permission checks, or the shared classification rules from `016_001` unless a verified integration defect requires it.
- Do not retrofit login or security-management workflows in this task.
- Do not add offline persistence, service workers, or third-party retry libraries.
- Do not redesign the admin table, editor, analytics dialog, or discovery toolbar.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/016/PHASE_SUMMARY.md`
- `src\ShortenLink.Web\src\features\short-links\pages\ShortLinkAdminPage.tsx`
- `src\ShortenLink.Web\src\shared\api\`
- `src\ShortenLink.Web\src\shared\components\`
- `src\ShortenLink.Web\test\`

## Acceptance Criteria

- A failed list retry uses the same search, status, sort, page, and page-size context that was visible when the request failed.
- Retryable analytics failures show a retry action and retain the selected link's analytics dialog.
- Retryable create and update failures keep the editor open with the submitted values intact.
- Failed bulk mutations keep the prior row selection so the operator can explicitly try again or cancel.
- Failed row mutations keep the affected row and do not claim success or silently repeat the operation.
- Validation and other non-retryable failures continue to use field-level or contextual messages without misleading retry affordances.
- Existing `401/403` navigation and permission-aware controls remain unchanged.
- Focused Bun tests cover representative read retry, mutation state preservation, and non-retryable validation behavior.

## Foundation for Next Step

This task should leave the primary short-link admin workflow resilient enough for the next task to apply the same shared failure contract to login and security-management screens, then evaluate phase closure.

## Verification

Run after implementation:

```powershell
Set-Location src\ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Added shared recovery presentation helpers for retry affordances and retryable mutation context preservation.
- Preserved existing list data, selection, discovery query, page, and page-size state when list reads fail; retry targets the failed numbered page explicitly.
- Added recoverable inline list failures without hiding previously loaded rows, plus contextual non-retry mutation failure banners.
- Added analytics retry actions for transient failures while retaining the selected link and open analytics dialog.
- Kept create/edit dialogs and submitted values open after retryable save failures; validation errors remain mapped to their fields.
- Preserved selected rows and row data after failed bulk or single-row mutations without automatically repeating unsafe operations.
- Added focused Bun coverage for read retry presentation, retryable mutation preservation, and non-retryable validation behavior.
- Verified with `bun test` (14 passed) and `bun run build`.
