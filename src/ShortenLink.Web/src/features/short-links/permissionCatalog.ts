export const permissionDescriptions: Record<string, string> = {
  "short_links.read": "View short links and their current status.",
  "short_links.create": "Create new short links.",
  "short_links.update": "Edit destinations, aliases, and expiration settings.",
  "short_links.activate": "Activate short links so they can redirect visitors.",
  "short_links.deactivate": "Deactivate short links without deleting them.",
  "short_links.delete": "Permanently delete short links.",
  "short_links.export": "Export short-link data for reporting or backup.",
  "analytics.read": "View traffic and redirect analytics.",
  "audit_logs.read": "Review security and administrative activity logs.",
  "security.assignments.manage": "Manage users, roles, and access assignments."
};

export function getPermissionDescription(permission: string) {
  return permissionDescriptions[permission] ?? "Use this application permission.";
}
