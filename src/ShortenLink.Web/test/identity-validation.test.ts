import { describe, expect, test } from "bun:test";
import { classifyHttpFailure } from "../src/shared/api/apiFailure";
import {
  hasFieldErrors,
  mapLoginApiFieldErrors,
  mapManagedUserApiFieldErrors,
  mapRoleAssignmentApiFieldErrors,
  validatePasswordReset,
  validateLoginForm,
  validateManagedUserForm
} from "../src/features/short-links/identityValidation";

describe("identity form validation", () => {
  test("maps required login values to exact controls", () => {
    expect(validateLoginForm({ username: " ", password: "" })).toEqual({
      username: "Enter your username.",
      password: "Enter your password."
    });
  });

  test("requires email, display name, and password while registering a managed user", () => {
    expect(validateManagedUserForm({ email: "invalid", displayName: "", password: "" })).toEqual({
      email: "Enter a valid email address.",
      displayName: "Enter a display name.",
      password: "Enter a password for the new user."
    });
    expect(validateManagedUserForm({ email: "editor@example.com", displayName: "Editor", password: "secret" })).toEqual({});
  });

  test("maps known login and managed-user API fields without unknown fields", () => {
    expect(mapLoginApiFieldErrors({ username: "Required", other: "Fallback" })).toEqual({ username: "Required" });
    expect(mapManagedUserApiFieldErrors({ username: "Duplicate", displayName: "Required", other: "Fallback" })).toEqual({
      displayName: "Required",
      email: "Duplicate"
    });
    expect(mapRoleAssignmentApiFieldErrors({ roleIds: "Unknown role", other: "Fallback" })).toEqual({ roleIds: "Unknown role" });
    expect(validatePasswordReset(" ")).toEqual({ password: "Enter a new password." });
  });

  test("normalizes the backend array-valued field error contract", () => {
    const failure = classifyHttpFailure(400, {
      errorCode: "invalid_security_user",
      message: "Invalid user",
      fieldErrors: { username: ["Username is required."] }
    });
    expect(failure.fieldErrors).toEqual({ username: "Username is required." });
    expect(hasFieldErrors(failure.fieldErrors)).toBe(true);
  });
});
