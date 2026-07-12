---
id: 001_008
phase: 001
task: 008
title: Phase 001 end-to-end smoke closure
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: validation
priority: high
depends_on:
  - 001_007
tags:
  - smoke-test
  - phase-1
  - closure
  - api
  - react
---

# 001_008 - Phase 001 End-To-End Smoke Closure

## Step Goal

Run the completed Phase 001 API and React demo together against the real local stack, verify the create/detail/deactivate/redirect/fallback journey end to end, and close Phase 001 only if every Phase Done Criterion is supported by current build, test, package, documentation, and smoke evidence.

This task should prove the Phase 001 slice is not merely compiled code: it should be usable from the browser and ready to hand off as the foundation for Phase 002 provider-toggle work.

## Dependency

- `001_007` completed the React create/result/detail/deactivate/fallback demo flow and verified the production frontend build.
- `001_006` completed reusable ASP.NET Core DI, endpoint mapping, create/detail/deactivate/redirect API behavior, and API integration tests.
- `001_005` completed SQLite-backed persistence and integration tests.
- `001_004` completed reusable core service contracts, validation, code generation, and core tests.
- `001_002` documented package build and consumer usage guidance.

## Scope

In:

- Start or otherwise exercise the demo API with SQLite default configuration.
- Start or otherwise exercise the React/Vite demo against the API through the documented proxy/config path.
- Smoke-check the browser-level or HTTP-backed user journey:
  - create a short link from the React form or equivalent frontend request path
  - inspect link details by code
  - verify the short URL redirects while active
  - deactivate the link
  - verify detail state after deactivation
  - verify unknown code/fallback behavior
- Re-run required verification for Phase 001 closure:
  - frontend build
  - backend build
  - backend tests
  - package build
- Review README instructions for local SQLite run, frontend run/build, endpoints, and package consumption against the current code.
- Update docs or small config gaps found during smoke only when they are necessary to make the verified Phase 001 flow reproducible.
- Update `PHASE_SUMMARY.md` to either close Phase 001 or document the exact remaining blocker.

Out:

- Do not add new product features beyond closure fixes.
- Do not start Phase 002 implementation in this task.
- Do not add PostgreSQL, Redis, analytics, rate limiting, Docker Compose, authentication, accounts, or dashboards.
- Do not refactor completed library/API/frontend architecture unless the smoke path exposes a real closure blocker.
- Do not mask failing smoke behavior by changing acceptance criteria; record the blocker if it cannot be fixed inside this closure slice.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/001/PHASE_SUMMARY.md`
- `.okf/phase/001/001_008-phase-001-end-to-end-smoke-closure.md`
- `README.md`
- `src/ShortenLink.Api/appsettings.Development.json`
- `src/ShortenLink.Web/vite.config.ts`
- `src/ShortenLink.Web/src/features/short-links/`

Only edit source files if smoke verification exposes a real issue that prevents Phase 001 closure.

## Acceptance Criteria

- The API can run locally with default SQLite configuration using the documented command path.
- The React demo can run locally against the API using the documented frontend command path and proxy/config behavior.
- The create flow produces a persisted short link and displays a usable short URL.
- `GET /api/short-links/{code}` returns detail for the created code.
- `GET /{code}` redirects active links to the original URL.
- `DELETE /api/short-links/{code}` deactivates the link, and a follow-up detail check reflects the inactive state.
- Unknown code behavior follows the configured fallback path and is consistent with README guidance.
- `npm run build` passes in `src/ShortenLink.Web`.
- `dotnet build ShortenLink.slnx` passes.
- `dotnet test ShortenLink.slnx` passes.
- `dotnet pack ShortenLink.slnx -c Release` passes or records a concrete package-boundary blocker.
- README run, endpoint, SQLite, frontend, and package-consumption guidance matches the current implementation.
- `PHASE_SUMMARY.md` is updated in the same pass:
  - if all Phase Done Criteria are satisfied, mark Phase 001 complete and point to Phase 002 / `002_001`;
  - if not, keep Phase 001 active and document the remaining blocker precisely.

## Foundation for Next Step

This step leaves Phase 001 with closure evidence instead of inferred readiness. If the smoke path passes, Phase 002 can begin PostgreSQL provider-toggle work on top of a verified SQLite/API/Web/package baseline. If it fails, the next step can address a single documented blocker rather than rediscovering the end-to-end gap.

## Implementation Notes

Prefer the smallest reliable smoke route available in this environment. Browser-level verification is ideal, but an HTTP-backed smoke script is acceptable when it exercises the same API/frontend configuration path and records exactly what could not be browser-verified.

Keep temporary logs and local databases out of the committed task scope. If previous background dev-server attempts left locked `.tmp` logs, inspect and clean only the processes/files that belong to this repo's smoke attempt.

## Verification

Run:

```powershell
cd .\src\ShortenLink.Web
npm run build
```

Run from the repository root:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

When dev servers are running, smoke-check:

- Create a short link from the React form.
- Copy or use the returned short URL.
- Open link details by code.
- Open the short URL and confirm redirect.
- Deactivate the link.
- Re-open details and confirm inactive state.
- Visit an unknown short code or `/not-found` and confirm fallback behavior.

## Done Notes

Completed on 2026-07-12.

Implemented:

- Verified the full Phase 001 create, copy, detail, redirect, deactivate, and fallback journey against the real local API and Vite frontend stack.
- Updated local run guidance in `README.md` to use the API `https` launch profile so returned short URLs and the frontend proxy target both work together during local runs.
- Updated development redirect fallback config so unknown short codes land on the React `/not-found` page instead of looping on the API host when the frontend runs separately.
- Expanded API integration coverage so frontend fallback configuration now supports both root-relative paths and absolute HTTP/HTTPS URLs.

Verification:

- `npm run build` passed in `src\ShortenLink.Web`.
- `dotnet build ShortenLink.slnx --verbosity minimal` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --verbosity minimal` passed with 42 total tests: 28 core, 5 infrastructure, and 9 API.
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal` passed for the reusable packages. The demo API remained intentionally non-packable and emitted the expected informational warning from NuGet pack targets.
- Browser-level smoke passed through a local Edge Playwright run:
  - create from the React form
  - copy returned short URL
  - open details and confirm active state
  - open the short URL and confirm redirect to the original destination
  - deactivate the link and confirm inactive detail state
  - open an unknown short code and confirm frontend fallback
  - open an unknown frontend route and confirm fallback page
