---
name: scope-output-clarifier
description: Determine the concrete required output, smallest useful action, and token-saving boundary before doing work. Use when the user's request is broad, ambiguous, exploratory, or likely to trigger unnecessary repo reads, edits, plans, tests, or long explanations.
---

# Scope Output Clarifier

Use this to avoid spending tokens before the deliverable is clear.

## Workflow

1. Convert the user request into one concrete output:
   - answer only;
   - repo state report;
   - docs edit;
   - code change;
   - verification run;
   - skill/template creation;
   - handoff/summary.
2. Define the smallest source set needed to produce that output.
3. Decide whether to act or ask one question:
   - act when the default is safe and reversible;
   - ask only when multiple outputs would materially change files, architecture, credentials, publish/deploy behavior, or user-visible scope.
4. Keep the response or plan short: no more than three bullets unless the user asks for detail.

## Token Rules

- Prefer targeted `rg` plus 1-3 file reads over broad repo exploration.
- Do not restate obvious system behavior.
- Do not create a long plan for a small answer.
- Stop after the requested output; do not add optional next work unless it is clearly useful.

## Output

If clarification is needed:

```text
I can do this as <option A> or <option B>. Which output do you want?
```

If no clarification is needed, proceed and keep the final answer compact.
