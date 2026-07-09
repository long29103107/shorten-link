# API Design Standard

## ASP.NET Core

- Use ASP.NET Core Web API.
- Use Minimal APIs for small endpoint groups.
- Register services before `builder.Build()`.
- Configure middleware before endpoint mapping.
- Group management endpoints under `/api/short-links`.
- Map redirect endpoint as `GET /{code}` only after considering frontend fallback routes.
- Keep endpoint handlers thin. Put reusable behavior in library services.
- Move endpoint groups into extension methods when `Program.cs` becomes hard to scan.

## Contracts

- Use explicit request and response DTOs.
- Keep the documented request/response shapes in `REQUEST.md` and `PRODUCT_VISION.md` stable unless the task explicitly changes them.
- Do not expose EF entities directly if response shape needs stability.
- Keep TypeScript frontend types aligned with backend DTOs.
- For every new API feature, document method, route, request body, response body, and error shape.

## Validation

- Reject empty URLs.
- Accept only `http` and `https` schemes.
- Reject invalid URL formats.
- Validate custom aliases with letters, numbers, `_`, and `-` only.
- Reject duplicate custom aliases.
- Keep production-only private-network blocking behind config if implemented.

## Errors

- Return API-friendly error payloads.
- Prefer ProblemDetails when errors become richer.
- Return 404 JSON for unknown codes when frontend fallback is disabled.
- Return frontend fallback or redirect/render behavior when frontend fallback is enabled.
- Return 410 Gone or configured fallback for expired links.

## Configuration

- Put safe defaults in `appsettings.json`.
- Use `ShortenLink` options for base URL, code generation, database, redirect fallback, analytics, and cache.
- Never require code changes to switch SQLite/PostgreSQL after Phase 2.

