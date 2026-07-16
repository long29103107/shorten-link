---
phase: 003
title: Production Readiness
status: done
created_at: 2026-07-09
updated_at: 2026-07-12
current_task: null
task_count: 5
done_count: 5
depends_on:
  - 002
---

# Phase 003 Summary

## Phase Goal

Strengthen the Shorten Link product for real-world usage with analytics, cache, async processing, operational packaging, rate limiting, and CI while preserving the reusable package boundary.

## Phase Done Criteria

- Click tracking abstraction and persistence are implemented.
- Redirect does not synchronously depend on slow analytics persistence.
- Cache abstraction is implemented.
- Redis can be enabled by configuration.
- Cache lookup happens before database lookup and cache entries are invalidated on delete/deactivate.
- Rate limiting protects create and redirect-sensitive endpoints.
- Docker Compose supports the local operational stack where appropriate.
- GitHub Actions CI validates build and tests.
- Tests cover cache behavior, analytics behavior, and endpoint contracts.

## Scope

In:

- Analytics click tracking.
- Async worker/channel path.
- Cache abstraction and Redis provider.
- Rate limiting.
- Docker Compose.
- GitHub Actions CI.
- Expanded tests.

Out:

- SaaS billing.
- Tenant management.
- Authentication/authorization unless a specific task adds it.
- Advanced analytics dashboards.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 003_001 | Async click analytics MVP | done | 2026-07-12 |
| 003_002 | Cache abstraction and Redis provider MVP | done | 2026-07-12 |
| 003_003 | Endpoint rate limiting MVP | done | 2026-07-12 |
| 003_004 | Local operational stack and Docker Compose MVP | done | 2026-07-12 |
| 003_005 | GitHub Actions CI validation MVP | done | 2026-07-12 |

## Current Task

No active task. Phase 003 is complete.

## Completed Notes

- `003_001` completed on 2026-07-12. Added reusable click analytics contracts, EF-backed click persistence, ASP.NET Core async recorder/worker integration, redirect analytics capture, and focused infrastructure/API coverage while keeping redirect responses independent from slow persistence.
- `003_002` completed on 2026-07-12. Added reusable short-link cache contracts, disabled defaults, memory and Redis provider wiring, distributed cache serialization, redirect cache lookup before database lookup, deactivate invalidation, README configuration notes, and tests covering cache behavior and provider selection without requiring a live Redis server.
- `003_003` completed on 2026-07-12. Added configurable ASP.NET Core fixed-window rate limiting for create and redirect endpoints, kept default behavior disabled for compatibility, ensured rejected redirects do not reach cache/database/analytics work, updated README/demo config, and added API coverage for accepted, over-limit, disabled, and invalid-option behavior.
- `003_004` completed on 2026-07-12. Added a multi-stage API Dockerfile, root Docker Compose stack for API + PostgreSQL + Redis, a repeatable compose smoke script, and README operational guidance while preserving the default SQLite developer path outside Docker. Repo verification passed with build, test, and pack; live compose smoke remained blocked by unavailable Docker daemon access to `//./pipe/docker_engine` in the current shell.
- `003_005` completed on 2026-07-12. Added GitHub Actions CI for push and pull request validation with .NET 10 restore, build, test, and pack steps. The workflow intentionally avoids Docker, PostgreSQL, Redis, secrets, publishing, and deployment while preserving the same local command surface verified for Phase 003.

## Next Task Proposal

Phase 003 is complete. Next, decide whether to open a new Phase 004 for release and consumer hardening, or finalize the current product definition.

## Task Notes

Historical and active task detail is compacted here so each phase stays in one markdown file.

### 003_001 - Async Click Analytics MVP

Source before compaction: `003_001-async-click-analytics-mvp.md`

#### Step Goal

Add the first reusable click analytics capability for short-link redirects, including persistence and an async recording path, so redirect behavior remains stable while click tracking becomes available for later operational and reporting work.

This step should turn Phase 003 from production-readiness planning into a concrete analytics foundation without introducing cache, Redis, rate limiting, or dashboard scope yet.

#### Dependency

- `001_008` completed the SQLite-backed local host and API smoke baseline for create/detail/delete-deactivate/redirect behavior.
- `002_002` completed the PostgreSQL provider-toggle phase and left the demo host documented for provider-specific verification.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

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

#### Acceptance Criteria

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

#### Foundation for Next Step

This step leaves Phase 003 with a reusable analytics abstraction, durable click persistence, and an async write path. The next task can build on that production-readiness baseline by adding cache/Redis behavior or tightening endpoint protection without reopening redirect analytics design.

#### Verification

Run the smallest relevant test project while iterating, then the full Phase 003 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If a PostgreSQL instance is available, also run the existing PostgreSQL host smoke path after analytics mapping is added.

