---
task: 017_001
phase: 017
title: Field-Aware API Validation Error Contract
status: done
created_at: 2026-07-20
completed_at: 2026-07-20T16:23:23+07:00
depends_on:
  - 016
---

# 017_001 - Field-Aware API Validation Error Contract

## Step Goal

Add backward-compatible field metadata to API validation failures for short-link and security request contracts.

This task should let later frontend work map backend validation failures to exact controls without parsing messages or duplicating endpoint-specific error-code switches.

## Scope

In:

- Extend the shared API error response with optional field-error metadata while preserving `errorCode` and `message`.
- Use stable JSON field names matching request DTO properties in frontend casing.
- Attach field metadata to short-link create/update URL and expiration validation failures.
- Attach field metadata to login, user, custom-role, personal API-key, and security-assignment request validation failures where a specific field or field group is known.
- Keep conflicts, not-found, authentication, authorization, and operational failures as form-level errors unless they clearly identify an input field.
- Add focused API tests for response shape, field names, multiple-field cases, and legacy compatibility.
- Document the additive validation error contract in README.

Out:

- Do not change frontend form behavior in this task.
- Do not replace existing `errorCode` values or message text solely to support field mapping.
- Do not introduce FluentValidation, JSON Schema, code generation, or a new validation dependency.
- Do not expose persistence entities or sensitive submitted values in error payloads.
- Do not change auth, permission, conflict-status, or retry semantics.

## Relevant Standards

- `.okf/standards/architecture.md`
- `.okf/standards/coding-style.md`
- `.okf/standards/api-design.md`
- `.okf/standards/testing.md`
- `PRODUCT_VISION.md`

## Affected Files

- `.okf/phase/017/PHASE_SUMMARY.md`
- `README.md`
- `src\ShortenLink.AspNetCore\ShortenLinkEndpointRouteBuilderExtensions.cs`
- `src\ShortenLink.AspNetCore\`
- `tests\ShortenLink.Api.Tests\ShortLinkEndpointsTests.cs`

## Acceptance Criteria

- Existing clients can continue reading `errorCode` and `message` unchanged.
- Validation responses may include a stable `fieldErrors` object keyed by frontend request-field names.
- Short-link URL and expiration failures identify `originalUrl` and `expiredAtUtc` respectively.
- Identity and security validation failures identify the relevant username, password, display-name, role, permission, API-key, or assignment inputs when deterministically known.
- Multiple invalid inputs can be represented in one response without exposing submitted secrets.
- Non-validation errors do not receive misleading field metadata.
- Focused API tests verify additive serialization, field mapping, multiple fields, and legacy error-code/message compatibility.
- README documents the response shape and states that server validation remains authoritative.

## Foundation for Next Step

This task should leave a verified additive API contract that the next task can consume in the short-link create/update UI for exact field mapping and frontend/backend validation parity.

## Verification

Run after implementation:

```powershell
dotnet build ShortenLink.slnx --verbosity minimal
dotnet test tests\ShortenLink.Api.Tests\ShortenLink.Api.Tests.csproj --verbosity minimal
```

## Done Notes

- Extended the API error response with an optional `fieldErrors` dictionary while preserving existing `errorCode`, `message`, and HTTP status behavior.
- Omitted `fieldErrors` from serialized non-field failures through null-ignore serialization.
- Added short-link validation mappings for `originalUrl` and `expiredAtUtc` on create and update failures.
- Added field mappings for missing login inputs, API-key display names, security assignments, custom roles, managed users, roles, and permissions.
- Kept unknown/bad-password login, authorization, not-found, conflict, and operational errors form-level without inferred field metadata.
- Added multi-field missing-login coverage plus compatibility assertions for legacy error code/message behavior and non-field login failures.
- Documented the additive field-aware validation response and server-authoritative behavior in README.
- Verified with `dotnet build ShortenLink.slnx --verbosity minimal` and 67 passing API tests.
