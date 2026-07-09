---
name: multiple-agent
description: Work on this repository's ASP.NET Core API, React/Bun frontend, OKF knowledge bundle, CodeGraph index, and Microsoft Agent Framework workflow boundary.
---

# Multiple Agent Repo Skill

Use this skill when implementing, refactoring, or reviewing this repository.

## Read Order

1. `AGENT.md`
2. `.okf/standards/architecture.md`
3. The relevant standard file:
   - Coding: `.okf/standards/coding-style.md`
   - API: `.okf/standards/api-design.md`
   - Testing: `.okf/standards/testing.md`
4. The workflow file matching the task type:
   - Feature: `.okf/workflows/feature.yaml`
   - Bugfix: `.okf/workflows/bugfix.yaml`
   - Refactor: `.okf/workflows/refactor.yaml`
5. The source files affected by the task.

## Project Boundaries

- Backend presentation lives in `src/MultipleAgent.Api`.
- Shared workflow functions and DTOs live in `src/MultipleAgent.Core`.
- Frontend lives in `src/MultipleAgent.Web`.
- Durable project knowledge lives in `.okf`.
- Local generated code intelligence lives in `.codegraph` and is ignored by git.

## Rules

- Keep model/provider secrets out of the browser.
- Keep `IMultiAgentWorkflow` as the backend workflow boundary in `MultipleAgent.Core`.
- Use ASP.NET Core Minimal API style already present in `Program.cs`.
- Use React Router + Zustand + shadcn/ui in the frontend.
- Prefer feature folders for frontend changes.
- Follow `.okf/standards/` for source layout, naming, state, API, and verification rules.

## Verification

Run the smallest relevant verification:

```powershell
dotnet build MultipleAgent.slnx
```

```powershell
cd .\src\MultipleAgent.Web
bun run build
```