#### Done Notes

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

### 003_002 - Cache Abstraction And Redis Provider MVP

Source before compaction: `003_002-cache-abstraction-and-redis-provider-mvp.md`

#### Step Goal

Add the first reusable short-link cache capability so successful redirect lookups can avoid the database on cache hits, while delete/deactivate operations invalidate cached entries and Redis can be selected by configuration.

This step should build directly on the verified redirect and analytics path from `003_001` without changing the public API contract or requiring Redis for the default local developer flow.

#### Dependency

- `003_001` completed async click analytics for successful redirects and preserved the existing redirect contract.
- Phase 002 completed provider selection by configuration, so this task should follow the same pattern for cache provider selection.
- `src/ShortenLink.Api/appsettings.json` already contains an early `ShortenLink:Cache` configuration shape, but no cache abstraction or provider wiring exists yet.

#### Scope

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

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

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

#### Acceptance Criteria

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

#### Foundation for Next Step

This step leaves Phase 003 with a cache abstraction, a local cache path, Redis provider selection, and invalidation semantics. The next task can build on that operational foundation by adding rate limiting or local stack packaging without reopening redirect resolution design.

#### Verification

Run the smallest relevant test project while iterating, then the full Phase 003 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If a Redis instance is available, optionally smoke-check the demo host with cache enabled and Redis selected, but do not require that external dependency for task completion.

#### Done Notes

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

### 003_003 - Endpoint Rate Limiting MVP

Source before compaction: `003_003-endpoint-rate-limiting-mvp.md`

#### Step Goal

Add the first production-readiness rate limiting layer for short-link creation and redirect-sensitive endpoints, so basic abuse protection can be enabled through host configuration without changing reusable service contracts or weakening the redirect/cache/analytics behavior delivered in `003_001` and `003_002`.

This step should protect the highest-risk public HTTP paths while keeping the default developer experience predictable and testable.

#### Dependency

- `003_001` completed async click analytics for successful redirects.
- `003_002` completed cache lookup before database lookup and invalidation after deactivate.
- Phase 003 still requires rate limiting before moving on to local operational packaging and CI.

#### Scope

In:

- Add configurable ASP.NET Core rate limiting for `POST /api/short-links`.
- Add configurable rate limiting for redirect-sensitive paths such as `GET /{code}`.
- Keep detail and deactivate endpoint behavior unchanged unless a minimal shared policy is required by the framework wiring.
- Add options under `ShortenLink:RateLimiting` with safe defaults and validation.
- Wire rate limiting in the reusable `ShortenLink.AspNetCore` integration layer, not in duplicated demo API business logic.
- Ensure cache and analytics still run only after a redirect request is accepted by rate limiting.
- Add endpoint tests for accepted requests, rejected over-limit requests, disabled default behavior, and options/provider wiring.
- Update README and demo host configuration with the rate limiting settings.

Out:

- Do not add authentication, per-user identity quotas, tenant-specific policies, dashboards, Docker Compose, GitHub Actions CI, or distributed rate limit coordination in this task.
- Do not redesign cache or analytics behavior unless endpoint integration exposes a concrete defect.
- Do not require Redis or any external infrastructure for rate limit tests.
- Do not change public response contracts for ordinary successful create/detail/deactivate/redirect flows.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_003-endpoint-rate-limiting-mvp.md`
- `README.md`
- `src/ShortenLink.AspNetCore/ShortenLinkOptions.cs`
- `src/ShortenLink.AspNetCore/ShortenLinkServiceCollectionExtensions.cs`
- `src/ShortenLink.AspNetCore/ShortenLinkEndpointRouteBuilderExtensions.cs`
- `src/ShortenLink.Api/Program.cs`
- `src/ShortenLink.Api/appsettings.json`
- `tests/ShortenLink.Api.Tests/`

#### Acceptance Criteria

- Rate limiting is configurable through `ShortenLink:RateLimiting` and can be disabled for compatibility.
- Create requests can be limited independently from redirect requests.
- Redirect requests are rate limited before cache lookup, database lookup, and analytics recording.
- Accepted requests keep the existing public behavior for create/detail/deactivate/redirect.
- Over-limit requests return the framework-appropriate rate-limit response, including HTTP `429`.
- Tests cover disabled/default behavior, enabled create limit behavior, enabled redirect limit behavior, and invalid options.
- Phase verification passes with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

#### Foundation for Next Step

This step leaves Phase 003 with HTTP-level abuse protection for the most sensitive public paths. The next task can build on the production-readiness foundation by adding local operational packaging, Docker Compose support, or CI validation without reopening endpoint protection design.

#### Verification

Run the smallest relevant API test project while iterating, then the full Phase 003 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

#### Done Notes

Completed on 2026-07-12.

Implemented:

- Added `ShortenLink:RateLimiting` options with independent create and redirect fixed-window policies.
- Added options validation for positive permit/window values and non-negative queues.
- Wired ASP.NET Core rate limiting services in `ShortenLink.AspNetCore` and enabled middleware through the demo host.
- Applied the create policy to `POST /api/short-links` and the redirect policy to `GET /{code}` only when rate limiting is enabled.
- Preserved default disabled behavior for compatibility.
- Ensured redirect requests are rejected by rate limiting before cache lookup, database lookup, or analytics recording.
- Updated demo host configuration and README with rate limiting settings.
- Added API tests for disabled behavior, create over-limit responses, redirect over-limit responses before extra analytics recording, option binding, and invalid options.

Verification:

- `dotnet build ShortenLink.slnx --verbosity minimal`
- `dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal`
- `dotnet test ShortenLink.slnx --verbosity minimal`
- `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

