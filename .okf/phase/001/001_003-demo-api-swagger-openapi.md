---
id: 001_003
phase: 001
task: 003
title: Demo API Swagger/OpenAPI
status: done
created_at: 2026-07-09
completed_at: 2026-07-09
owner: codex
type: feature
priority: medium
depends_on:
  - 001_002
tags:
  - swagger
  - openapi
  - demo-api
  - developer-experience
---

# 001_003 - Demo API Swagger/OpenAPI

## Step Goal

Add Swagger/OpenAPI to the demo API so developers can inspect and try the available HTTP surface while Phase 001 endpoints are built out.

This is a demo-host feature. It must not move Swagger dependencies into the reusable library packages.

## Dependency

- `001_002` documented the library package/consumer flow and left Phase 001 ready for the next demo/API developer-experience slice.

## Foundation for Next Step

This step gives later API endpoint tasks an immediate documentation surface. As create/detail/delete/redirect endpoints are implemented, they should show up in Swagger without adding a separate documentation pass.

## Scope

In:

- Add Swagger/OpenAPI package support to `ShortenLink.Api`.
- Register Swagger services in the demo API host.
- Enable Swagger UI in development.
- Add a stable endpoint name for the current health endpoint so it appears cleanly in generated docs.
- Document how to open Swagger locally.

Out:

- Do not add Swagger dependencies to `ShortenLink.Core`, `ShortenLink.Infrastructure`, or `ShortenLink.AspNetCore`.
- Do not implement short-link endpoints in this task.
- Do not publish OpenAPI artifacts.
- Do not add Scalar, ReDoc, Docker, or CI.

## Acceptance Criteria

- `ShortenLink.Api` builds with Swagger enabled.
- `ShortenLink.Api` has a package reference for Swagger/OpenAPI support.
- `Program.cs` registers Swagger services before `builder.Build()`.
- `Program.cs` maps Swagger UI in development.
- README documents the local Swagger URL.
- Reusable library projects remain free of Swagger package references.

## Affected Files

- `src/ShortenLink.Api/ShortenLink.Api.csproj`
- `src/ShortenLink.Api/Program.cs`
- `README.md`

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`

## Implementation Notes

Keep Swagger in the demo API host. The library package should stay focused on reusable short-link behavior and ASP.NET Core integration extensions.

## Verification

```powershell
dotnet build ShortenLink.slnx
```

## Done Notes

Completed on 2026-07-09.

Implemented:

- Added `Swashbuckle.AspNetCore` to `ShortenLink.Api`.
- Registered `AddEndpointsApiExplorer()` and `AddSwaggerGen()`.
- Enabled `UseSwagger()` and `UseSwaggerUI()` in development.
- Added a stable endpoint name to `/api/health`.
- Documented local Swagger URL in README.

Verification:

- `dotnet build ShortenLink.slnx --no-restore` passed with 0 warnings and 0 errors.
- Local smoke test passed:
  - `GET http://127.0.0.1:5188/swagger/index.html` returned 200.
  - `GET http://127.0.0.1:5188/swagger/v1/swagger.json` returned 200.
  - `GET http://127.0.0.1:5188/api/health` returned `{"status":"ok","app":"ShortenLink.Api"}`.
