---
task: 012_002
phase: 012
title: Admin Analytics UI Detail Panel MVP
status: done
created_at: 2026-07-16
completed_at: 2026-07-16T22:53:22+07:00
depends_on:
  - 012_001
  - 011_003
  - 010_001
---

# 012_002 - Admin Analytics UI Detail Panel MVP

## Step Goal

Surface the analytics API from `012_001` in the React admin experience so an admin can inspect click count, last-clicked timestamp, and recent click activity for a selected short link without leaving the management flow.

This task should make analytics visible and operational in the browser while keeping charts, dashboards, and reporting out of scope.

## Dependency

- `012_001` added `GET /api/short-links/{code}/analytics`, including `clickCount`, `lastClickedAtUtc`, and `recentClicks`.
- `011_003` added frontend credential configuration and permission-aware admin controls.
- Phase 010 added `/unauthorized` and `/forbidden` status pages for protected API outcomes.

## Scope

In:

- Add frontend API types and client function for `GET /api/short-links/{code}/analytics`.
- Add a compact analytics panel in the admin/detail experience for the currently selected short link.
- Show click count and last-clicked timestamp with a clear empty state when no clicks exist.
- Show recent click activity with timestamp, user agent, referrer, and remote IP where available.
- Route `401` and `403` analytics API outcomes through the existing status-page handling.
- Hide or disable analytics UI affordances when the configured frontend permission set lacks `analytics.read`.
- Keep existing create, update, activate/deactivate, delete, pagination, copy, and status routing behavior intact.
- Update README only if frontend analytics usage or environment variables need clarification.

Out:

- Do not build dashboard charts, aggregate metrics cards, CSV export, or report scheduling.
- Do not change backend analytics contracts unless the frontend exposes a concrete mismatch.
- Do not add new frontend dependencies unless strictly necessary.
- Do not add auth provider integration or role-management UI.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/012/PHASE_SUMMARY.md`
- `.okf/phase/012/012_002-admin-analytics-ui-detail-panel-mvp.md`
- `src\ShortenLink.Web\src\features\short-links\api\shortLinksApi.ts`
- `src\ShortenLink.Web\src\features\short-links\api\http.ts`
- `src\ShortenLink.Web\src\features\short-links\types.ts`
- `src\ShortenLink.Web\src\features\short-links\pages\ShortLinkAdminPage.tsx`
- `src\ShortenLink.Web\src\features\short-links\components\`
- `README.md` only if frontend setup guidance changes

## Acceptance Criteria

- Frontend can request analytics for a selected short link through the `012_001` endpoint.
- Admin UI displays click count and last-clicked timestamp for the selected short link.
- Admin UI displays recent click activity with graceful handling for missing IP, user agent, or referrer.
- Links with no clicks show a clear empty analytics state.
- Analytics API `401` navigates to `/unauthorized`; `403` navigates to `/forbidden`.
- Analytics UI controls or panel access are unavailable without `analytics.read` in the configured frontend permission set.
- Existing admin list, detail, create, update, activate/deactivate, delete, copy, and pagination behavior remains intact.
- Frontend build passes.

## Foundation for Next Step

This task should leave an analytics UI pattern that a later task can extend into list columns, dashboard metrics, or richer activity views without redesigning frontend API access.

## Verification

Run after implementation:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Run backend verification only if backend contracts change:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

## Done Notes

Completed on 2026-07-16T22:53:22+07:00.

Implemented:

- Added frontend analytics response types for click count, last-clicked timestamp, and recent click activity.
- Added `getShortLinkAnalytics(...)` API client support for `GET /api/short-links/{code}/analytics`.
- Added `canReadAnalytics` to the admin permission state using the existing `analytics.read` permission.
- Added an Analytics row action that is only available when the configured frontend permissions include `analytics.read`.
- Added a compact admin analytics dialog for a selected short link.
- The dialog displays click count, last-clicked timestamp, recent click activity, loading state, error state, and no-clicks empty state.
- Recent activity gracefully handles missing user agent, referrer, and remote IP.
- Existing `fetchJson` behavior continues routing analytics `401` and `403` responses to `/unauthorized` and `/forbidden`.
- Kept existing create, update, activate/deactivate, delete, copy, bulk, and pagination behavior intact.

Verification:

- `npm run build` passed in `src\ShortenLink.Web`.
- Backend verification was not rerun because this task only changed frontend files and consumed the already verified `012_001` backend contract.
