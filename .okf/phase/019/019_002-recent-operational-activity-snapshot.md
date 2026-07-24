---
task: 019_002
phase: 019
title: Recent operational activity snapshot
status: done
created_at: 2026-07-24
updated_at: 2026-07-24
completed_at: 2026-07-24
---

# 019_002 — Recent Operational Activity Snapshot

## Step Goal

Add a compact Admin Dashboard activity view from authoritative recent short-link and user creation data while preserving partial-source failure behavior.

## Scope

- Compose deterministic recent activity events from existing short-link and security-user list responses.
- Show recent link creation and identity registration in one chronological dashboard view.
- Keep loading, empty, and partial-source states contextual.
- Do not claim to provide a durable mutation audit log; no audit persistence/API exists yet.

## Acceptance Criteria

- Dashboard shows a bounded, newest-first activity list.
- Every entry identifies its source, subject, timestamp, and useful actor/context text.
- Failure of one upstream source does not hide healthy activity from the other.
- Empty and degraded states are explicit.
- Focused frontend tests and production build pass.

## Foundation for Next Step

The dashboard owns a reusable activity presentation boundary that a future persisted audit-log API can replace without redesigning the page.

## Affected Files

- `src/ShortenLink.Web/src/features/short-links/adminDashboard.ts`
- `src/ShortenLink.Web/src/features/short-links/pages/AdminDashboardPage.tsx`
- `src/ShortenLink.Web/src/styles.css`
- `src/ShortenLink.Web/test/admin-dashboard.test.ts`
- `.okf/phase/019/PHASE_SUMMARY.md`

## Verification

- Passed `bun test`: 41 tests, 0 failures.
- Passed `bun run build`: TypeScript and Vite production build.

## Done Notes

- Added deterministic composition of recent link and user creation events with newest-first ordering and a bounded result.
- Added a compact, responsive Admin Dashboard activity surface with loading, empty, degraded, and healthy states.
- Preserved partial-source behavior by deriving activity only from fulfilled list responses.
- Labeled the view as a current-record creation snapshot rather than a durable mutation audit log.
