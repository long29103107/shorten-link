import { describe, expect, test } from "bun:test";
import { parseRoute } from "../src/app/router";
import { toFriendlyErrorMessage } from "../src/features/short-links/types";

describe("security navigation", () => {
  test("parses the login route", () => {
    expect(parseRoute("/login")).toEqual({ kind: "login" });
  });

  test("routes security sidebar destinations and keeps the legacy default", () => {
    expect(parseRoute("/security")).toEqual({ kind: "security", section: "users" });
    expect(parseRoute("/security/users")).toEqual({ kind: "security", section: "users" });
    expect(parseRoute("/security/roles")).toEqual({ kind: "security", section: "roles" });
    expect(parseRoute("/security/permissions")).toEqual({ kind: "security", section: "permissions" });
  });

  test("maps invalid login errors to form-friendly copy", () => {
    expect(toFriendlyErrorMessage("invalid_login", "fallback")).toBe("Username or password is invalid.");
  });
});
