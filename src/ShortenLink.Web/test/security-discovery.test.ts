import { describe, expect, test } from "bun:test";
import { discoverSecurityUsers, getVisiblePages, paginateItems } from "../src/features/short-links/securityDiscovery";
import type { SecurityUser } from "../src/features/short-links/types";

const users: SecurityUser[] = [
  { id: "1", username: "z@example.com", displayName: "Zed", roleIds: [], isEnabled: true, isHidden: false, isBootstrap: false, createdAtUtc: "2026-01-02T00:00:00Z" },
  { id: "2", username: "a@example.com", displayName: "Alice", roleIds: ["Admin"], isEnabled: false, isHidden: false, isBootstrap: false, createdAtUtc: "2026-01-01T00:00:00Z" }
];

describe("security user discovery", () => {
  test("searches, filters, and sorts deterministically", () => {
    expect(discoverSecurityUsers(users, { search: "alice", status: "disabled", sortBy: "email", direction: "asc" }).map((user) => user.id)).toEqual(["2"]);
    expect(discoverSecurityUsers(users, { search: "", status: "all", sortBy: "email", direction: "asc" }).map((user) => user.id)).toEqual(["2", "1"]);
  });

  test("paginates and creates compact page ranges", () => {
    expect(paginateItems([1, 2, 3, 4, 5], 2, 2)).toEqual([3, 4]);
    expect(getVisiblePages(5, 10)).toEqual([1, "gap", 4, 5, 6, "gap", 10]);
  });
});
