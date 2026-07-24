---
task: 019_001
phase: 019
title: Accurate dashboard snapshot and partial health
status: done
created_at: 2026-07-24
completed_at: 2026-07-24T09:32:28+07:00
depends_on:
  - 018_005
---

# 019_001 - Accurate Dashboard Snapshot and Partial Health

## Step Goal

Replace approximate dashboard counts with authoritative server totals and keep healthy metrics visible when one dashboard source fails.

## Scope

In:

- Query total, active, and deactivated short-link counts through server-side status discovery.
- Summarize managed/enabled users and available roles.
- Track Short Links, Users, and Roles health independently.
- Render operational, degraded, loading, and retry states contextually.
- Add focused pure tests for dashboard snapshot composition.

Out:

- Historical trends or time-series storage.
- New backend endpoints.
- Audit-log visualization.

## Acceptance Criteria

- Active and deactivated counts use API totals, not the latest page.
- User and role metrics remain visible when Short Links fails, and vice versa.
- Source-level health identifies exactly which source failed.
- Refresh retries every source.
- Snapshot composition has focused tests.
- Frontend tests and production build pass.

## Foundation for Next Step

Leaves a resilient current-state dashboard ready for trend and audit visibility.

## Affected Files

- `.okf/phase/019/PHASE_SUMMARY.md`
- `.okf/phase/019/019_001-accurate-dashboard-snapshot-and-partial-health.md`
- `src/ShortenLink.Web/src/features/short-links/pages/AdminDashboardPage.tsx`
- `src/ShortenLink.Web/src/features/short-links/adminDashboard.ts`
- `src/ShortenLink.Web/test/admin-dashboard.test.ts`
- `src/ShortenLink.Web/src/styles.css`

## Verification

```powershell
cd src/ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Replaced latest-page approximations with API `totalCount` queries for all, active, and inactive short links.
- Added managed/enabled user and available-role metrics.
- Kept source failures independent so healthy dashboard metrics remain visible.
- Added source-level Available/Unavailable health and operational/degraded states.
- Added focused snapshot composition tests.
- Verification passed: `bun test` (40 passed, 0 failed) and `bun run build`.
