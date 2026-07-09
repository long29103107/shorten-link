# Add Agent Workflow

Use this prompt when connecting or expanding Microsoft Agent Framework workflows.

## Required Reads

1. `AGENT.md`
2. `.okf/standards/architecture.md`
3. `.okf/standards/api-design.md`
4. `.okf/workflows/feature.yaml`
5. `src/MultipleAgent.Api/MultiAgent/IMultiAgentWorkflow.cs`
6. `src/MultipleAgent.Api/MultiAgent/Contracts.cs`

## Backend Rules

- Keep provider credentials, deployment names, and endpoints server-side.
- Keep the React app talking to the ASP.NET Core API, not directly to model providers.
- Preserve `/api/workflows/run` unless the task explicitly introduces streaming or AG-UI.
- Put workflow provider implementation behind `IMultiAgentWorkflow`.
- Prefer dependency injection and options classes for provider configuration.

## Frontend Rules

- If workflow response shape changes, update `features/workflow/types.ts` first.
- Keep Zustand store actions small and feature-local.
- Show observable execution state in the UI.

## Verification

```powershell
dotnet build MultipleAgent.slnx
```

```powershell
cd .\src\MultipleAgent.Web
bun run build
```
