---
name: okf-compactor
description: Compact OKF phase, task, memory, or rollout markdown history into fewer searchable files while preserving task ids, current state, done notes, verification evidence, and next-task direction. Use when the user asks to compact, merge, clean up, or reduce noisy OKF/task/history files.
---

# OKF Compactor

Goal: reduce file count and token load without losing task ids or current-state fidelity.

## Workflow

1. Read the relevant summary/index file first.
2. Inventory files before editing with `rg --files`.
3. Preserve these fields in the compacted markdown:
   - frontmatter phase/status/counts/current task;
   - task index table;
   - `Step Goal`, `Scope`, `Acceptance Criteria`, `Foundation for Next Step`, `Verification`, and `Done Notes`;
   - source filename before compaction.
4. Prefer one phase file: `.okf/phase/<PPP>/PHASE_SUMMARY.md` with `## Task Notes`.
5. Update README/templates/skills that still mention the old layout.
6. Delete only files whose content has been copied into the compacted target.
7. Verify with:
   - file inventory;
   - `rg` for obsolete wording;
   - `git diff --check`.

## Guardrails

- Keep task ids searchable as plain text, for example `011_004`.
- Keep planned/current tasks visibly actionable.
- Do not rewrite product meaning while compacting.
- Do not run build/test for docs-only compaction.

## Output

Report file-count reduction, where the compacted notes live, and which checks passed.
