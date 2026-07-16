---
name: verification-triage
description: Classify build, test, pack, script, restore, CI, frontend build, or verification failures before editing. Use when a command fails, verification is requested, an error log is shown, or Codex must decide whether to rerun, escalate network/permissions, patch code, or skip heavy checks for docs-only work.
---

# Verification Triage

Keep this skill short: classify the failure, choose the smallest trustworthy next action, then stop or execute that action.

## Workflow

1. Read the exact failing command and the first real error, not just the final exit code.
2. Classify it as one of: environment, restore/network, permission/policy, transient lock, known non-blocking noise, docs-only no-run, or real regression.
3. Choose one next action:
   - rerun the same check only for transient locks or flaky process timing;
   - rerun with approval/escalation for network, filesystem, GUI, or policy blockers;
   - patch code/config/tests only for a real regression;
   - verify by read-back only for docs/bookkeeping-only changes.
4. Report the classification and the next action in one compact paragraph unless the user asked for details.

## Repo Heuristics

- Prefer sequential .NET checks after restore: `dotnet build --no-restore`, then `dotnet test --no-build --no-restore`, then pack if needed.
- Treat NuGet/network failures such as `NU1301` as environment/restore until a network-enabled restore proves otherwise.
- Treat `CS2012`, `MSB3026`, or compiler-server file locks as transient; rerun sequentially.
- Treat PowerShell execution-policy failures as policy; rerun with `powershell -ExecutionPolicy Bypass -File ...`.
- Treat duplicate package warnings such as `NU1504` and expected demo-host `IsPackable=false` pack warnings as non-blocking unless the task is warning cleanup.
- For docs-only closure or OKF bookkeeping, do not run full build/test unless command paths, scripts, package metadata, public APIs, or executable code changed.

## Output

Use this shape:

```text
Classification: <one label>
Next action: <one command or one patch target>
Reason: <one sentence tied to the log>
```
