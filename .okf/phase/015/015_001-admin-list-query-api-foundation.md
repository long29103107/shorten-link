---
task: 015_001
phase: 015
title: Admin List Query API Foundation
status: done
created_at: 2026-07-17
completed_at: 2026-07-17T15:57:58+07:00
depends_on:
  - 014
---

# 015_001 - Admin List Query API Foundation

## Step Goal

Add a stable backend query contract for admin short-link search, status filtering, and sorting.

This task should let the API return filtered, sorted, numbered pages with accurate counts so the next frontend task can wire compact controls without guessing client-side behavior.

## Scope

In:

- Add query parameters to `GET /api/short-links` for `search`, `status`, `sortBy`, and `sortDirection`.
- Support `status=all|active|inactive|expired|expiring-soon`.
- Support `sortBy=created|expiry|destination|code|status`.
- Support `sortDirection=asc|desc`.
- Keep existing page/limit behavior and filtered pagination metadata.
- Add focused API tests for search, status filter, sort, and filtered counts.
- Document the query contract in README.

Out:

- Do not add React controls in this task.
- Do not add analytics sorting or click-count filters.
- Do not add database full-text search or provider-specific indexes.
- Do not change create/update/delete contracts.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/015/PHASE_SUMMARY.md`
- `README.md`
- `src\ShortenLink.Core\`
- `src\ShortenLink.Infrastructure\`
- `src\ShortenLink.AspNetCore\`
- `tests\ShortenLink.Api.Tests\`

## Acceptance Criteria

- API returns only links matching a code or destination search term when `search` is provided.
- API returns only links matching the requested status filter.
- API returns deterministic sorted pages for supported sort fields and directions.
- `totalCount` and `totalPages` reflect the filtered result set.
- Invalid status, sort field, or sort direction returns a stable `400` error.
- Existing protected list endpoint behavior remains permission-protected.
- README documents query parameters and accepted values.

## Foundation for Next Step

This task should leave a verified API contract for the next task to add React admin search/filter/sort controls.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
```

## Done Notes

- Added Core list query/page contracts for search, status filters, sort fields, sort direction, and filtered counts.
- Added EF Core repository filtering/sorting with deterministic tie-breakers and a 7-day `expiring-soon` status window.
- Extended `GET /api/short-links` numbered pages with `search`, `status`, `sortBy`, and `sortDirection` query parameters while preserving cursor list behavior.
- Added stable `400` errors for invalid filter and sort query values.
- Added API tests for search+sort+metadata, status filters, and invalid discovery query values.
- Documented the admin list discovery query contract in README.
- Verified with `dotnet build ShortenLink.slnx --verbosity minimal` and `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal`.
