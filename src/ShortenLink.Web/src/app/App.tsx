import { startTransition, useEffect, useState } from "react";
import { CreateShortLinkPage } from "../features/short-links/pages/CreateShortLinkPage";
import { ShortLinkAdminPage } from "../features/short-links/pages/ShortLinkAdminPage";
import { NotFoundPage } from "../features/short-links/pages/NotFoundPage";
import { ShortLinkDetailPage } from "../features/short-links/pages/ShortLinkDetailPage";
import type { AppRoute, CreatedShortLink } from "../features/short-links/types";
import { Button } from "../shared/components/ui/button";
import { parseRoute } from "./router";

export function App() {
  const [route, setRoute] = useState<AppRoute>(() => parseRoute(window.location.pathname));
  const [recentLink, setRecentLink] = useState<CreatedShortLink | null>(null);

  useEffect(() => {
    const handlePopState = () => {
      startTransition(() => {
        setRoute(parseRoute(window.location.pathname));
      });
    };

    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, []);

  const navigate = (path: string) => {
    if (window.location.pathname !== path) {
      window.history.pushState({}, "", path);
    }

    startTransition(() => {
      setRoute(parseRoute(path));
    });
  };

  const pageTitle =
    route.kind === "admin"
      ? "Admin"
      : route.kind === "detail"
        ? "Link detail"
        : route.kind === "not-found"
          ? "Not found"
          : "Endpoint";

  const pageDescription =
    route.kind === "admin"
      ? "Manage generated random short links"
      : route.kind === "detail"
        ? "Inspect and retire one generated link"
        : route.kind === "not-found"
          ? "Return to the short-link workspace"
          : "Random short-link creation";

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="window-controls" aria-hidden="true">
          <span className="window-dot window-dot-red" />
          <span className="window-dot window-dot-yellow" />
          <span className="window-dot window-dot-green" />
        </div>

        <div className="brand-block">
          <div className="brand-mark">SL</div>
          <div>
            <h1>Shorten Link</h1>
          </div>
        </div>

        <div className="release-note">
          <p>Random code mode enabled</p>
          <code>100% generated links</code>
        </div>

        <nav className="sidebar-nav" aria-label="Primary">
          <Button
            className="sidebar-nav-button"
            aria-current={route.kind === "home" ? "page" : undefined}
            variant="ghost"
            onClick={() => navigate("/")}
          >
            <span className="nav-glyph" aria-hidden="true" />
            Endpoint
          </Button>
          <Button
            className="sidebar-nav-button"
            aria-current={route.kind === "admin" ? "page" : undefined}
            variant="ghost"
            onClick={() => navigate("/admin")}
          >
            <span className="nav-glyph" aria-hidden="true" />
            Admin URLs
          </Button>
        </nav>

      </aside>

      <main className="app-main">
        <header className="topbar">
          <div>
            <p className="eyebrow">Shorten Link</p>
            <h1 className="app-title">{pageTitle}</h1>
            <p className="page-description">{pageDescription}</p>
          </div>
          <div className="topbar-tools" aria-hidden="true">
            <span />
            <span />
          </div>
        </header>

        <div className="workspace">
        {route.kind === "home" ? (
          <CreateShortLinkPage
            recentLink={recentLink}
            onCreated={(createdLink) => setRecentLink(createdLink)}
          />
        ) : null}

        {route.kind === "admin" ? <ShortLinkAdminPage /> : null}

        {route.kind === "detail" ? (
          <ShortLinkDetailPage
            code={route.code}
            onBackHome={() => navigate("/")}
          />
        ) : null}

        {route.kind === "not-found" ? (
          <NotFoundPage onBackHome={() => navigate("/")} />
        ) : null}
        </div>
      </main>
    </div>
  );
}
