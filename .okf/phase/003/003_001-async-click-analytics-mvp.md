---
id: 003_001
phase: 003
task: 001
title: Async click analytics MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: feature
priority: high
depends_on:
  - 002_002
tags:
  - analytics
  - redirects
  - background-worker
  - persistence
  - phase-3
---

# 003_001 - Async Click Analytics MVP

## Step Goal

Add the first reusable click analytics capability for short-link redirects, including persistence and an async recording path, so redirect behavior remains stable while click tracking becomes available for later operational and reporting work.

This step should turn Phase 003 from production-readiness planning into a concrete analytics foundation without introducing cache, Redis, rate limiting, or dashboard scope yet.

## Dependency

- `001_008` completed the SQLite-backed local host and API smoke baseline for create/detail/delete-deactivate/redirect behavior.
- `002_002` completed the PostgreSQL provider-toggle phase and left the demo host documented for provider-specific verification.

## Scope

In:

- Add a library-owned click record model or equivalent persistence shape for redirect analytics.
- Add analytics abstractions behind the reusable package boundary, such as a recorder and repository contract.
- Record short code, clicked UTC timestamp, remote IP address, user agent, and referrer where the request provides them.
- Add an async queue/background-worker path so redirects do not synchronously depend on slow analytics persistence when analytics is enabled.
- Keep analytics configurable through options with safe defaults for existing consumers.
- Wire the ASP.NET Core redirect endpoint to record analytics without changing the public redirect contract.
- Add provider-compatible EF Core mapping and indexes for click records where practical.
- Add focused tests for disabled analytics, enqueue/record behavior, persistence mapping, and redirect no-regression.
- Update README configuration notes if new settings are introduced.

Out:

- Do not build analytics dashboards, charts, exports, or aggregation reports in this task.
- Do not add cache, Redis, rate limiting, Docker Compose, CI, authentication, or authorization.
- Do not redesign short-link creation, deletion, or provider selection contracts.
- Do not require analytics persistence to succeed before a redirect can be returned.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_001-async-click-analytics-mvp.md`
- `README.md`
- `src/ShortenLink.Core/`
- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.AspNetCore/`
- `src/ShortenLink.Api/`
- `tests/ShortenLink.Core.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/`
- `tests/ShortenLink.Api.Tests/`

## Acceptance Criteria

- Redirecting an active short link still returns the original URL with the same public API behavior as Phase 001.
- Click analytics can be enabled through configuration while remaining behind the reusable library and ASP.NET Core integration boundary.
- When async analytics is enabled, the redirect path enqueues or schedules click recording instead of waiting on analytics persistence before responding.
- Click records persist short code, clicked UTC timestamp, remote IP address, user agent, and referrer with nullable handling where request data is unavailable.
- EF Core persistence supports SQLite and PostgreSQL-compatible schema behavior for click records where practical.
- Tests cover disabled analytics no-op behavior, enabled analytics recording, redirect no-regression, and persistence/model behavior.
- Phase verification passes with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

## Foundation for Next Step

This step leaves Phase 003 with a reusable analytics abstraction, durable click persistence, and an async write path. The next task can build on that production-readiness baseline by adding cache/Redis behavior or tightening endpoint protection without reopening redirect analytics design.

## Verification

Run the smallest relevant test project while iterating, then the full Phase 003 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If a PostgreSQL instance is available, also run the existing PostgreSQL host smoke path after analytics mapping is added.

## Done Notes

Completed on 2026-07-12.

Implemented:

- Added reusable click analytics contracts in `ShortenLink.Core` for click records, recorder input, and persistence abstraction.
- Added EF Core click persistence in `ShortenLink.Infrastructure` with provider-compatible schema and indexes for SQLite and PostgreSQL model generation.
- Added ASP.NET Core analytics options, async channel-backed recorder, background persistence worker, and disabled/synchronous fallbacks in the host integration layer.
- Wired successful redirect handling to capture short code, clicked timestamp, remote IP address, user agent, and referrer without changing the public redirect contract.
- Added infrastructure and API tests covering click persistence, redirect no-regression, analytics enabled recording, and analytics disabled no-op behavior.
- Updated demo host configuration and README with opt-in analytics settings and queue behavior.

Verification:

- `dotnet build ShortenLink.slnx --verbosity minimal`
- `dotnet test ShortenLink.slnx --verbosity minimal`
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

Notes:

- Async analytics enablement is now resolved from runtime options so test and host overrides activate the correct recorder path consistently.
- `dotnet pack` still emits the expected informational warning that `ShortenLink.Api` is not packable, while the reusable packages are produced successfully.
