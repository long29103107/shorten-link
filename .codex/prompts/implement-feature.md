# Implement Feature

Use this prompt when adding a project feature.

## Context First

1. Read `AGENT.md`.
2. Read `.okf/standards/architecture.md`.
3. Read `.okf/standards/coding-style.md`.
4. Read `.okf/standards/api-design.md` for backend/API work.
5. Read `.okf/workflows/feature.yaml`.
6. Use `codegraph status` if CodeGraph is available, then inspect source files directly before editing.

## Implementation Rules

- Keep backend provider secrets server-side.
- Keep `IMultiAgentWorkflow` stable unless the task explicitly changes the workflow contract.
- For frontend work, follow the `app/features/shared/assets/styles` architecture.
- Use shadcn/ui components from `src/shared/components/ui`.
- Add feature-local API, hooks, store, types, pages, components, and routes under `src/MultipleAgent.Web/src/features/<feature-name>/`.
- Update `.okf` files when architecture, public API contracts, workflow rules, or source conventions change.

## Verification

Run the relevant checks:

```powershell
dotnet build MultipleAgent.slnx
```

```powershell
cd .\src\MultipleAgent.Web
bun run build
```
