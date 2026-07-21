import type { SecurityCurrentUser } from "../types";

export const shortLinkPermissions = {
  read: "short_links.read",
  create: "short_links.create",
  update: "short_links.update",
  activate: "short_links.activate",
  deactivate: "short_links.deactivate",
  delete: "short_links.delete",
  export: "short_links.export",
  analyticsRead: "analytics.read",
  auditLogsRead: "audit_logs.read",
  securityAssignmentsManage: "security.assignments.manage"
} as const;

const allPermissions = Object.values(shortLinkPermissions);
const accessTokenKey = "shortenLink.accessToken";
const refreshTokenKey = "shortenLink.refreshToken";
const legacySessionTokenKey = "shortenLink.sessionToken";
const currentUserKey = "shortenLink.currentUser";

const rolePermissionBundles: Record<string, readonly string[]> = {
  owner: allPermissions,
  admin: allPermissions,
  editor: [
    shortLinkPermissions.read,
    shortLinkPermissions.create,
    shortLinkPermissions.update,
    shortLinkPermissions.activate,
    shortLinkPermissions.deactivate
  ],
  viewer: [
    shortLinkPermissions.read,
    shortLinkPermissions.analyticsRead
  ]
};

export type AdminPermissionState = {
  canCreate: boolean;
  canUpdate: boolean;
  canActivate: boolean;
  canDeactivate: boolean;
  canDelete: boolean;
  canReadAnalytics: boolean;
  canManageSecurityAssignments: boolean;
};

export function getStoredSessionToken(): string | null {
  return window.localStorage.getItem(accessTokenKey)
    ?? window.localStorage.getItem(legacySessionTokenKey);
}

export function getStoredRefreshToken(): string | null {
  return window.localStorage.getItem(refreshTokenKey);
}

export function getStoredCurrentUser(): SecurityCurrentUser | null {
  const value = window.localStorage.getItem(currentUserKey);
  if (!value) {
    return null;
  }

  try {
    return JSON.parse(value) as SecurityCurrentUser;
  } catch {
    clearStoredSession();
    return null;
  }
}

export function storeSession(accessToken: string, refreshToken: string, user: SecurityCurrentUser): void {
  window.localStorage.setItem(accessTokenKey, accessToken);
  window.localStorage.setItem(refreshTokenKey, refreshToken);
  window.localStorage.removeItem(legacySessionTokenKey);
  window.localStorage.setItem(currentUserKey, JSON.stringify(user));
  window.dispatchEvent(new Event("shortenlink-auth-changed"));
}

export function clearStoredSession(): void {
  window.localStorage.removeItem(accessTokenKey);
  window.localStorage.removeItem(refreshTokenKey);
  window.localStorage.removeItem(legacySessionTokenKey);
  window.localStorage.removeItem(currentUserKey);
  window.dispatchEvent(new Event("shortenlink-auth-changed"));
}

export function getAdminApiKeyHeader(): Record<string, string> {
  const sessionToken = getStoredSessionToken();
  if (sessionToken) {
    return { Authorization: `Bearer ${sessionToken}` };
  }

  const apiKey = import.meta.env.VITE_SHORTENLINK_ADMIN_API_KEY?.trim();
  if (!apiKey) {
    return {};
  }

  const headerName =
    import.meta.env.VITE_SHORTENLINK_ADMIN_API_KEY_HEADER?.trim()
    || "X-ShortenLink-Api-Key";

  return { [headerName]: apiKey };
}

export function getAdminPermissionState(): AdminPermissionState {
  const permissions = getConfiguredPermissions();

  return {
    canCreate: permissions.has(shortLinkPermissions.create),
    canUpdate: permissions.has(shortLinkPermissions.update),
    canActivate: permissions.has(shortLinkPermissions.activate),
    canDeactivate: permissions.has(shortLinkPermissions.deactivate),
    canDelete: permissions.has(shortLinkPermissions.delete),
    canReadAnalytics: permissions.has(shortLinkPermissions.analyticsRead),
    canManageSecurityAssignments: permissions.has(shortLinkPermissions.securityAssignmentsManage)
  };
}

function getConfiguredPermissions(): Set<string> {
  const currentUser = getStoredCurrentUser();
  if (currentUser) {
    return new Set(currentUser.permissions);
  }

  const configuredPermissions = parseList(import.meta.env.VITE_SHORTENLINK_ADMIN_PERMISSIONS);
  const configuredRoles = parseList(import.meta.env.VITE_SHORTENLINK_ADMIN_ROLE);

  if (configuredPermissions.length === 0 && configuredRoles.length === 0) {
    return new Set(allPermissions);
  }

  const permissions = new Set(configuredPermissions);
  for (const role of configuredRoles) {
    const rolePermissions = rolePermissionBundles[role.toLowerCase()];
    rolePermissions?.forEach((permission) => permissions.add(permission));
  }

  return permissions;
}

function parseList(value: string | undefined): string[] {
  if (!value) {
    return [];
  }

  return value
    .split(",")
    .map((item) => item.trim())
    .filter(Boolean);
}
