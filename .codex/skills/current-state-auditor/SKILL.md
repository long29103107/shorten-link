---
name: current-state-auditor
description: Audit the repository's current phase, task, worktree, and blocker state without implementing changes. Use when the user asks what task is active, what remains not done, whether a ticket is done, what phase is current, or asks for status before deciding what to do next.
---

# Current State Auditor

Answer from the current repo state. Do not implement, scaffold, or update docs unless the user asks.

## Workflow

1. Inspect the worktree with `git status --short` when change safety matters.
2. Read the authoritative phase summary files before answering task state:
   - current layout: `.okf/phase/<PPP>/PHASE_SUMMARY.md`
   - task detail lives under `## Task Notes`, not separate task files.
3. Identify:
   - active phase;
   - `current_task`;
   - task rows whose status is not `done`;
   - obvious blockers or mismatches between frontmatter and task index.
4. Answer with exact ids, titles, statuses, and file paths.

## Stop Rules

- Do not rely on memory when the repo files are cheap to read.
- Do not create the next task or implement a planned task.
- If state is inconsistent, report the inconsistency and the smallest repair option.

## Output

Keep it terse:

```text
Active phase: <PPP title/status>
Current task: <PPP_TTT title/status>
Not done: <ids or none>
Source: <PHASE_SUMMARY.md path>
```
