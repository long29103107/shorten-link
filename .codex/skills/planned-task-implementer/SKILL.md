---
name: planned-task-implementer
description: Review, activate, and implement one explicitly selected OKF phase task whose status is planned. Use when the user names a specific `.okf/phase/PPP/PPP_TTT-task.md` task and wants Codex to assess its readiness, clarify material problems as a technical adviser, then generate code and keep fixing verification failures until the required builds succeed.
---

# Planned Task Implementer

Use this skill only for a task the user identifies explicitly. Use
`phase-task-runner` instead when the user asks for the current or next active
task.

## Operating Contract

- Treat the user's explicit request as permission to evaluate and activate the
  named planned task, not as permission to ignore dependencies or contradictions.
- Act as a technical adviser before acting as an implementer. Challenge unclear,
  unsafe, contradictory, or unnecessarily complex requirements.
- Ask for confirmation only when a decision is material. Do not pause for
  preferences that can be resolved safely from repository conventions.
- Keep the task, phase summary, implementation, tests, and verification state
  consistent.
- Confirm the task's step goal advances the current phase goal and leaves the
  documented foundation for the next step.
- Continue through generation, compilation, tests, and fixes until required
  verification succeeds or a genuine external blocker requires user action.
- Never claim success when a required check is failing or was not run.

## Workflow

### 1. Resolve the exact task

1. Read `AGENT.md` and the repository skill and standards it routes to.
2. Read `.okf/phase/<PPP>/PHASE_SUMMARY.md` before the named task file.
3. Confirm that exactly one task was named and that its frontmatter status is
   `planned`.
4. Read the full task, its declared dependencies, directly relevant completed
   tasks, and affected source files.
5. Inspect the current worktree before editing and preserve unrelated changes.

If the target is missing or ambiguous, ask the user to identify the exact task.
If it is already active, ready, or done, hand the work to `phase-task-runner`
semantics instead of forcing it through this workflow.

### 2. Perform a readiness review

Compare the task with:

- dependency and phase-summary state;
- current source behavior and public contracts;
- repository architecture, coding, API, and testing standards;
- acceptance criteria and affected-file coverage;
- the phase goal, phase done criteria, step goal, and foundation for the next
  step;
- persistence, concurrency, security, compatibility, and migration risks;
- the smallest design that can satisfy the step goal.

Proceed without another confirmation when the task is coherent and its
dependencies are complete.

Pause and request one consolidated confirmation when any material issue exists,
including:

- an incomplete dependency or a phase scan rule that forbids activation;
- conflict between the task, current code, product direction, or standards;
- acceptance criteria that permit substantially different implementations;
- destructive changes, external mutations, new credentials, or new authority;
- scope that is infeasible, unsafe, or meaningfully over-engineered.

Present each issue as:

```text
Finding: <what does not fit>
Impact: <what could break or change>
Recommendation: <preferred resolution and why>
Decision needed: <the smallest question the user must answer>
```

Recommend a concrete default. Do not implement the disputed portion until the
user confirms it.

### 3. Activate the task

After the readiness gate passes:

1. Update the task status from `planned` to the repository's active-task status.
2. Update `PHASE_SUMMARY.md` to make the phase active and set `current_task`.
3. Preserve task counts and dependency history.

Do not silently mark an incomplete dependency done.

### 4. Implement the complete slice

1. Translate every acceptance criterion into code or an explicit verification.
2. Implement the smallest cohesive design using existing project boundaries.
3. Add or update automated tests for behavior, failure cases, persistence, and
   compatibility required by the task.
4. Generate only artifacts required by the change, such as EF migrations,
   clients, schemas, or lockfiles. Use repository-native generators.
5. Re-read the task after implementation and close any uncovered criterion.

If implementation exposes a new material product or architecture decision,
return to the readiness-review format and ask once. Continue independently for
ordinary coding decisions and repair work.

### 5. Build-to-green loop

Run the smallest relevant checks from `.okf/standards/testing.md`, followed by
all verification commands required by the task:

1. Run generation or schema checks required by the implementation.
2. Build every touched backend and frontend target.
3. Run the relevant automated tests.
4. Diagnose failures from the actual logs.
5. Patch in-scope code, configuration, generated output, or tests.
6. Repeat the failed check, then rerun the full required verification set.

Keep looping for code-caused failures. Stop only for a genuine external blocker
such as unavailable credentials, inaccessible infrastructure, missing user
authority, or a decision that materially changes scope. Report the exact blocker
and the last successful check.

Do not weaken tests, suppress valid errors, or delete user work merely to obtain
a green build.

### 6. Complete bookkeeping

Only after required checks pass:

1. Mark the task `done` and set its completion timestamp.
2. Add concise done notes with implementation and verification evidence.
3. Update phase counts, task index, completed notes, and `current_task`.
4. Close the phase only when all indexed tasks are done and the phase done
   criteria are verified. If the phase goal is still incomplete, propose the
   next step in the same phase.
5. Report the delivered outcome, confirmed decisions, and checks that passed.

Do not pre-create another task unless the user explicitly asks.