Notes:

- Over-limit requests return HTTP `429`.
- `dotnet pack` still emits the expected informational warning that `ShortenLink.Api` is not packable, while the reusable packages are produced successfully.

### 003_004 - Local Operational Stack And Docker Compose MVP

Source before compaction: `003_004-local-operational-stack-and-docker-compose-mvp.md`

#### Step Goal

Add a reproducible local operational stack for the demo API and its production-readiness dependencies, so developers can run the Shorten Link host with PostgreSQL, Redis-backed cache, analytics, and rate limiting settings from documented Docker Compose commands.

This step should convert the verified configuration toggles from Phase 002 and Phase 003 into a practical local stack without making Docker mandatory for the default SQLite developer path.

#### Dependency

- `002_002` completed PostgreSQL setup guidance and host smoke script behavior.
- `003_001` completed async click analytics and persistence.
- `003_002` completed cache abstraction with memory and Redis provider selection.
- `003_003` completed endpoint rate limiting with safe disabled defaults and config-driven enablement.

#### Scope

In:

- Add Dockerfile support for the demo API if it is not already present.
- Add Docker Compose support for the local operational stack.
- Include PostgreSQL as the configured database provider for the composed API.
- Include Redis as the configured cache provider for the composed API.
- Configure analytics and rate limiting through environment variables or compose configuration.
- Preserve the default local non-Docker SQLite path.
- Document compose startup, shutdown, environment variables, expected ports, and smoke commands in `README.md`.
- Add or update scripts only where they materially improve repeatable local smoke verification.
- Verify compose files and Docker artifacts structurally where the local environment allows, and document any external Docker daemon blocker precisely.

Out:

- Do not add GitHub Actions CI in this task.
- Do not add production deployment manifests, Kubernetes, cloud infrastructure, TLS termination, authentication, or observability dashboards.
- Do not require Docker for unit/integration tests or the default developer flow.
- Do not change reusable service, repository, cache, analytics, or rate limiting contracts unless Docker smoke exposes a concrete configuration defect.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_004-local-operational-stack-and-docker-compose-mvp.md`
- `README.md`
- `docker-compose.yml` or `compose.yml`
- `src/ShortenLink.Api/Dockerfile`
- `src/ShortenLink.Api/appsettings.json`
- `scripts/`
- `tests/ShortenLink.Api.Tests/`

#### Acceptance Criteria

- A Docker Compose file defines the demo API plus PostgreSQL and Redis services.
- The composed API is configured to use PostgreSQL by configuration only.
- The composed API can use Redis cache by configuration only.
- Analytics and rate limiting settings are represented in the compose path without changing application code.
- The default SQLite local path remains available outside Docker.
- README documents how to start, stop, and smoke-check the local operational stack.
- Verification includes structural checks for Docker/Compose artifacts and a live compose smoke where the local environment allows.
- If Docker is unavailable, the task records the exact blocker and still verifies the repo-side build/test/pack set.
- Phase verification passes with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

#### Foundation for Next Step

This step leaves Phase 003 with a reproducible local stack for PostgreSQL, Redis, analytics, cache, and rate limiting. The next task can add GitHub Actions CI validation on top of a documented operational shape instead of inventing CI targets from scratch.

#### Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

When Docker is available, also run the smallest practical compose validation and smoke path, such as:

```powershell
docker compose config
docker compose up --build
```

Then smoke-check:

- `GET /api/health`
- `POST /api/short-links`
- `GET /{code}`
- `GET /api/short-links/{code}`
- `DELETE /api/short-links/{code}`

#### Done Notes

- Completed on 2026-07-12.
- Added `.dockerignore`, `src/ShortenLink.Api/Dockerfile`, root `compose.yml`, and `scripts/smoke-docker-compose.ps1` so the demo API can run in a local Docker Compose stack with PostgreSQL and Redis while enabling analytics, cache, and rate limiting through configuration only.
- Updated `README.md` with compose startup, shutdown, ports, configuration expectations, smoke commands, and an explicit reminder that the default non-Docker SQLite path remains available.
- Verified repo-side acceptance with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
- Verified Docker artifact structure with `docker compose -f compose.yml config`.
- Live compose smoke was blocked by the local environment because Docker daemon access to `//./pipe/docker_engine` was unavailable from the current shell, and Docker also reported access denied for `C:\Users\LENOVO\.docker\config.json`. The smoke script now fails fast with that exact blocker instead of masking it.

