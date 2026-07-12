import { startTransition, useEffect, useState } from "react";
import { DetailLookup } from "../features/short-links/components/DetailLookup";
import { CreateShortLinkPage } from "../features/short-links/pages/CreateShortLinkPage";
import { NotFoundPage } from "../features/short-links/pages/NotFoundPage";
import { ShortLinkDetailPage } from "../features/short-links/pages/ShortLinkDetailPage";
import type { AppRoute, CreatedShortLink } from "../features/short-links/types";
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

  return (
    <div className="app-shell">
      <div className="app-backdrop" />
      <header className="topbar">
        <div>
          <p className="eyebrow">Shorten Link</p>
          <h1 className="app-title">Create, inspect, and retire links from one quiet desk.</h1>
        </div>
        <DetailLookup
          onOpenDetails={(code) => navigate(`/links/${encodeURIComponent(code)}`)}
        />
      </header>

      <main className="workspace">
        {route.kind === "home" ? (
          <CreateShortLinkPage
            recentLink={recentLink}
            onCreated={(createdLink) => setRecentLink(createdLink)}
            onOpenDetails={(code) => navigate(`/links/${encodeURIComponent(code)}`)}
          />
        ) : null}

        {route.kind === "detail" ? (
          <ShortLinkDetailPage
            code={route.code}
            onBackHome={() => navigate("/")}
          />
        ) : null}

        {route.kind === "not-found" ? (
          <NotFoundPage onBackHome={() => navigate("/")} />
        ) : null}
      </main>
    </div>
  );
}
