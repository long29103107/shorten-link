---
phase: 009
title: Live Publishing Preflight
status: active
created_at: 2026-07-15
updated_at: 2026-07-15
current_task: 009_001
task_count: 1
done_count: 0
depends_on:
  - 008
---

# Phase 009 Summary

## Phase Goal

Prepare the project for a future intentional live NuGet.org publishing decision by documenting and verifying the external preconditions that cannot be proven by repository-only release automation.

Phase 009 should bridge the completed release-readiness closure to real registry operations without publishing packages by default, committing credentials, or adding automatic publish behavior.

## Phase Done Criteria

- Maintainers have a preflight checklist for NuGet.org package ID ownership, account access, API key scope, and approval authority.
- The checklist maps the existing local release gates to the exact evidence required before a live publish attempt.
- The live publish path remains manual, credential-protected, and opt-in only.
- Dry-run, local feed rehearsal, and consumer smoke remain the required repository-side gates before any external publish.
- Documentation distinguishes repository-ready evidence from external registry state that a maintainer must confirm.
- No NuGet.org package push, API key, secret, token, or automatic publish workflow is added.

## Scope

In:

- NuGet.org package ownership and maintainer approval preflight.
- Manual API key and package ID checks that happen outside source control.
- Mapping repository release gates to live publish readiness.
- Documentation for go/no-go criteria before `scripts\publish-nuget.ps1 -Publish`.
- Task bookkeeping for the live publishing preflight phase.

Out:

- Do not publish to NuGet.org.
- Do not add real API keys, secrets, tokens, or credentials.
- Do not create automatic publish-on-push, publish-on-tag, GitHub Releases, or deployment automation.
- Do not change package IDs, public APIs, or package metadata unless a later preflight task exposes a concrete blocker.
- Do not add SaaS billing, tenants, authentication, authorization, or analytics dashboards.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 009_001 | NuGet.org publish preflight checklist | planned | |

## Current Task

`009_001 - NuGet.org publish preflight checklist`

## Completed Notes

- None yet.

## Next Task Proposal

After `009_001`, decide whether the external NuGet.org ownership and credential prerequisites are satisfied. If they are not, stop Phase 009 as blocked by external maintainer action rather than adding a publish task.

## Scan Rule

Agents must read this file before loading any `009_*` task file. Do not activate Phase 009 until Phase 008 is complete.
