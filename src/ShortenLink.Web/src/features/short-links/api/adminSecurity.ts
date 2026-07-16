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

export function getAdminApiKeyHeader(): Record<string, string> {
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
