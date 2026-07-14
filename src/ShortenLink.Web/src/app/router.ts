import type { AppRoute } from "../features/short-links/types";

export function parseRoute(pathname: string): AppRoute {
  if (pathname === "/") {
    return { kind: "home" };
  }

  if (pathname === "/admin") {
    return { kind: "admin" };
  }

  if (pathname === "/not-found") {
    return { kind: "not-found" };
  }

  const detailMatch = /^\/links\/([^/]+)$/.exec(pathname);
  if (detailMatch) {
    const code = decodeURIComponent(detailMatch[1] ?? "").trim();
    return code ? { kind: "detail", code } : { kind: "not-found" };
  }

  return { kind: "not-found" };
}
