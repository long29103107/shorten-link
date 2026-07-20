import { startTransition, useEffect, useState } from "react";
import { CreateShortLinkPage } from "../features/short-links/pages/CreateShortLinkPage";
import {
  clearStoredSession,
  getAdminPermissionState,
  getStoredCurrentUser,
  getStoredSessionToken,
  storeSession
} from "../features/short-links/api/adminSecurity";
import { getCurrentSecurityUser } from "../features/short-links/api/shortLinksApi";
import { LoginPage } from "../features/short-links/pages/LoginPage";
import { SecurityManagementPage } from "../features/short-links/pages/SecurityManagementPage";
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
  const [currentUser, setCurrentUser] = useState(() => getStoredCurrentUser());
  const adminPermissions = getAdminPermissionState();

  useEffect(() => {
    const handleAuthChanged = () => {
      setCurrentUser(getStoredCurrentUser());
    };

    window.addEventListener("shortenlink-auth-changed", handleAuthChanged);
    return () => window.removeEventListener("shortenlink-auth-changed", handleAuthChanged);
  }, []);

  useEffect(() => {
    const token = getStoredSessionToken();
    if (!token) {
      return;
    }

    let isCurrent = true;
    void getCurrentSecurityUser()
      .then((user) => {
        if (isCurrent) {
          storeSession(token, user);
        }
      })
      .catch(() => {
        if (isCurrent) {
          clearStoredSession();
        }
      });

    return () => {
      isCurrent = false;
    };
  }, []);

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
      : route.kind === "security"
        ? "Security"
      : route.kind === "login"
        ? "Sign in"
      : route.kind === "detail"
        ? "Link detail"
        : route.kind === "status"
          ? `${route.statusCode}`
          : "Endpoint";

  const pageDescription =
    route.kind === "admin"
      ? "Manage generated random short links"
      : route.kind === "security"
        ? "Manage user, role, and permission assignments"
      : route.kind === "login"
        ? "Use your ShortenLink identity session"
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
          {currentUser || adminPermissions.canManageSecurityAssignments ? (
            <Button
              className="sidebar-nav-button"
              aria-current={route.kind === "security" ? "page" : undefined}
              variant="ghost"
              onClick={() => navigate("/security")}
            >
              <span className="nav-glyph" aria-hidden="true" />
              Security
            </Button>
          ) : null}
        </nav>

        <div className="session-panel">
          {currentUser ? (
            <>
              <p>{currentUser.displayName || currentUser.username}</p>
              <code>{currentUser.roles.join(", ") || "No role"}</code>
              <Button
                variant="secondary"
                onClick={() => {
                  clearStoredSession();
                  navigate("/login");
                }}
              >
                Sign out
              </Button>
            </>
          ) : (
            <Button
              className="sidebar-nav-button"
              aria-current={route.kind === "login" ? "page" : undefined}
              variant="ghost"
              onClick={() => navigate("/login")}
            >
              <span className="nav-glyph" aria-hidden="true" />
              Sign in
            </Button>
          )}
        </div>
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

        {route.kind === "security" ? (
          <SecurityManagementPage />
        ) : null}

        {route.kind === "login" ? (
          <LoginPage onSignedIn={() => navigate("/security")} />
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
