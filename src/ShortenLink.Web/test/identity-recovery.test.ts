import { describe, expect, test } from "bun:test";
import { classifyHttpFailure } from "../src/shared/api/apiFailure";
import {
  createRecoveryNotice,
  resolveOneTimeSecret,
  shouldPreserveMutationContext
} from "../src/shared/api/recovery";

describe("identity recovery compatibility", () => {
  test("keeps transient login failures retryable without auth navigation", () => {
    const failure = classifyHttpFailure(503, {
      errorCode: "service_unavailable",
      message: "Try again later"
    });
    expect(createRecoveryNotice(failure, failure.message)).toEqual({
      message: "Try again later",
      retryable: true
    });
    expect(failure.shouldNavigateToAuth).toBe(false);
  });

  test("keeps invalid login contextual and non-retryable", () => {
    const failure = classifyHttpFailure(400, {
      errorCode: "invalid_login",
      message: "Invalid credentials"
    });
    expect(failure.retryable).toBe(false);
    expect(failure.shouldNavigateToAuth).toBe(false);
  });

  test("preserves security mutation forms only for retryable failures", () => {
    expect(shouldPreserveMutationContext({ retryable: true })).toBe(true);
    expect(shouldPreserveMutationContext({ retryable: false })).toBe(false);
  });

  test("reveals one-time secrets only after success and clears every later boundary", () => {
    expect(resolveOneTimeSecret({ type: "created", secret: "slk_secret" })).toBe("slk_secret");
    expect(resolveOneTimeSecret({ type: "request-started" })).toBeNull();
    expect(resolveOneTimeSecret({ type: "refreshed" })).toBeNull();
    expect(resolveOneTimeSecret({ type: "dismissed" })).toBeNull();
  });
});
