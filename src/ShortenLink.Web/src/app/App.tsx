import { startTransition, useEffect, useState } from "react";
import { CreateShortLinkPage } from "../features/short-links/pages/CreateShortLinkPage";
import { ShortLinkAdminPage } from "../features/short-links/pages/ShortLinkAdminPage";
import { StatusPage } from "../features/short-links/pages/StatusPage";
import { ShortLinkDetailPage } from "../features/short-links/pages/ShortLinkDetailPage";
import type { AppRoute, CreatedShortLink } from "../features/short-links/types";
import { Button } from "../shared/components/ui/button";
import { ConfirmDialog } from "../shared/components/ConfirmDialog";
import { Toaster } from "../shared/components/Toaster";
import { parseRoute } from "./router";

export function App() {
  const [route, setRoute] = useState<AppRoute>(() => parseRoute(window.location.pathname));
  const [recentLink, setRecentLink] = useState<CreatedShortLink | null>(null);
  const [hasAdminEditChanges, setHasAdminEditChanges] = useState(false);
  const [pendingNavigationPath, setPendingNavigationPath] = useState<string | null>(null);

  useEffect(() => {
    const handlePopState = () => {
      const nextPath = window.location.pathname;
      if (route.kind === "admin" && hasAdminEditChanges && nextPath !== "/admin") {
        window.history.pushState({}, "", "/admin");
        setPendingNavigationPath(nextPath);
        startTransition(() => {
          setRoute(parseRoute("/admin"));
        });
        return;
      }

      startTransition(() => {
        setRoute(parseRoute(nextPath));
      });
    };

    window.addEventListener("popstate", handlePopState);
    return () => window.removeEventListener("popstate", handlePopState);
  }, [hasAdminEditChanges, route.kind]);

  const commitNavigation = (path: string) => {
    if (window.location.pathname !== path) {
      window.history.pushState({}, "", path);
    }

    startTransition(() => {
      setRoute(parseRoute(path));
    });
  };

  const navigate = (path: string) => {
    if (route.kind === "admin" && hasAdminEditChanges && path !== "/admin") {
      setPendingNavigationPath(path);
      return;
    }

    commitNavigation(path);
  };

  const confirmDiscardAndNavigate = () => {
    if (!pendingNavigationPath) {
      return;
    }

    setHasAdminEditChanges(false);
    commitNavigation(pendingNavigationPath);
    setPendingNavigationPath(null);
  };

  const pageTitle =
    route.kind === "admin"
      ? "Admin"
      : route.kind === "detail"
        ? "Link detail"
        : route.kind === "status"
          ? `${route.statusCode}`
          : "Endpoint";

  const pageDescription =
    route.kind === "admin"
      ? "Manage generated random short links"
      : route.kind === "detail"
        ? "Inspect and retire one generated link"
        : route.kind === "status"
          ? "Return to the short-link workspace"
          : "Random short-link creation";

  if (route.kind === "status") {
    return (
      <div className="status-shell">
        <StatusPage
          statusCode={route.statusCode}
          onBackHome={() => navigate("/")}
        />
        <ConfirmDialog
          open={pendingNavigationPath !== null}
          title="Discard form changes?"
          description="You have unsaved changes in the admin form. Leave this page and discard them?"
          confirmLabel="Discard changes"
          variant="destructive"
          onConfirm={confirmDiscardAndNavigate}
          onCancel={() => setPendingNavigationPath(null)}
        />
        <Toaster />
      </div>
    );
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
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
        </header>

        <div className="workspace">
        {route.kind === "home" ? (
          <CreateShortLinkPage
            recentLink={recentLink}
            onCreated={(createdLink) => setRecentLink(createdLink)}
          />
        ) : null}

        {route.kind === "admin" ? (
          <ShortLinkAdminPage onDirtyChange={setHasAdminEditChanges} />
        ) : null}

        {route.kind === "detail" ? (
          <ShortLinkDetailPage
            code={route.code}
            onBackHome={() => navigate("/")}
          />
        ) : null}

        </div>
      </main>
      <ConfirmDialog
        open={pendingNavigationPath !== null}
        title="Discard form changes?"
        description="You have unsaved changes in the admin form. Leave this page and discard them?"
        confirmLabel="Discard changes"
        variant="destructive"
        onConfirm={confirmDiscardAndNavigate}
        onCancel={() => setPendingNavigationPath(null)}
      />
      <Toaster />
    </div>
  );
}
