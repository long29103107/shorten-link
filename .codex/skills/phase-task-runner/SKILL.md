---
name: phase-task-runner
description: Run one active OKF phase task at a time in this repository. Use when the user asks to do the next task, continue phase work, create or update .okf/phase task notes, inspect PHASE_SUMMARY.md, mark a task done, or propose the next PPP_TTT task without pre-creating future tasks.
---

# Phase Task Runner

Use this skill for `.okf/phase/<PPP>/` work in this repository.

Use `planned-task-implementer` instead when the user explicitly selects a task
whose status is `planned` and wants a readiness review followed by implementation.
This runner remains the authority for the current or next active task.

## Delivery Hierarchy

Use this hierarchy consistently:

```text
Product Vision
  -> Phase Goal
      -> Task/Step Goal
          -> Verified Outcome
              -> Foundation for the next Task/Step
```

- `PRODUCT_VISION.md` defines the product direction and ordered phases.
- `PHASE_SUMMARY.md` defines one phase goal and measurable phase done criteria.
- Each `PPP_TTT` task note is one implementation step inside that phase, with its
  own step goal, acceptance criteria, verification, and explicit foundation for
  the next step.
- A completed step advances the phase; it does not automatically complete it.

## Core Rules

- Load `.okf/phase/<PPP>/PHASE_SUMMARY.md` before reading task notes.
- Work on only one task note at a time.
- Use task ids in `PPP_TTT` format, for example `001_002`.
- Keep each phase compacted into a single `PHASE_SUMMARY.md`.
- Create subsequent tasks inside the current active phase until the phase
  goal is actually achieved.
- Do not open a new phase merely because the current task is done. Open the
  next phase only after the current phase goal and its done criteria
  are complete.
- Define phase goals broadly enough to require meaningful incremental tasks; do
  not make a phase goal identical to one smallest task.
- Give every phase explicit done criteria. Close it only when all criteria are
  satisfied by verified task outcomes.
- Give every task one independently verifiable step goal and state what durable
  capability or contract it leaves for the next task.
- The next task must build on verified output from completed tasks in the same
  phase; it must not recreate a parallel foundation.
- Do not create future task notes unless the user explicitly asks.
- After finishing a task, update the task note and phase bookkeeping in `PHASE_SUMMARY.md` in the same pass.
- After finishing a task, propose the next smallest task, but do not create it yet.

## Expected Layout

```text
.okf/phase/
  001/
    PHASE_SUMMARY.md
```

## Task Workflow

1. Read `PHASE_SUMMARY.md`.
2. Identify `current_task` and the task row with `Status` not `done`.
3. Read only that task note plus directly relevant source files.
4. Implement the smallest complete slice described by the task.
5. Verify with the smallest relevant commands from `.okf/standards/testing.md`.
6. Mark the task `done`, set `completed_at`, append done notes, and update phase counts.
7. Evaluate the phase goal and done criteria:
   - if it is not achieved, propose the next task in the same phase;
   - if it is achieved, close the phase and propose the next phase.
8. Suggest the next task id/title in the final response without creating it.

## Markdown Requirements

Every phase summary must have YAML frontmatter.

Task notes must include:

- `Step Goal`
- `Scope`
- `Acceptance Criteria`
- `Foundation for Next Step`
- `Affected Files`
- `Verification`
- `Done Notes`

Phase summaries must include:

- `Phase Goal`
- `Phase Done Criteria`
- task index table
- completed notes
- current task
- next task proposal area
- task notes area

## Done Criteria

A task is done only when code/docs requested by the task are complete, verification has been run or explicitly reported as skipped, and summary bookkeeping is updated.
