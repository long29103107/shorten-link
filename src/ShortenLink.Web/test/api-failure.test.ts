import { describe, expect, test } from "bun:test";
import {
  classifyFetchFailure,
  classifyHttpFailure
} from "../src/shared/api/apiFailure";
import { ToastDeduplicator } from "../src/shared/toast";

describe("shared API failure policy", () => {
  test("classifies network and timeout failures as retryable", () => {
    expect(classifyFetchFailure(new TypeError("fetch failed"))).toMatchObject({
      kind: "network",
      status: null,
      errorCode: "network_error",
      retryable: true,
      shouldNavigateToAuth: false
    });
    expect(classifyFetchFailure({ name: "AbortError" })).toMatchObject({
      kind: "timeout",
      errorCode: "request_timeout",
      retryable: true
    });
  });

  test("classifies retryable HTTP failures", () => {
    expect(classifyHttpFailure(408, payload("timeout"))).toMatchObject({ kind: "timeout", retryable: true });
    expect(classifyHttpFailure(429, payload("limited"))).toMatchObject({ kind: "rate-limit", retryable: true });
    expect(classifyHttpFailure(503, payload("down"))).toMatchObject({ kind: "server", retryable: true });
  });

  test("keeps validation and auth failures non-retryable", () => {
    expect(classifyHttpFailure(400, payload("invalid_url"))).toMatchObject({
      kind: "validation",
      retryable: false,
      shouldNavigateToAuth: false
    });
    expect(classifyHttpFailure(401, payload("unauthorized"))).toMatchObject({
      kind: "authentication",
      retryable: false,
      shouldNavigateToAuth: true
    });
    expect(classifyHttpFailure(403, payload("forbidden"))).toMatchObject({
      kind: "authorization",
      retryable: false,
      shouldNavigateToAuth: true
    });
    expect(classifyHttpFailure(404, payload("not_found"))).toMatchObject({
      kind: "not-found",
      retryable: false
    });
  });
});

describe("error toast deduplication", () => {
  test("suppresses equivalent errors inside the window and allows later recurrence", () => {
    let now = 1000;
    const deduplicator = new ToastDeduplicator(5000, 100, () => now);
    const toast = { title: "Request failed", message: "Unavailable", variant: "error" as const };

    expect(deduplicator.shouldEmit(toast)).toBe(true);
    now = 5999;
    expect(deduplicator.shouldEmit(toast)).toBe(false);
    now = 6000;
    expect(deduplicator.shouldEmit(toast)).toBe(true);
  });

  test("allows distinct errors and all non-error notifications", () => {
    const deduplicator = new ToastDeduplicator(5000, 100, () => 1000);
    expect(deduplicator.shouldEmit({ title: "Failed", message: "A", variant: "error" })).toBe(true);
    expect(deduplicator.shouldEmit({ title: "Failed", message: "B", variant: "error" })).toBe(true);
    expect(deduplicator.shouldEmit({ title: "Saved", variant: "success" })).toBe(true);
    expect(deduplicator.shouldEmit({ title: "Saved", variant: "success" })).toBe(true);
  });

  test("bounds tracked deduplication state", () => {
    const deduplicator = new ToastDeduplicator(5000, 2, () => 1000);
    deduplicator.shouldEmit({ title: "A", variant: "error" });
    deduplicator.shouldEmit({ title: "B", variant: "error" });
    deduplicator.shouldEmit({ title: "C", variant: "error" });
    expect(deduplicator.trackedErrorCount).toBe(2);
  });
});

function payload(errorCode: string) {
  return { errorCode, message: errorCode };
}
