---
name: frontend-architecture
description: Build and refactor the MultipleAgent.Web React + Bun frontend using app/features/shared architecture, React Router, Zustand, and shadcn/ui.
---

# Frontend Architecture Skill

Use this skill for work inside `src/MultipleAgent.Web`.

## Required Reads

1. `src/MultipleAgent.Web/components.json`
2. `.okf/standards/architecture.md`
3. `.okf/standards/coding-style.md`
4. `.okf/standards/testing.md`
5. `src/MultipleAgent.Web/src/app/router/AppRouter.tsx`
6. `src/MultipleAgent.Web/src/app/layouts/MainLayout.tsx`
7. The target feature folder under `src/MultipleAgent.Web/src/features`.

## Architecture Rules

- App wiring belongs in `src/app`.
- User-facing routes belong in `features/<feature>/routes.tsx`.
- Feature UI belongs in `features/<feature>/components`.
- Feature data access belongs in `features/<feature>/api`.
- Feature state belongs in `features/<feature>/stores`.
- Cross-feature primitives belong in `shared`.
- Global styling belongs in `styles/global.css`.

## UI Rules

- Use shadcn/ui primitives from `src/shared/components/ui`.
- Use lucide-react icons for buttons, metrics, and section labels.
- Keep dashboard UI operational and scannable.
- Keep card radius at 8px or below.
- Do not create a landing page when a usable app screen is needed.

## Verification

```powershell
cd .\src\MultipleAgent.Web
bun run build
```
