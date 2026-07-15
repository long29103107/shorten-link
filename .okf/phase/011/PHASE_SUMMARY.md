---
phase: 011
title: Security And Permission-Based Admin Protection
status: active
created_at: 2026-07-15
updated_at: 2026-07-15
current_task:
task_count: 1
done_count: 1
depends_on:
  - 010
---

# Phase 011 Summary

## Phase Goal

Add a small, production-aware security foundation for the demo admin surface using permission-based authorization grouped through roles, without turning the product into a full identity platform.

Phase 011 should protect admin management actions, define reusable permission contracts, and leave a clean path for future user/session/JWT integrations.

## Phase Done Criteria

- Admin UI and admin API mutations are protected by an explicit authorization boundary.
- Permissions are modeled as first-class constants or policies and can be grouped by role.
- Default local/demo configuration remains easy to run without external identity infrastructure.
- Unauthorized and forbidden outcomes reuse the Phase 010 status experience.
- Tests cover protected endpoint behavior and permission evaluation.
- Documentation explains the local security model and how to configure it.

## Scope

In:

- Permission catalog for short-link admin actions.
- Role-to-permission grouping for default roles such as Owner, Admin, Editor, and Viewer.
- Minimal local/demo authentication or API-key/session boundary if needed to exercise permissions.
- ASP.NET Core authorization policies for admin endpoints.
- Frontend handling for `401` and `403` outcomes.
- Tests for permission evaluation and endpoint protection.

Out:

- OAuth/OIDC provider integration.
- Multi-tenant user management.
- Billing, invitations, password reset, or account lifecycle.
- Public SaaS security administration.
- Replacing the existing short-link core contracts unless a security boundary requires a narrow extension.

## Task Index

| Task | Title | Status | Done At |
|---|---|---|---|
| 011_001 | Permission catalog and admin authorization foundation | done | 2026-07-15T20:47:00+07:00 |

## Current Task

No active task.

## Completed Notes

- `011_001` established permission constants, role bundles, API-key permission evaluation, a protected admin list endpoint, frontend `401`/`403` routing, README security docs, and tests for role bundles plus protected endpoint outcomes.

## Next Task Proposal

Next task should apply the permission boundary across admin mutations: create, update, activate, deactivate, and delete.

## Scan Rule

Read this file before loading any `011_*` task file. Keep implementation permission-based, with roles acting as permission bundles rather than hard-coded authorization logic.
