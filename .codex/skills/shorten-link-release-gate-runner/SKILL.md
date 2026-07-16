---
name: shorten-link-release-gate-runner
description: Run or triage the safe ShortenLink release gate for package metadata, dry-run manifests, local feed rehearsal, manual publish guardrails, and release closure. Use for shorten-link release/package tasks; never publish to NuGet.org or use secrets unless explicitly requested.
---

# ShortenLink Release Gate Runner

Default stance: prove release readiness locally; do not publish or mutate registries.

## Safe Order

1. Inspect the current phase summary and release docs.
2. Build/package only what the task changed.
3. Prefer local dry-run scripts before any publish wrapper.
4. Inspect generated manifests, not only exit codes.
5. Use local feed rehearsal as the bridge between dry-run and any future real publish.

## Commands

Use only when relevant:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test ShortenLink.slnx --verbosity minimal
dotnet pack ShortenLink.slnx -c Release --verbosity minimal
powershell -ExecutionPolicy Bypass -File .\scripts\release-dry-run.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\rehearse-local-feed.ps1
```

## Guardrails

- `scripts/release-dry-run.ps1` is the local package gate.
- Manifest evidence matters: package ids, version policy, hashes, artifact paths, and `publishAttempted: false`.
- `scripts/publish-nuget.ps1` must stay preview-by-default; `-Publish` and credentials require explicit user intent.
- Do not add secrets, push packages, deploy, or mutate NuGet feeds by default.
- Treat `ShortenLink.Api` `IsPackable=false` and known duplicate test package warnings as non-blocking unless the task targets them.

## Output

Report:

```text
Gate: <dry-run/local-feed/docs-only/manual-preview>
Evidence: <manifest/doc/check>
Publish attempted: no
Next release blocker: <none or exact blocker>
```
