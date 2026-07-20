import { describe, expect, test } from "bun:test";
import { parseRoute } from "../src/app/router";
import { toFriendlyErrorMessage } from "../src/features/short-links/types";

describe("security navigation", () => {
  test("parses the login route", () => {
    expect(parseRoute("/login")).toEqual({ kind: "login" });
  });

  test("maps invalid login errors to form-friendly copy", () => {
    expect(toFriendlyErrorMessage("invalid_login", "fallback")).toBe("Username or password is invalid.");
  });
});
