---
id: 003_002
phase: 003
task: 002
title: Cache abstraction and Redis provider MVP
status: done
created_at: 2026-07-12
completed_at: 2026-07-12
owner: codex
type: feature
priority: high
depends_on:
  - 003_001
tags:
  - cache
  - redis
  - redirects
  - invalidation
  - phase-3
---

# 003_002 - Cache Abstraction And Redis Provider MVP

## Step Goal

Add the first reusable short-link cache capability so successful redirect lookups can avoid the database on cache hits, while delete/deactivate operations invalidate cached entries and Redis can be selected by configuration.

This step should build directly on the verified redirect and analytics path from `003_001` without changing the public API contract or requiring Redis for the default local developer flow.

## Dependency

- `003_001` completed async click analytics for successful redirects and preserved the existing redirect contract.
- Phase 002 completed provider selection by configuration, so this task should follow the same pattern for cache provider selection.
- `src/ShortenLink.Api/appsettings.json` already contains an early `ShortenLink:Cache` configuration shape, but no cache abstraction or provider wiring exists yet.

## Scope

In:

- Add a reusable cache abstraction for resolved short-link redirect data behind the library boundary.
- Add a disabled/no-op cache path so existing consumers keep current behavior by default.
- Add an in-memory cache provider for local/test behavior where useful.
- Add Redis provider wiring that can be enabled by configuration without changing application code.
- Use cache lookup before database lookup for successful redirect resolution when caching is enabled.
- Store only redirect-safe data needed to return the original URL and enforce active/expiration behavior.
- Invalidate cached entries when a short link is deleted/deactivated.
- Keep cache and analytics behavior compatible when both are enabled.
- Add focused tests for cache hit/miss behavior, invalidation after deactivate, default disabled behavior, and provider-selection wiring.
- Update README configuration notes if settings or package dependencies are added.

Out:

- Do not add rate limiting, Docker Compose, GitHub Actions CI, dashboards, analytics aggregation, authentication, or tenant-specific cache partitioning in this task.
- Do not require Redis to be reachable for the default test suite.
- Do not cache failed lookups, inactive links, expired links, or error responses unless a later task explicitly designs negative caching.
- Do not redesign click analytics from `003_001` unless cache integration exposes a concrete defect.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_002-cache-abstraction-and-redis-provider-mvp.md`
- `README.md`
- `src/ShortenLink.Core/`
- `src/ShortenLink.Infrastructure/`
- `src/ShortenLink.AspNetCore/`
- `src/ShortenLink.Api/appsettings.json`
- `tests/ShortenLink.Core.Tests/`
- `tests/ShortenLink.Infrastructure.Tests/`
- `tests/ShortenLink.Api.Tests/`

## Acceptance Criteria

- Cache behavior is disabled by default and current create/detail/deactivate/redirect behavior remains unchanged.
- When cache is enabled, successful redirect resolution checks cache before database lookup.
- A cache hit can return the original URL without needing a database read for the redirect path.
- Deactivate/delete invalidates the affected cache entry so a previously cached active link no longer redirects after deactivation.
- Redis can be selected through configuration and the same host code path remains provider-agnostic.
- Cache implementation stays behind reusable `Core`/`Infrastructure`/`AspNetCore` boundaries and does not leak into the demo API business logic.
- Tests cover disabled cache behavior, enabled cache hit/miss behavior, invalidation after deactivate, and provider-selection wiring without requiring a live Redis server.
- Phase verification passes with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

## Foundation for Next Step

This step leaves Phase 003 with a cache abstraction, a local cache path, Redis provider selection, and invalidation semantics. The next task can build on that operational foundation by adding rate limiting or local stack packaging without reopening redirect resolution design.

## Verification

Run the smallest relevant test project while iterating, then the full Phase 003 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If a Redis instance is available, optionally smoke-check the demo host with cache enabled and Redis selected, but do not require that external dependency for task completion.

## Done Notes

Completed on 2026-07-12.

Implemented:

- Added reusable `IShortLinkCache` and disabled cache defaults in `ShortenLink.Core`.
- Updated `ShortLinkService.ResolveAsync` to check cache before repository lookup, set cache after successful database resolution, and remove stale cache entries on expired/inactive cached links.
- Updated `ShortLinkService.DeactivateAsync` to invalidate the affected cache entry after persistence update.
- Added ASP.NET Core cache options, memory-cache provider wiring, Redis provider wiring, cache validation, and distributed-cache backed short-link serialization.
- Added Redis cache package dependency to `ShortenLink.AspNetCore` while keeping the demo API non-packable.
- Updated demo host config and README with cache provider, Redis connection, and TTL settings.
- Added tests for cache hit/miss behavior, repository bypass on cache hit, invalidation after deactivate, default disabled cache behavior, memory provider wiring, Redis provider wiring, and invalid cache configuration.

Verification:

- `dotnet build ShortenLink.slnx --verbosity minimal`
- `dotnet test ShortenLink.slnx --verbosity minimal`
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

Notes:

- Redis provider selection is verified through DI without requiring a live Redis server.
- `dotnet pack` still emits the expected informational warning that `ShortenLink.Api` is not packable, while the reusable packages are produced successfully.
