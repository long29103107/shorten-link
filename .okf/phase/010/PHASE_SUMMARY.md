---
phase: 010
title: HTTP Status Experience
status: done
created_at: 2026-07-15
updated_at: 2026-07-15
current_task:
task_count: 1
done_count: 1
depends_on:
  - 008
---

# Phase 010 Summary

## Phase Goal

Add a small, reusable HTTP status experience for the demo app and integration surface so unauthorized, forbidden, and not-found outcomes are handled consistently.

Phase 010 should make `401`, `403`, and `404` visible as deliberate product states instead of incidental browser or JSON failures, while preserving the reusable package boundary.

## Phase Done Criteria

- The React demo has explicit user-facing routes or states for `401`, `403`, and `404`.
- API or endpoint behavior that returns `401`, `403`, or `404` has stable response expectations.
- The existing short-link fallback behavior remains compatible with the new `404` experience.
- The implementation does not introduce authentication as a product feature unless a task explicitly scopes it.
- Verification covers routing/status behavior with focused frontend and/or API checks.

## Scope

In:

- Demo UI status pages or shared status component for `401`, `403`, and `404`.
- Route parsing/navigation support for those status pages.
- Stable API/error response expectations where the backend already emits these statuses.
- README or developer notes only if the new status routes need documentation.

Out:

- Do not add login, users, roles, JWT, OAuth, or tenant authorization.
- Do not change package IDs or release automation.
- Do not publish packages.
- Do not replace the existing short-link create/detail/redirect/deactivate flows.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 010_001 | HTTP 401 403 404 status pages MVP | done | 2026-07-15T20:18:59+07:00 |

## Current Task

No active task.

## Completed Notes

- `010_001` added standalone reusable React status pages for unauthorized, forbidden, and not-found states, mapped `/not-found` and unknown frontend routes to the `404` status experience, documented the routes in README, and verified frontend build plus API status expectations.

## Next Task Proposal

Open Phase 011 for permission-based security and admin protection.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 010_001 - HTTP 401 403 404 Status Pages MVP

Source before compaction: `010_001-http-401-403-404-status-pages-mvp.md`

#### Step Goal

Create one cohesive slice for handling `401 Unauthorized`, `403 Forbidden`, and `404 Not Found` as intentional user-facing states in the demo app, with stable backend/API expectations where those statuses already exist.

This task should make missing access and missing resource states clear without adding a full authentication or authorization system.

#### Dependency

- Phase 001 added the React create/detail/fallback demo flow and configurable unknown-code fallback behavior.
- Phase 003 added endpoint hardening and stable endpoint contracts.
- Phase 008 closed the current release-ready product definition; this task is optional future product polish.

#### Scope

In:

- Add React routes or route states for `401`, `403`, and `404`.
- Prefer a shared status-page component with status-specific title, message, and primary action.
- Keep the existing unknown short-code fallback compatible with the new `404` page.
- Ensure direct browser navigation to `/unauthorized`, `/forbidden`, and `/not-found` renders the expected state.
- Ensure unknown frontend routes land on the `404` experience instead of a blank or confusing state.
- Add focused tests or verification for route parsing/rendering and any affected API status behavior.

Out:

- Do not add login, logout, users, roles, JWT, OAuth, cookies, tenant permissions, or real authorization policy enforcement.
- Do not change short-link core service contracts unless a concrete status-handling bug requires it.
- Do not change package IDs, publish scripts, release workflow, or NuGet publishing docs.
- Do not replace existing short-link create/detail/redirect/deactivate behavior.
- Do not make `401` or `403` appear for existing public short-link flows unless a future auth task introduces protected resources.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/010/PHASE_SUMMARY.md`
- `.okf/phase/010/010_001-http-401-403-404-status-pages-mvp.md`
- `src\ShortenLink.Web\src\app\router.ts`
- `src\ShortenLink.Web\src\app\App.tsx`
- `src\ShortenLink.Web\src\features\short-links\types.ts`
- `src\ShortenLink.Web\src\features\short-links\`
- `src\ShortenLink.Web\src\app\`
- `src\ShortenLink.AspNetCore\ShortenLinkEndpointRouteBuilderExtensions.cs`
- `tests\ShortenLink.Api.Tests\ShortLinkEndpointsTests.cs`

#### Acceptance Criteria

- `/unauthorized` renders a clear unauthorized state telling the user access requires permission or sign-in, without implementing sign-in.
- `/forbidden` renders a clear forbidden state telling the user access is denied even though the route exists.
- `/not-found` renders a clear not-found state and is reused for unknown frontend routes.
- Existing unknown short-link fallback behavior still lands on the configured frontend fallback path and produces the expected user-facing not-found state.
- Status pages provide a safe action back to the create-link/home flow.
- Frontend route parsing handles known status paths deterministically.
- Backend/API status expectations remain stable for existing `404` short-link cases; add `401`/`403` backend coverage only if implementation introduces explicit endpoints or responses for them.

#### Foundation for Next Step

This task should leave a reusable status-page pattern that future authentication, authorization, admin, or protected-link work can reuse without redesigning error handling.

#### Verification

Run frontend verification after React/routing changes:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Run backend verification if API endpoint behavior or tests change:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
```

For task-only changes, read back `.okf\phase\010\PHASE_SUMMARY.md` and this task note.

#### Done Notes

- Implemented reusable React `StatusPage` for `401`, `403`, and `404`.
- Rendered status pages in a standalone full-page shell without the admin/create sidebar.
- Added deterministic route parsing for `/unauthorized`, `/forbidden`, `/not-found`, and unknown frontend routes.
- Preserved unknown-code fallback compatibility by keeping `/not-found` mapped to the `404` status state.
- Added README documentation for status routes.
- Verification:
  - `npm run build` passed in `src/ShortenLink.Web`.
  - `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --no-build --verbosity minimal` passed, confirming existing API status expectations from the current build.
  - Full `dotnet test ShortenLink.slnx --verbosity minimal` was attempted but blocked by a running `ShortenLink.Api` process locking API output DLLs; Core and Infrastructure tests passed before the API project copy step failed.


## Scan Rule
Agents must read this file before working on any `010_*` task note. Do not activate Phase 010 implementation until prior active phase work is intentionally paused, completed, or superseded by user direction.
