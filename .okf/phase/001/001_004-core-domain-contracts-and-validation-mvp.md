---
id: 001_004
phase: 001
task: 004
title: Core domain contracts and validation MVP
status: done
created_at: 2026-07-11
completed_at: 2026-07-11
owner: codex
type: feature
priority: high
depends_on:
  - 001_003
tags:
  - core-domain
  - contracts
  - validation
  - tests
---

# 001_004 - Core Domain Contracts And Validation MVP

## Step Goal

Implement the first real `ShortenLink.Core` domain surface so the product has reusable models, request/result contracts, service/repository interfaces, Base62 short-code generation, custom alias validation, URL validation, and focused core tests.

This task should make the core library useful on its own while leaving persistence, ASP.NET Core endpoint mapping, and demo UI integration for later Phase 001 tasks.

## Dependency

- `001_001` created the solution, reusable project boundary, package metadata, and placeholder consumer-facing extension points.
- `001_002` documented how consumers should reference and call the package surface.
- `001_003` added a Swagger/OpenAPI surface to the demo API so later endpoints can be inspected as they are implemented.

## Foundation for Next Step

This step establishes the stable domain and service contracts that the SQLite repository, ASP.NET Core DI/endpoint mapping, demo API, and React demo flow can reuse without redefining short-link behavior in host projects.

## Scope

In:

- Add the core `ShortLink` domain model.
- Add create/detail/resolve request and result DTOs needed by the first service contract.
- Add `IShortLinkService`, `IShortCodeGenerator`, and `IShortLinkRepository` contracts.
- Implement a default Base62 short-code generator with default length 7.
- Implement custom alias validation for letters, numbers, `_`, and `-`.
- Implement URL validation that rejects empty, malformed, and non-HTTP/HTTPS URLs.
- Add focused `ShortenLink.Core.Tests` coverage for generator behavior, alias validation, URL validation, and core service edge cases that can be tested without persistence.

Out:

- Do not implement EF Core or SQLite persistence in this task.
- Do not implement ASP.NET Core endpoint mapping beyond any compile-only contract adjustments required by the new interfaces.
- Do not implement demo API create/detail/delete/redirect endpoints yet.
- Do not implement React UI flows yet.
- Do not add PostgreSQL, Redis, analytics worker, rate limiting, Docker Compose, or CI.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

Expected starting points:

- `src/ShortenLink.Core/`
- `tests/ShortenLink.Core.Tests/`
- `src/ShortenLink.AspNetCore/` only if placeholders must compile against the new contracts
- `README.md` only if public contract names differ from existing consumer examples

## Acceptance Criteria

- `ShortenLink.Core` exposes a reusable `ShortLink` model with code, original URL, active state, created timestamp, and optional expiry information.
- Core request/result DTOs are available for creating, resolving, and inspecting short links without depending on demo API types.
- `IShortLinkService`, `IShortCodeGenerator`, and `IShortLinkRepository` are defined in the core package.
- The default short-code generator uses Base62 characters and defaults to length 7.
- Custom aliases allow only letters, numbers, `_`, and `-`.
- Empty, malformed, and non-HTTP/HTTPS URLs are rejected by reusable core validation.
- Core tests cover generator shape, URL validation, alias validation, and service-level validation behavior that does not need a database.
- The solution builds after the new contracts are added.
- Package boundaries remain intact: `ShortenLink.Core` does not reference demo API/Web or infrastructure.

## Implementation Notes

Keep validation reusable and host-agnostic. Prefer small core types and interfaces that later persistence/API tasks can compose rather than controller-specific models.

If a full service implementation needs persistence to be meaningful, define the service contract now and keep implementation limited to validation-only behavior or a small in-memory test seam only when it directly supports the acceptance criteria.

## Verification

Run the smallest relevant checks:

```powershell
dotnet test ShortenLink.slnx --filter ShortenLink.Core.Tests
```

If test filtering is not supported by the current project shape, run:

```powershell
dotnet test ShortenLink.slnx
```

Also verify the package boundary with:

```powershell
dotnet build ShortenLink.slnx
```

## Done Notes

Completed on 2026-07-11.

Implemented:

- Added the reusable `ShortLink` domain model with code, original URL, active state, created timestamp, optional expiry, expiry checks, and deactivate behavior.
- Added core create/detail/resolve/deactivate request/result contracts and `ShortLinkErrorCodes`.
- Added `IShortLinkService`, `IShortCodeGenerator`, and `IShortLinkRepository`.
- Implemented `Base62ShortCodeGenerator` with default length 7 and Base62 alphabet.
- Implemented reusable custom alias validation for letters, numbers, `_`, and `-`.
- Implemented reusable URL validation for absolute HTTP/HTTPS URLs.
- Implemented `ShortLinkService` validation, duplicate custom alias handling, generated-code retry, resolve detail, and deactivate behavior against the repository contract.
- Added xUnit-based core tests for generator shape, alias validation, URL validation, duplicate alias handling, generated-code retry, expired link behavior, and deactivate behavior.
- Added `ShortenLink.Core.Tests` back into `ShortenLink.slnx` so root-level verification runs the core test suite.

Verification:

- `dotnet test tests\ShortenLink.Core.Tests\ShortenLink.Core.Tests.csproj --verbosity minimal` passed with 28 tests.
- `dotnet build ShortenLink.slnx --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test ShortenLink.slnx --no-restore --verbosity minimal` passed with 28 tests.
- `dotnet pack ShortenLink.slnx -c Release --no-restore` passed. It reported the expected `IsPackable=false` warning for `ShortenLink.Api`, which is the demo host and should not produce a reusable package.
