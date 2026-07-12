---
id: 001_007
phase: 001
task: 007
title: React create/detail/fallback demo flow
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: feature
priority: high
depends_on:
  - 001_006
tags:
  - react
  - vite
  - frontend
  - demo-flow
  - phase-1
---

# 001_007 - React Create Detail Fallback Demo Flow

## Step Goal

Build the Phase 1 React demo experience on top of the verified reusable API contracts: a user can create a short link, copy the generated short URL, inspect link details, deactivate a link, and land on a friendly fallback state when a short code or frontend route is not found.

This task should prove the product is usable from the browser without moving short-link business rules into `ShortenLink.Web`.

## Dependency

- `001_006` completed reusable ASP.NET Core DI and endpoint mapping for create, detail, deactivate, redirect, stable JSON errors, and frontend fallback behavior.
- `001_005` completed SQLite-backed persistence for the API host.
- `001_004` completed reusable core validation and service behavior that the frontend must consume through HTTP rather than duplicate.

## Scope

In:

- Replace the placeholder React scaffold with the actual demo app as the first screen.
- Add frontend API request/response types that match the `001_006` endpoint contracts.
- Add a feature-scoped API client for:
  - `POST /api/short-links`
  - `GET /api/short-links/{code}`
  - `DELETE /api/short-links/{code}`
- Build the home flow with long URL input, optional custom alias, optional expiry, create button, validation/error display, created result, and copy-short-url action.
- Build a detail flow for inspecting original URL, short code, created timestamp, optional expiry, and active/deactivated state.
- Build deactivate/delete behavior that updates UI state after `DELETE /api/short-links/{code}`.
- Build a friendly not-found/fallback page for `/not-found` and unknown frontend routes.
- Add simple client-side route handling that supports home, detail, and fallback views without introducing a heavy router unless needed.
- Configure frontend API base behavior for local development, such as a Vite proxy or a small API base helper.
- Keep backend business rules in the API/library; frontend validation should only improve UX and must not be treated as authoritative.
- Update README only if frontend run/build/API proxy instructions need to change.

Out:

- Do not add authentication, user accounts, dashboards, analytics charts, Redis/cache, rate limiting, Docker Compose, PostgreSQL, or worker infrastructure.
- Do not create backend endpoints that duplicate existing `MapShortenLinkEndpoints()` behavior.
- Do not add a marketing landing page; the first screen should be the usable short-link demo.
- Do not add a large frontend framework or state-management library unless the implementation clearly needs it.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `src/ShortenLink.Web/src/main.tsx`
- `src/ShortenLink.Web/src/styles.css`
- `src/ShortenLink.Web/src/features/short-links/`
- `src/ShortenLink.Web/src/shared/`
- `src/ShortenLink.Web/vite.config.ts`
- `src/ShortenLink.Web/package.json` only if frontend dependencies or scripts need to change
- `README.md` only if frontend run/build or API proxy instructions change

## Acceptance Criteria

- The first rendered screen is the usable create-short-link experience, not placeholder or marketing copy.
- A user can submit a valid long URL with optional alias and optional expiry to create a short link through `POST /api/short-links`.
- The UI shows the returned short URL, original URL, code, and created timestamp after a successful create.
- The UI exposes a copy action for the short URL and clearly reflects copied/error states.
- Duplicate alias, invalid URL, invalid alias, and generic API errors are displayed with user-friendly messages based on the backend error payload.
- A user can inspect link details by code using `GET /api/short-links/{code}`.
- Detail view shows original URL, short code, created timestamp, optional expiry, and active/deactivated state.
- A user can deactivate an existing link through `DELETE /api/short-links/{code}` and the detail/result UI updates accordingly.
- `/not-found` and unknown frontend routes render a friendly fallback view with a path back to the create flow.
- Frontend types align with the API DTOs from `001_006`.
- Frontend code is feature-scoped and keeps API calls outside React components where practical.
- `ShortenLink.Web` builds successfully with the repository's frontend build command, or the task explicitly records why dependency installation/build verification was unavailable.

## Foundation for Next Step

This step completes the browser-facing Phase 1 product flow. The next step can verify and close Phase 001 with README/run guidance and an end-to-end smoke path, or move to Phase 002 if all Phase 1 done criteria are satisfied.

## Implementation Notes

Prefer a compact feature structure under `src/ShortenLink.Web/src/features/short-links/` with `api`, `components`, `pages`, and `types.ts` if the app grows beyond a single file. Keep the UI operational and task-focused: dense enough to create, inspect, copy, and deactivate links quickly.

Use the existing React + Vite stack. If local API calls need a proxy, prefer configuring Vite rather than adding CORS or host-specific frontend behavior in the backend.

## Verification

Run the frontend build:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

If frontend dependencies are missing, install or restore them only when the environment allows it; otherwise report the skipped build honestly.

If API or backend configuration changes are required, also run:

```powershell
dotnet build ShortenLink.slnx
dotnet test ShortenLink.slnx
```

When dev servers are running, smoke-check:

- Create a short link from the React form.
- Copy the returned short URL.
- Open link details by code.
- Deactivate the link.
- Visit `/not-found` or an unknown frontend route.

## Done Notes

Completed on 2026-07-12.

Implemented:

- Replaced the placeholder React scaffold with a real Phase 1 demo shell for create, result, detail lookup, deactivate, and fallback flows.
- Added feature-scoped frontend modules under `src/ShortenLink.Web/src/features/short-links/` for API access, DTO types, pages, and components.
- Added a lightweight browser-history router for `/`, `/links/{code}`, and `/not-found` without introducing a full routing dependency.
- Added create flow UX with local form validation, backend error mapping, result display, copy-to-clipboard action, and jump-to-details behavior.
- Added detail flow UX backed by `GET /api/short-links/{code}` and deactivate behavior backed by `DELETE /api/short-links/{code}`.
- Added Vite API proxy defaults for local development and updated development config so returned short URLs use the actual API HTTPS port.
- Updated README with current API endpoints, frontend dev/build instructions, proxy behavior, and corrected direct-service usage docs.

Verification:

- `npm install` completed in `src\ShortenLink.Web`.
- `npm install -D @types/react @types/react-dom` completed in `src\ShortenLink.Web`.
- `npm run build` passed in `src\ShortenLink.Web`.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 41 tests total: 28 core, 5 infrastructure, and 8 API.

Notes:

- I attempted to background-start the API and Vite dev servers for an extra live smoke pass, but this shell session was flaky about keeping those background launches reachable. The compiled frontend, backend build, tests, and updated run instructions are all in place.
