---
task: 012_001
phase: 012
title: Admin Analytics Summary API MVP
status: done
created_at: 2026-07-16
completed_at: 2026-07-16T22:36:09+07:00
depends_on:
  - 003_001
  - 011
---

# 012_001 - Admin Analytics Summary API MVP

## Step Goal

Expose the first admin-facing analytics API surface on top of the existing click-tracking persistence: admins with `analytics.read` can retrieve click count, last-clicked timestamp, and recent click activity for short links.

This task should make analytics useful to the admin app without adding charts or changing redirect behavior.

## Dependency

- Phase 003 added click tracking contracts, EF-backed click persistence, and async redirect recording.
- Phase 011 added permission-based admin protection and the `analytics.read` permission through built-in role bundles.

## Scope

In:

- Add reusable query/repository support for click summaries and recent click activity.
- Expose an admin API endpoint or stable response fields for click count and last-clicked timestamp.
- Expose recent click activity for a selected short link.
- Require `analytics.read` for the new admin analytics surface when security is enabled.
- Preserve analytics-disabled behavior by returning empty/zero analytics results rather than failing ordinary admin flows.
- Add tests for populated analytics, empty analytics, and `401`/`403` protected analytics access.
- Update README with the new admin analytics API behavior.

Out:

- Do not build frontend charts, dashboard cards, or list-column UI in this task.
- Do not change redirect tracking semantics or require redirects to wait on analytics persistence.
- Do not add new analytics providers, queues, workers, or external telemetry sinks.
- Do not add export CSV or report scheduling.
- Do not add authentication provider integration.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/012/PHASE_SUMMARY.md`
- `.okf/phase/012/012_001-admin-analytics-summary-api-mvp.md`
- `src\ShortenLink.Core\`
- `src\ShortenLink.Infrastructure\`
- `src\ShortenLink.AspNetCore\ShortenLinkEndpointRouteBuilderExtensions.cs`
- `tests\ShortenLink.Infrastructure.Tests\`
- `tests\ShortenLink.Api.Tests\`
- `README.md`

## Acceptance Criteria

- A reusable backend query path can calculate click count and last-clicked timestamp for a short code.
- A reusable backend query path can return recent click activity for a short code with a safe limit.
- Admin analytics API returns zero/empty analytics for links with no clicks.
- Admin analytics API requires `analytics.read` when security is enabled.
- Missing analytics credentials return stable `401 unauthorized`.
- Valid credentials without `analytics.read` return stable `403 forbidden`.
- Existing admin and redirect endpoints keep their current contracts.
- Tests cover summary, recent activity, empty state, and protected access.
- README documents the analytics admin API and permission requirement.

## Foundation for Next Step

This task should leave stable admin analytics contracts that a later frontend task can use to add click-count columns, last-clicked badges, recent activity detail, or dashboard metrics without redesigning analytics persistence.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

Run frontend verification only if frontend files change:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

## Done Notes

Completed on 2026-07-16T22:36:09+07:00.

Implemented:

- Extended `IShortLinkClickRepository` with reusable analytics query methods for click summary and recent click activity.
- Implemented EF Core click summary/recent queries in `EfCoreShortLinkClickRepository`, keeping SQLite-compatible client-side ordering for `DateTimeOffset`.
- Added `GET /api/short-links/{code}/analytics` to the reusable ASP.NET Core endpoint surface.
- Protected the analytics endpoint with the existing `analytics.read` permission when security is enabled.
- Returned stable analytics payloads with `clickCount`, `lastClickedAtUtc`, and `recentClicks`.
- Preserved empty analytics behavior for links with no clicks by returning zero count, null last click, and an empty recent-click list.
- Updated README with the admin analytics endpoint, payload behavior, and permission requirement.
- Added Infrastructure tests for summary, empty state, recent activity ordering, and schema coverage.
- Added API tests for populated analytics, empty analytics, `401 unauthorized`, and `403 forbidden`.

Verification:

- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test tests\ShortenLink.Infrastructure.Tests\ShortenLink.Infrastructure.Tests.csproj --no-build --verbosity minimal` passed with 19 tests.
- `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --no-build --verbosity minimal` passed with 46 tests.
- `dotnet test ShortenLink.slnx --no-build --verbosity minimal` passed with 97 total tests.
- Frontend build was not run because this task did not change frontend files.
