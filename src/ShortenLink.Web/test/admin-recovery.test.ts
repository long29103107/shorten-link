import { describe, expect, test } from "bun:test";
import {
  createRecoveryNotice,
  shouldPreserveMutationContext
} from "../src/shared/api/recovery";

describe("admin recovery policy", () => {
  test("offers explicit retry for retryable read failures", () => {
    expect(createRecoveryNotice({ retryable: true }, "Server unavailable")).toEqual({
      message: "Server unavailable",
      retryable: true
    });
  });

  test("preserves mutation context only for retryable failures", () => {
    expect(shouldPreserveMutationContext({ retryable: true })).toBe(true);
    expect(shouldPreserveMutationContext({ retryable: false })).toBe(false);
    expect(shouldPreserveMutationContext(new Error("unknown"))).toBe(false);
  });

  test("does not offer retry for validation failures", () => {
    expect(createRecoveryNotice({ retryable: false }, "Check the URL")).toEqual({
      message: "Check the URL",
      retryable: false
    });
  });
});
