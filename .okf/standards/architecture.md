# Architecture Standard

## Repository Shape

```text
src/
|-- ShortenLink.Core/
|-- ShortenLink.Infrastructure/
|-- ShortenLink.AspNetCore/
|-- ShortenLink.Worker/
|-- ShortenLink.Api/
`-- ShortenLink.Web/

tests/
|-- ShortenLink.Core.Tests/
|-- ShortenLink.Infrastructure.Tests/
`-- ShortenLink.Api.Tests/
```

## Library Boundary

- `ShortenLink.Core` owns domain models, request/result contracts, interfaces, validation, short-code generation, and core service behavior that does not depend on a concrete database or ASP.NET host.
- `ShortenLink.Infrastructure` owns EF Core persistence, repositories, provider selection, migrations, SQLite defaults, and PostgreSQL support.
- `ShortenLink.AspNetCore` owns DI extensions, options binding, endpoint mapping helpers, redirect/fallback integration, and host-facing middleware helpers.
- `ShortenLink.Api` is a demo host. It must use the library projects instead of duplicating short-link business logic.
- `ShortenLink.Web` is a demo frontend. It talks to the API and must not contain backend business rules.
- Keep the reusable library projects packable and consumable by another .NET project.
- Package metadata belongs on the reusable library surface, not on the demo API/Web.

## Frontend Shape

Prefer this React + Vite shape:

```text
src/ShortenLink.Web/
|-- src/
|   |-- app/
|   |-- features/
|   |   `-- short-links/
|   |       |-- api/
|   |       |-- components/
|   |       |-- pages/
|   |       `-- types.ts
|   |-- shared/
|   |   |-- api/
|   |   |-- components/
|   |   `-- utils/
|   |-- styles/
|   `-- main.tsx
|-- index.html
`-- package.json
```

Rules:

- Keep routes and pages small and feature-focused.
- Keep API calls in feature/shared API modules, not scattered through components.
- Use Tailwind utilities for layout and styling unless a shared component requires otherwise.
- Keep frontend fallback behavior visible and user-friendly.

## Backend Shape

Prefer ASP.NET Core Minimal APIs for the demo host and endpoint mapping library.

Keep API entry points thin. Reusable behavior belongs in `ShortenLink.Core`, persistence in `ShortenLink.Infrastructure`, and host integration in `ShortenLink.AspNetCore`.

Use EF Core with SQLite as the default local persistence option. PostgreSQL support must be selected by config only.

## Boundaries

- Do not hard-code database provider selection.
- Do not put redirect resolution rules only in the demo API.
- Do not require Redis, PostgreSQL, Docker, or workers for Phase 1.
- If a choice would make the library impossible to pack as NuGet, flag it with `PACKAGE RISK: <reason>`.
- If a persistence choice weakens SQLite default behavior, flag it with `SQLITE RISK: <reason>`.
- If a proposal adds production infrastructure before the phase needs it, flag it with `OVER-ENGINEERED: <reason>`.

