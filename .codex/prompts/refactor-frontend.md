# Refactor Frontend

Use this prompt when changing the React + Bun frontend architecture.

## Required Reads

1. `AGENT.md`
2. `.okf/standards/architecture.md`
3. `.okf/standards/coding-style.md`
4. `.okf/workflows/refactor.yaml`
5. `src/MultipleAgent.Web/components.json`
6. `src/MultipleAgent.Web/src/app/router/AppRouter.tsx`
7. The target feature folder under `src/MultipleAgent.Web/src/features/`

## Architecture

Use this shape:

```text
src/
|-- app/
|   |-- router/
|   |-- providers/
|   `-- layouts/
|-- features/
|   `-- feature-name/
|       |-- api/
|       |-- components/
|       |-- hooks/
|       |-- pages/
|       |-- stores/
|       |-- types.ts
|       `-- routes.tsx
|-- shared/
|   |-- api/
|   |-- components/ui/
|   |-- hooks/
|   |-- utils/
|   |-- constants/
|   `-- types/
|-- assets/
|-- styles/
`-- main.tsx
```

## UI Rules

- Use shadcn/ui primitives from `src/shared/components/ui`.
- Use lucide-react icons in controls and section titles.
- Keep dashboard/tool UI dense, useful, and operational.
- Do not create marketing/landing pages for app workflows.
- Avoid putting cards inside cards.

## Verification

```powershell
cd .\src\MultipleAgent.Web
bun run build
```
