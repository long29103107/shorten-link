import { describe, expect, test } from "bun:test";
import { buildShortLinkListUrl } from "../src/features/short-links/api/shortLinksApi";
import {
  createShortLinkDiscoveryChange,
  defaultShortLinkDiscoveryQuery,
  hasShortLinkDiscoveryCriteria
} from "../src/features/short-links/components/ShortLinkDiscoveryToolbar";

describe("admin discovery", () => {
  test("serializes supported list discovery parameters", () => {
    expect(buildShortLinkListUrl(10, 3, {
      search: "  docs.example  ",
      status: "expiring-soon",
      sortBy: "destination",
      sortDirection: "asc"
    })).toBe(
      "/api/short-links?limit=10&page=3&search=docs.example&status=expiring-soon&sortBy=destination&sortDirection=asc&fe=%28%28Code+contains+%60docs.example%60%29+%7C+%28OriginalUrl+contains+%60docs.example%60%29%29&sort=%2BOriginalUrl"
    );
  });

  test("omits an empty search while preserving explicit defaults", () => {
    expect(buildShortLinkListUrl(25, 1, defaultShortLinkDiscoveryQuery)).toBe(
      "/api/short-links?limit=25&page=1&status=all&sortBy=created&sortDirection=desc&sort=-CreatedAt"
    );
    expect(hasShortLinkDiscoveryCriteria(defaultShortLinkDiscoveryQuery)).toBe(false);
  });

  test("resets numbered pagination when toolbar criteria change", () => {
    const nextQuery = { ...defaultShortLinkDiscoveryQuery, status: "inactive" as const };
    expect(createShortLinkDiscoveryChange(nextQuery)).toEqual({
      query: nextQuery,
      pageNumber: 1
    });
    expect(hasShortLinkDiscoveryCriteria(nextQuery)).toBe(true);
  });
});
