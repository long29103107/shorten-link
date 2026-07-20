import type { AppRoute } from "../features/short-links/types";

export function parseRoute(pathname: string): AppRoute {
  if (pathname === "/") {
    return { kind: "home" };
  }

  if (pathname === "/admin") {
    return { kind: "admin" };
  }

  if (pathname === "/security") {
    return { kind: "security" };
  }

  if (pathname === "/login") {
    return { kind: "login" };
  }

  if (pathname === "/unauthorized") {
    return { kind: "status", statusCode: 401 };
  }

  if (pathname === "/forbidden") {
    return { kind: "status", statusCode: 403 };
  }

  if (pathname === "/not-found") {
    return { kind: "status", statusCode: 404 };
  }

  const detailMatch = /^\/links\/([^/]+)$/.exec(pathname);
  if (detailMatch) {
    const code = decodeURIComponent(detailMatch[1] ?? "").trim();
    return code ? { kind: "detail", code } : { kind: "status", statusCode: 404 };
  }

  return { kind: "status", statusCode: 404 };
}
