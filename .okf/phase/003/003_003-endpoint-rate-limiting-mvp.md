---
id: 003_003
phase: 003
task: 003
title: Endpoint rate limiting MVP
status: active
created_at: 2026-07-12
completed_at: null
owner: codex
type: feature
priority: high
depends_on:
  - 003_002
tags:
  - rate-limiting
  - endpoints
  - redirects
  - abuse-protection
  - phase-3
---

# 003_003 - Endpoint Rate Limiting MVP

## Step Goal

Add the first production-readiness rate limiting layer for short-link creation and redirect-sensitive endpoints, so basic abuse protection can be enabled through host configuration without changing reusable service contracts or weakening the redirect/cache/analytics behavior delivered in `003_001` and `003_002`.

This step should protect the highest-risk public HTTP paths while keeping the default developer experience predictable and testable.

## Dependency

- `003_001` completed async click analytics for successful redirects.
- `003_002` completed cache lookup before database lookup and invalidation after deactivate.
- Phase 003 still requires rate limiting before moving on to local operational packaging and CI.

## Scope

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

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

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

## Acceptance Criteria

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

## Foundation for Next Step

This step leaves Phase 003 with HTTP-level abuse protection for the most sensitive public paths. The next task can build on the production-readiness foundation by adding local operational packaging, Docker Compose support, or CI validation without reopening endpoint protection design.

## Verification

Run the smallest relevant API test project while iterating, then the full Phase 003 verification set:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
```

## Done Notes

Not started.
