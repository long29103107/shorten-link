import { startTransition, useEffect, useState } from "react";
import { CreateShortLinkPage } from "../features/short-links/pages/CreateShortLinkPage";
import {
  clearStoredSession,
  getAdminPermissionState,
  getStoredCurrentUser,
  getStoredRefreshToken,
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

type NavigationIconName = "endpoint" | "admin" | "users" | "roles" | "permissions" | "sign-in";

const securitySectionIcons = {
  users: "users",
  roles: "roles",
  permissions: "permissions"
} as const satisfies Record<"users" | "roles" | "permissions", NavigationIconName>;

export function App() {
  const [route, setRoute] = useState<AppRoute>(() =>
    getStoredSessionToken() ? parseRoute(window.location.pathname) : { kind: "login" }
  );
  const [recentLink, setRecentLink] = useState<CreatedShortLink | null>(null);
  const [hasAdminEditChanges, setHasAdminEditChanges] = useState(false);
  const [pendingNavigationPath, setPendingNavigationPath] = useState<string | null>(null);
  const [currentUser, setCurrentUser] = useState(() => getStoredCurrentUser());
  const adminPermissions = getAdminPermissionState();

  useEffect(() => {
    const handleAuthChanged = () => {
      const nextUser = getStoredCurrentUser();
      setCurrentUser(nextUser);
      if (!getStoredSessionToken() && window.location.pathname !== "/login") {
        window.history.replaceState({}, "", "/login");
        setRoute({ kind: "login" });
      }
    };

    window.addEventListener("shortenlink-auth-changed", handleAuthChanged);
    return () => window.removeEventListener("shortenlink-auth-changed", handleAuthChanged);
  }, []);

  useEffect(() => {
    const token = getStoredSessionToken();
    if (!token) {
      if (window.location.pathname !== "/login") {
        window.history.replaceState({}, "", "/login");
      }
      return;
    }

    let isCurrent = true;
    void getCurrentSecurityUser()
      .then((user) => {
        if (isCurrent) {
          const refreshToken = getStoredRefreshToken();
          if (refreshToken) {
            storeSession(token, refreshToken, user);
          } else {
            clearStoredSession();
          }
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
      if (!getStoredSessionToken() && nextPath !== "/login") {
        window.history.replaceState({}, "", "/login");
        setRoute({ kind: "login" });
        return;
      }
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

  const commitNavigation = (requestedPath: string) => {
    const path = !getStoredSessionToken() && requestedPath !== "/login"
      ? "/login"
      : requestedPath;
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
        ? "Identity & Access"
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
        ? `Manage ${route.section} access controls`
      : route.kind === "login"
        ? "Use your ShortenLink identity session"
      : route.kind === "detail"
        ? "Inspect and retire one generated link"
        : route.kind === "status"
          ? "Return to the short-link workspace"
          : "Random short-link creation";

  if (route.kind === "status" || route.kind === "login") {
    return (
      <div className="status-shell">
        {route.kind === "status" ? (
          <StatusPage
            statusCode={route.statusCode}
            onBackHome={() => navigate("/")}
          />
        ) : (
          <LoginPage
            onSignedIn={() => navigate("/security/users")}
            onBackHome={() => navigate("/")}
          />
        )}
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
            <NavigationIcon name="endpoint" />
            Endpoint
          </Button>
          <Button
            className="sidebar-nav-button"
            aria-current={route.kind === "admin" ? "page" : undefined}
            variant="ghost"
            onClick={() => navigate("/admin")}
          >
            <NavigationIcon name="admin" />
            Admin URLs
          </Button>
          {currentUser || adminPermissions.canManageSecurityAssignments ? (
            <div className="sidebar-nav-group">
              <p className="sidebar-nav-group-label">Security</p>
              {(["users", "roles", "permissions"] as const).map((section) => (
                <Button
                  key={section}
                  className="sidebar-nav-button sidebar-nav-child"
                  aria-current={route.kind === "security" && route.section === section ? "page" : undefined}
                  variant="ghost"
                  onClick={() => navigate(`/security/${section}`)}
                >
                  <NavigationIcon name={securitySectionIcons[section]} />
                  {section[0].toUpperCase() + section.slice(1)}
                </Button>
              ))}
            </div>
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
              variant="ghost"
              onClick={() => navigate("/login")}
            >
              <NavigationIcon name="sign-in" />
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
          <SecurityManagementPage section={route.section} />
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

function NavigationIcon({ name }: { name: NavigationIconName }) {
  const paths: Record<NavigationIconName, React.ReactNode> = {
    endpoint: (
      <>
        <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71" />
        <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71" />
      </>
    ),
    admin: (
      <>
        <rect width="18" height="18" x="3" y="3" rx="2" />
        <path d="M8 3v18M8 8h13M8 13h13" />
      </>
    ),
    users: (
      <>
        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
        <circle cx="9" cy="7" r="4" />
        <path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
      </>
    ),
    roles: (
      <>
        <path d="M20 13c0 5-3.5 7.5-8 9-4.5-1.5-8-4-8-9V5l8-3 8 3v8Z" />
        <path d="m9 12 2 2 4-4" />
      </>
    ),
    permissions: (
      <>
        <circle cx="7.5" cy="15.5" r="5.5" />
        <path d="m21 2-9.6 9.6M15 8l3 3M18 5l3 3" />
      </>
    ),
    "sign-in": (
      <>
        <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4" />
        <path d="m10 17 5-5-5-5M15 12H3" />
      </>
    )
  };

  return (
    <svg
      className="nav-icon"
      aria-hidden="true"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      {paths[name]}
    </svg>
  );
}