### 003_005 - GitHub Actions CI Validation MVP

Source before compaction: `003_005-github-actions-ci-validation-mvp.md`

#### Step Goal

Add a minimal GitHub Actions CI workflow that validates the reusable Shorten Link package boundary and demo host on push and pull request, so Phase 003 has automated build and test feedback for the production-readiness work completed so far.

This step should make CI mirror the required local repo verification without introducing external infrastructure dependencies such as PostgreSQL, Redis, Docker daemon access, package publishing, secrets, or deployment.

#### Dependency

- `003_001` completed async click analytics and endpoint coverage.
- `003_002` completed cache abstraction, memory provider, Redis provider selection, and related tests.
- `003_003` completed endpoint rate limiting and endpoint tests.
- `003_004` completed the local operational Docker Compose shape and documented Docker daemon blockers separately from repo-side verification.

#### Scope

In:

- Add a GitHub Actions workflow for push and pull request validation.
- Set up the .NET SDK version required by the solution.
- Restore, build, test, and pack the solution or packable projects.
- Keep CI independent from live PostgreSQL, Redis, Docker Compose, and local machine state.
- Use repository-native commands already required by Phase 003 verification.
- Add README or task notes only if needed to document the CI command surface.

Out:

- Do not add package publishing to NuGet.
- Do not add deployment, container registry publishing, release automation, or environment secrets.
- Do not require Docker daemon access in CI for this MVP.
- Do not add database service containers unless an existing test truly requires them.
- Do not change product behavior just to satisfy CI.

#### Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

#### Affected Files

Expected starting points:

- `.okf/phase/003/PHASE_SUMMARY.md`
- `.okf/phase/003/003_005-github-actions-ci-validation-mvp.md`
- `.github/workflows/`
- `README.md`
- `ShortenLink.slnx`

#### Acceptance Criteria

- A GitHub Actions workflow exists under `.github/workflows/`.
- The workflow runs on push and pull request.
- The workflow installs the .NET SDK version needed by `ShortenLink.slnx`.
- The workflow restores dependencies.
- The workflow builds the solution.
- The workflow runs the test suite.
- The workflow verifies package creation for packable reusable projects or the solution pack command used locally.
- The workflow does not require Docker, PostgreSQL, Redis, credentials, or publishing permissions.
- The workflow keeps default SQLite-based tests and config-driven optional providers compatible with CI.
- Phase verification passes locally with:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`

#### Foundation for Next Step

This step should close the remaining Phase 003 CI done criterion. After it is complete, Phase 003 can be evaluated for closure against all done criteria before deciding whether to open the next phase.

#### Verification

Run repo verification:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

If local GitHub Actions tooling such as `act` is available, optionally validate the workflow structure. Do not make `act` mandatory for task completion.

#### Done Notes

- Completed on 2026-07-12.
- Added `.github/workflows/ci.yml` with push and pull request triggers.
- The workflow uses `actions/checkout@v7`, `actions/setup-dotnet@v5`, and .NET `10.0.x`.
- CI restores `ShortenLink.slnx`, builds the solution, runs the test suite, and packs the solution without requiring Docker, PostgreSQL, Redis, credentials, package publishing, or deployment permissions.
- Verified locally with the same command surface:
  - `dotnet restore ShortenLink.slnx --verbosity minimal`
  - `dotnet build ShortenLink.slnx --no-restore --verbosity minimal`
  - `dotnet test ShortenLink.slnx --no-build --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --no-restore --verbosity minimal`
- Verified required phase commands:
  - `dotnet build ShortenLink.slnx --verbosity minimal`
  - `dotnet test ShortenLink.slnx --verbosity minimal`
  - `dotnet pack ShortenLink.slnx -c Release --verbosity minimal`
- `dotnet pack` exits successfully and emits the expected warning for `ShortenLink.Api` because the demo host has `IsPackable=false`; reusable packages are produced for `ShortenLink.Core`, `ShortenLink.Infrastructure`, and `ShortenLink.AspNetCore`.
- Optional local `act` validation was skipped because `act` is not installed in the current shell.


## Scan Rule
Agents must read this file before working on any `003_*` task note. Do not activate Phase 003 until Phase 002 is complete.
