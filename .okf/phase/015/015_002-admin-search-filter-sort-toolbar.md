---
task: 015_002
phase: 015
title: Admin Search Filter And Sort Toolbar
status: done
created_at: 2026-07-20
completed_at: 2026-07-20T08:47:08+07:00
depends_on:
  - 015_001
---

# 015_002 - Admin Search Filter And Sort Toolbar

## Step Goal

Add compact React admin controls for searching, filtering, and sorting short links through the verified `015_001` list query contract.

This task should let operators narrow and order the protected short-link list while keeping pagination, bulk actions, and permission-aware controls coherent.

## Scope

In:

- Add a compact toolbar to the admin short-link list for search, status, sort field, and sort direction.
- Send `search`, `status`, `sortBy`, and `sortDirection` through the existing list API client.
- Reset numbered pagination when discovery criteria change and preserve filtered pagination metadata from the API.
- Support all API-defined status and sort values with clear labels.
- Provide usable loading, empty-filter-result, invalid-query, and retry states.
- Add focused frontend tests for query construction and the main toolbar interactions.

Out:

- Do not add saved views, user preferences, advanced query syntax, or export workflows.
- Do not implement client-side filtering or sorting that duplicates backend behavior.
- Do not change the backend list query contract established by `015_001` unless a verified integration defect requires it.
- Do not redesign unrelated create, detail, security, analytics, or bulk-action workflows.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/015/PHASE_SUMMARY.md`
- `src\ShortenLink.Web\src\features\short-links\api\shortLinksApi.ts`
- `src\ShortenLink.Web\src\features\short-links\pages\ShortLinkAdminPage.tsx`
- `src\ShortenLink.Web\src\features\short-links\components\`
- `src\ShortenLink.Web\src\styles.css`
- `src\ShortenLink.Web\test\`

## Acceptance Criteria

- Admin users can search by short code or destination URL from the short-link list.
- Admin users can filter by all, active, inactive, expired, and expiring-soon statuses.
- Admin users can sort by created date, expiry, destination URL, code, and status in ascending or descending order.
- The frontend sends only supported discovery query values and consumes filtered pagination metadata returned by the API.
- Changing search, filter, or sort criteria returns the list to the first numbered page.
- Loading, no-match, validation-error, and request-failure states are clear and recoverable.
- Existing bulk actions and permission-aware controls remain usable with filtered results.
- Focused frontend tests cover API query serialization and representative toolbar state changes.

## Foundation for Next Step

This task should leave the admin discovery workflow complete enough for phase-wide verification, documentation review, and closure.

## Verification

Run after implementation:

```powershell
Set-Location src\ShortenLink.Web
bun test
bun run build
```

## Done Notes

- Added typed frontend discovery contracts and list query serialization for search, status, sort field, and sort direction.
- Added a compact admin discovery toolbar with supported API values, explicit search submission, and reset behavior.
- Reset numbered pagination to page 1 when discovery criteria change while preserving filtered API counts and page metadata.
- Added distinct loading, no-match, validation/request-error, retry, and clear-filter experiences without removing bulk or permission-aware actions.
- Added focused Bun tests for URL serialization, default handling, filter detection, and pagination reset behavior.
- Verified with `bun test` (5 passed) and `bun run build`.
