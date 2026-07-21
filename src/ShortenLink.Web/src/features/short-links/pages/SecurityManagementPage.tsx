import { useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import {
  disableCustomSecurityRole,
  disableSecurityUser,
  listSecurityRoles,
  listSecurityUsers,
  upsertCustomSecurityRole,
  upsertSecurityUser
} from "../api/shortLinksApi";
import { getAdminPermissionState, getStoredCurrentUser, shortLinkPermissions } from "../api/adminSecurity";
import { ApiError } from "../api/http";
import type { SecurityRole, SecuritySection, SecurityUser } from "../types";
import { formatDateTime, toFriendlyErrorMessage } from "../types";
import {
  hasFieldErrors,
  mapManagedUserApiFieldErrors,
  mapPasswordResetApiFieldErrors,
  mapRoleAssignmentApiFieldErrors,
  validateManagedUserForm,
  validatePasswordReset,
  type ManagedUserFieldErrors
} from "../identityValidation";
import {
  mapCustomRoleApiFieldErrors,
  validateCustomRoleForm,
  type CustomRoleFieldErrors
} from "../securityValidation";
import {
  defaultSecurityUserDiscovery,
  discoverSecurityUsers,
  getVisiblePages,
  paginateItems,
  type SecurityUserDiscovery
} from "../securityDiscovery";
import { Badge } from "../../../shared/components/ui/badge";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "../../../shared/components/ui/card";
import { EmptyState } from "../../../shared/components/EmptyState";
import { Input } from "../../../shared/components/ui/input";
import { Label } from "../../../shared/components/ui/label";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "../../../shared/components/ui/table";
import { showToast } from "../../../shared/toast";
import { createRecoveryNotice, type RecoveryNotice } from "../../../shared/api/recovery";

const permissionOptions = Object.values(shortLinkPermissions);
const permissionGroups = [
  { id: "short-links", name: "Short links", permissions: permissionOptions.filter((permission) => permission.startsWith("short_links.")) },
  { id: "reporting", name: "Reporting and audit", permissions: permissionOptions.filter((permission) => permission === "analytics.read" || permission === "audit_logs.read") },
  { id: "security", name: "Security", permissions: permissionOptions.filter((permission) => permission.startsWith("security.")) }
];

export function SecurityManagementPage({ section }: { section: SecuritySection }) {
  const adminPermissions = getAdminPermissionState();
  const currentUser = getStoredCurrentUser();
  const [isLoading, setIsLoading] = useState(false);
  const [readFailure, setReadFailure] = useState<RecoveryNotice | null>(null);
  const [actionFailure, setActionFailure] = useState<RecoveryNotice | null>(null);
  const [users, setUsers] = useState<SecurityUser[]>([]);
  const [systemRoles, setSystemRoles] = useState<SecurityRole[]>([]);
  const [customRoles, setCustomRoles] = useState<SecurityRole[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [isCreateUserOpen, setIsCreateUserOpen] = useState(false);
  const [userDiscovery, setUserDiscovery] = useState<SecurityUserDiscovery>(defaultSecurityUserDiscovery);
  const [userPage, setUserPage] = useState(1);
  const [userPageSize, setUserPageSize] = useState(10);
  const [createUserForm, setCreateUserForm] = useState({ email: "", displayName: "", password: "" });
  const [createUserErrors, setCreateUserErrors] = useState<ManagedUserFieldErrors>({});
  const [resetPassword, setResetPassword] = useState("");
  const [profileEmail, setProfileEmail] = useState("");
  const [profileDisplayName, setProfileDisplayName] = useState("");
  const [profileError, setProfileError] = useState<string | undefined>();
  const [resetPasswordError, setResetPasswordError] = useState<string | undefined>();
  const [assignedRoleIds, setAssignedRoleIds] = useState<string[]>([]);
  const [roleAssignmentError, setRoleAssignmentError] = useState<string | undefined>();
  const [roleForm, setRoleForm] = useState({ id: "", name: "", permissions: [] as string[], isEnabled: true });
  const [roleFieldErrors, setRoleFieldErrors] = useState<CustomRoleFieldErrors>({});

  const selectedUser = useMemo(
    () => users.find((user) => user.id === selectedUserId) ?? null,
    [selectedUserId, users]
  );
  const roleOptions = useMemo(
    () => [...systemRoles, ...customRoles].filter((role) => role.isEnabled),
    [customRoles, systemRoles]
  );
  const discoveredUsers = useMemo(() => discoverSecurityUsers(users, userDiscovery), [userDiscovery, users]);
  const userTotalPages = Math.max(1, Math.ceil(discoveredUsers.length / userPageSize));
  const visibleUsers = useMemo(
    () => paginateItems(discoveredUsers, Math.min(userPage, userTotalPages), userPageSize),
    [discoveredUsers, userPage, userPageSize, userTotalPages]
  );

  const updateUserDiscovery = (patch: Partial<SecurityUserDiscovery>) => {
    setUserDiscovery((current) => ({ ...current, ...patch }));
    setUserPage(1);
  };

  const loadSecurity = async () => {
    setIsLoading(true);
    setReadFailure(null);
    try {
      if (!adminPermissions.canManageSecurityAssignments) {
        return;
      }
      const [rolesResult, usersResult] = await Promise.all([listSecurityRoles(), listSecurityUsers()]);
      setSystemRoles(rolesResult.systemRoles);
      setCustomRoles(rolesResult.customRoles);
      setUsers(usersResult.items);
    } catch (error) {
      setReadFailure(toRecoveryNotice(error, "Security data could not be loaded."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadSecurity();
  }, []);

  const createUser = async () => {
    const errors = validateManagedUserForm(createUserForm);
    if (hasFieldErrors(errors)) {
      setCreateUserErrors(errors);
      setActionFailure(null);
      return;
    }

    setCreateUserErrors({});
    setActionFailure(null);
    try {
      const user = await upsertSecurityUser({
        id: createInternalUserId(),
        username: createUserForm.email.trim(),
        displayName: createUserForm.displayName.trim(),
        password: createUserForm.password,
        roleIds: [],
        isEnabled: true
      });
      setUsers((current) => upsertBy(current, user, "id"));
      setCreateUserForm({ email: "", displayName: "", password: "" });
      setIsCreateUserOpen(false);
      selectUser(user);
      showToast({ title: "User registered", message: user.username, variant: "success" });
    } catch (error) {
      const fieldErrors = error instanceof ApiError ? mapManagedUserApiFieldErrors(error.fieldErrors) : {};
      setCreateUserErrors(fieldErrors);
      setActionFailure(hasFieldErrors(fieldErrors) ? null : toRecoveryNotice(error, "User could not be registered."));
    }
  };

  const selectUser = (user: SecurityUser) => {
    setSelectedUserId(user.id);
    setAssignedRoleIds(user.roleIds);
    setProfileEmail(user.username);
    setProfileDisplayName(user.displayName);
    setProfileError(undefined);
    setResetPassword("");
    setResetPasswordError(undefined);
    setRoleAssignmentError(undefined);
    setActionFailure(null);
  };

  const updateSelectedUserProfile = async () => {
    if (!selectedUser) return;
    if (!profileEmail.trim() || !profileDisplayName.trim()) {
      setProfileError("Enter email and display name.");
      return;
    }
    setProfileError(undefined);
    try {
      const updated = await upsertSecurityUser({ ...selectedUser, username: profileEmail.trim(), displayName: profileDisplayName.trim(), password: null });
      setUsers((current) => upsertBy(current, updated, "id"));
      showToast({ title: "User updated", message: updated.username, variant: "success" });
    } catch (error) {
      setActionFailure(toRecoveryNotice(error, "User could not be updated."));
    }
  };

  const resetSelectedUserPassword = async () => {
    if (!selectedUser) return;
    const errors = validatePasswordReset(resetPassword);
    if (errors.password) {
      setResetPasswordError(errors.password);
      return;
    }

    setResetPasswordError(undefined);
    setActionFailure(null);
    try {
      const updated = await upsertSecurityUser({
        id: selectedUser.id,
        username: selectedUser.username,
        displayName: selectedUser.displayName,
        password: resetPassword,
        roleIds: selectedUser.roleIds,
        isEnabled: selectedUser.isEnabled
      });
      setUsers((current) => upsertBy(current, updated, "id"));
      setResetPassword("");
      showToast({ title: "Password reset", message: selectedUser.username, variant: "success" });
    } catch (error) {
      const fieldErrors = error instanceof ApiError ? mapPasswordResetApiFieldErrors(error.fieldErrors) : {};
      setResetPasswordError(fieldErrors.password);
      setActionFailure(fieldErrors.password ? null : toRecoveryNotice(error, "Password could not be reset."));
    }
  };

  const saveSelectedUserRoles = async () => {
    if (!selectedUser) return;
    setRoleAssignmentError(undefined);
    setActionFailure(null);
    try {
      const updated = await upsertSecurityUser({
        id: selectedUser.id,
        username: selectedUser.username,
        displayName: selectedUser.displayName,
        password: null,
        roleIds: assignedRoleIds,
        isEnabled: selectedUser.isEnabled
      });
      setUsers((current) => upsertBy(current, updated, "id"));
      showToast({ title: "Roles assigned", message: selectedUser.username, variant: "success" });
    } catch (error) {
      const fieldErrors = error instanceof ApiError ? mapRoleAssignmentApiFieldErrors(error.fieldErrors) : {};
      setRoleAssignmentError(fieldErrors.roleIds);
      setActionFailure(fieldErrors.roleIds ? null : toRecoveryNotice(error, "Roles could not be assigned."));
    }
  };

  const disableSelectedUser = async () => {
    if (!selectedUser) return;
    setActionFailure(null);
    try {
      const result = await disableSecurityUser(selectedUser.id);
      setUsers((current) => current.map((user) => user.id === result.id ? { ...user, isEnabled: result.isEnabled } : user));
      showToast({ title: "User disabled", message: selectedUser.username, variant: "success" });
    } catch (error) {
      setActionFailure(toRecoveryNotice(error, "User could not be disabled."));
    }
  };

  const saveRole = async () => {
    const errors = validateCustomRoleForm(roleForm);
    if (hasFieldErrors(errors)) {
      setRoleFieldErrors(errors);
      setActionFailure(null);
      return;
    }

    setRoleFieldErrors({});
    setActionFailure(null);
    try {
      const role = await upsertCustomSecurityRole({
        id: roleForm.id.trim(),
        name: roleForm.name.trim(),
        permissions: roleForm.permissions,
        isEnabled: roleForm.isEnabled
      });
      setCustomRoles((current) => upsertBy(current, role, "id"));
      setRoleForm({ id: "", name: "", permissions: [], isEnabled: true });
      showToast({ title: "Role saved", message: role.name, variant: "success" });
    } catch (error) {
      const fieldErrors = error instanceof ApiError ? mapCustomRoleApiFieldErrors(error.fieldErrors) : {};
      setRoleFieldErrors(fieldErrors);
      setActionFailure(hasFieldErrors(fieldErrors) ? null : toRecoveryNotice(error, "Role could not be saved."));
    }
  };

  if (!currentUser) {
    return <EmptyState title="Sign in required" description="Sign in to manage users, roles, and permissions." />;
  }

  if (!adminPermissions.canManageSecurityAssignments) {
    return <EmptyState title="Permission required" description="This section requires security.assignments.manage." />;
  }

  return (
    <Card className="admin-panel">
      <CardHeader className="panel-heading-wide">
        <div className="card-title-row">
          <span className="card-glyph" aria-hidden="true" />
          <div>
            <p className="eyebrow">Identity</p>
            <CardTitle>Manage users, roles, and permissions.</CardTitle>
          </div>
        </div>
        <Button variant="secondary" disabled={isLoading} onClick={() => void loadSecurity()}>Refresh</Button>
      </CardHeader>
      <CardContent>
        {readFailure ? <RecoveryBanner notice={readFailure} onRetry={() => void loadSecurity()} /> : null}
        {actionFailure ? <RecoveryBanner notice={actionFailure} onDismiss={() => setActionFailure(null)} /> : null}

        {section === "users" ? (
          <div className="security-tab-stack">
            <div className="security-list-header">
              <div>
                <p className="eyebrow">Users</p>
                <h3>Manage registered identities</h3>
              </div>
              <Button onClick={() => setIsCreateUserOpen(true)}>Create</Button>
            </div>

            {isCreateUserOpen ? (
              <Card className="security-form-card status-page">
                <CardHeader>
                  <p className="eyebrow">Register identity</p>
                  <CardTitle>Create managed user</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="muted-copy">Create the identity first, then assign roles in the separate user section.</p>
                  <IdentityField id="new-user-email" label="Email" type="email" autoComplete="email" value={createUserForm.email} error={createUserErrors.email} onChange={(email) => {
                    setCreateUserForm((current) => ({ ...current, email }));
                    setCreateUserErrors((current) => ({ ...current, email: undefined }));
                  }} />
                  <IdentityField id="new-user-display-name" label="Display name" value={createUserForm.displayName} error={createUserErrors.displayName} onChange={(displayName) => {
                    setCreateUserForm((current) => ({ ...current, displayName }));
                    setCreateUserErrors((current) => ({ ...current, displayName: undefined }));
                  }} />
                  <IdentityField id="new-user-password" label="Password" type="password" autoComplete="new-password" value={createUserForm.password} error={createUserErrors.password} onChange={(password) => {
                    setCreateUserForm((current) => ({ ...current, password }));
                    setCreateUserErrors((current) => ({ ...current, password: undefined }));
                  }} />
                </CardContent>
                <CardFooter>
                  <Button variant="secondary" onClick={() => {
                    setCreateUserForm({ email: "", displayName: "", password: "" });
                    setCreateUserErrors({});
                    setIsCreateUserOpen(false);
                  }}>Cancel</Button>
                  <Button onClick={() => void createUser()}>Register user</Button>
                </CardFooter>
              </Card>
            ) : null}

            <div className="admin-discovery-toolbar">
              <div className="admin-discovery-search"><Input
                aria-label="Search users"
                placeholder="Search email or display name"
                value={userDiscovery.search}
                onChange={(event) => updateUserDiscovery({ search: event.target.value })}
              /></div>
              <label className="admin-discovery-field"><span>Status</span><select value={userDiscovery.status} onChange={(event) => updateUserDiscovery({ status: event.target.value as SecurityUserDiscovery["status"] })}><option value="all">All</option><option value="enabled">Enabled</option><option value="disabled">Disabled</option></select></label>
              <label className="admin-discovery-field"><span>Sort by</span><select value={userDiscovery.sortBy} onChange={(event) => updateUserDiscovery({ sortBy: event.target.value as SecurityUserDiscovery["sortBy"] })}><option value="createdAt">Created date</option><option value="email">Email</option><option value="displayName">Display name</option></select></label>
              <label className="admin-discovery-field"><span>Direction</span><select value={userDiscovery.direction} onChange={(event) => updateUserDiscovery({ direction: event.target.value as SecurityUserDiscovery["direction"] })}><option value="desc">Descending</option><option value="asc">Ascending</option></select></label>
            </div>

            {visibleUsers.length === 0 ? (
              <EmptyState title={users.length === 0 ? "No users" : "No matching users"} description={users.length === 0 ? "Create a user to populate this table." : "Try different search or filter criteria."} />
            ) : (
              <div className="admin-table-wrap">
                <Table>
                  <TableHeader><TableRow><TableHead>Email</TableHead><TableHead>Display name</TableHead><TableHead>Roles</TableHead><TableHead>Created</TableHead><TableHead>Status</TableHead><TableHead>Actions</TableHead></TableRow></TableHeader>
                  <TableBody>
                    {visibleUsers.map((user) => (
                      <TableRow key={user.id}>
                        <TableCell><button type="button" className="table-link-button" onClick={() => selectUser(user)}>{user.username}</button></TableCell>
                        <TableCell>{user.displayName}</TableCell>
                        <TableCell>{user.roleIds.join(", ") || "No roles"}</TableCell>
                        <TableCell>{formatDateTime(user.createdAtUtc)}</TableCell>
                        <TableCell><Badge variant={user.isEnabled ? "default" : "destructive"}>{user.isEnabled ? "Enabled" : "Disabled"}</Badge></TableCell>
                        <TableCell><div className="admin-row-actions"><Button variant="secondary" onClick={() => selectUser(user)}>Manage</Button>{user.isEnabled ? <Button variant="destructive" onClick={() => { selectUser(user); void disableSecurityUser(user.id).then((result) => { setUsers((current) => current.map((item) => item.id === result.id ? { ...item, isEnabled: false } : item)); showToast({ title: "User disabled", message: user.username, variant: "success" }); }).catch((error) => setActionFailure(toRecoveryNotice(error, "User could not be disabled."))); }}>Deactivate</Button> : null}</div></TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}

            {discoveredUsers.length > 0 ? (
              <div className="admin-pagination">
                <div className="pagination-summary"><span>{discoveredUsers.length} items</span><span>Page {Math.min(userPage, userTotalPages)} of {userTotalPages}</span></div>
                <div className="pagination-controls">
                  <label className="pagination-page-size"><select value={userPageSize} onChange={(event) => { setUserPageSize(Number(event.target.value)); setUserPage(1); }}><option value="10">10</option><option value="25">25</option><option value="50">50</option></select><span>/ page</span></label>
                  <div className="pagination-pages" aria-label="User pagination">
                    {userPage > 1 ? <button type="button" className="pagination-arrow pagination-arrow-prev" aria-label="Previous page" onClick={() => setUserPage((page) => page - 1)} /> : null}
                    {getVisiblePages(Math.min(userPage, userTotalPages), userTotalPages).map((item, index) => item === "gap" ? <span key={`gap-${index}`} className="pagination-gap">...</span> : <button key={item} type="button" className={item === userPage ? "pagination-page pagination-page-active" : "pagination-page"} onClick={() => setUserPage(item)}>{item}</button>)}
                    {userPage < userTotalPages ? <button type="button" className="pagination-arrow pagination-arrow-next" aria-label="Next page" onClick={() => setUserPage((page) => page + 1)} /> : null}
                  </div>
                </div>
              </div>
            ) : null}

            {selectedUser ? (
              <Card className="selected-user-panel">
                <CardHeader className="panel-heading-wide">
                  <div>
                    <p className="eyebrow">Selected user</p>
                    <CardTitle>{selectedUser.displayName}</CardTitle>
                    <p className="muted-copy">{selectedUser.username}</p>
                  </div>
                  <Badge variant={selectedUser.isEnabled ? "default" : "destructive"}>{selectedUser.isEnabled ? "Enabled" : "Disabled"}</Badge>
                </CardHeader>
                <CardContent className="security-user-actions-grid">
                  <section className="security-action-section">
                    <h3>Update user</h3>
                    <IdentityField id="update-user-email" label="Email" type="email" value={profileEmail} onChange={(value) => { setProfileEmail(value); setProfileError(undefined); }} />
                    <IdentityField id="update-user-display" label="Display name" value={profileDisplayName} error={profileError} onChange={(value) => { setProfileDisplayName(value); setProfileError(undefined); }} />
                    <Button onClick={() => void updateSelectedUserProfile()}>Save changes</Button>
                  </section>
                  <section className="security-action-section">
                    <h3>Reset password</h3>
                    <p className="muted-copy">Replace the sign-in password for this identity.</p>
                    <IdentityField id="reset-user-password" label="New password" type="password" autoComplete="new-password" value={resetPassword} error={resetPasswordError} onChange={(password) => {
                      setResetPassword(password);
                      setResetPasswordError(undefined);
                    }} />
                    <Button onClick={() => void resetSelectedUserPassword()}>Set new password</Button>
                  </section>

                  <section className="security-action-section">
                    <h3>Assign roles</h3>
                    <p className="muted-copy">Choose the access bundles assigned to this identity.</p>
                    <RoleChoiceGroup roles={roleOptions} selected={assignedRoleIds} error={roleAssignmentError} onToggle={(roleId) => {
                      setAssignedRoleIds((current) => current.includes(roleId) ? current.filter((id) => id !== roleId) : [...current, roleId]);
                      setRoleAssignmentError(undefined);
                    }} />
                    <Button onClick={() => void saveSelectedUserRoles()}>Save roles</Button>
                  </section>
                </CardContent>
                <CardFooter>
                  <Button variant="destructive" disabled={!selectedUser.isEnabled} onClick={() => void disableSelectedUser()}>Disable user</Button>
                </CardFooter>
              </Card>
            ) : null}
          </div>
        ) : null}

        {section === "roles" ? (
          <RolePermissionMatrix
            roles={[...systemRoles, ...customRoles]}
            form={roleForm}
            errors={roleFieldErrors}
            onFormChange={setRoleForm}
            onErrorsChange={setRoleFieldErrors}
            onSave={() => void saveRole()}
            onDisable={(roleId) => void disableCustomSecurityRole(roleId).then((result) => {
              setCustomRoles((current) => current.map((item) => item.id === result.id ? { ...item, isEnabled: result.isEnabled } : item));
            }).catch((error) => setActionFailure(toRecoveryNotice(error, "Role could not be disabled.")))}
          />
        ) : null}

        {section === "permissions" ? (
          <Card className="security-form-card permission-catalog">
            <CardHeader><p className="eyebrow">Permissions</p><CardTitle>Supported permission catalog</CardTitle></CardHeader>
            <CardContent>
              <p className="muted-copy">Permissions are assigned through roles. Use the Roles tab to compose a custom access bundle.</p>
              <div className="permission-catalog-grid">
                {permissionOptions.map((permission) => <code key={permission}>{permission}</code>)}
              </div>
            </CardContent>
          </Card>
        ) : null}
      </CardContent>
    </Card>
  );
}

function IdentityField({ id, label, value, error, type = "text", autoComplete, onChange }: {
  id: string;
  label: string;
  value: string;
  error?: string;
  type?: "text" | "email" | "password";
  autoComplete?: string;
  onChange: (value: string) => void;
}) {
  const errorId = `${id}-error`;
  return (
    <Label className="field" htmlFor={id}>
      <span className="field-label">{label}</span>
      <Input id={id} type={type} value={value} autoComplete={autoComplete} aria-invalid={error ? "true" : undefined} aria-describedby={error ? errorId : undefined} onChange={(event) => onChange(event.target.value)} />
      {error ? <span id={errorId} className="field-error">{error}</span> : null}
    </Label>
  );
}

function RoleChoiceGroup({ roles, selected, error, onToggle }: { roles: SecurityRole[]; selected: string[]; error?: string; onToggle: (roleId: string) => void }) {
  return (
    <fieldset className="security-choice-group security-permission-grid" aria-invalid={error ? "true" : undefined}>
      <legend>Roles</legend>
      {roles.map((role) => <label className="security-choice" key={role.id}><input type="checkbox" checked={selected.includes(role.id)} onChange={() => onToggle(role.id)} /><span>{role.name}</span></label>)}
      {error ? <span className="field-error">{error}</span> : null}
    </fieldset>
  );
}

function RolePermissionMatrix({ roles, form, errors, onFormChange, onErrorsChange, onSave, onDisable }: {
  roles: SecurityRole[];
  form: { id: string; name: string; permissions: string[]; isEnabled: boolean };
  errors: CustomRoleFieldErrors;
  onFormChange: (form: { id: string; name: string; permissions: string[]; isEnabled: boolean }) => void;
  onErrorsChange: (errors: CustomRoleFieldErrors) => void;
  onSave: () => void;
  onDisable: (roleId: string) => void;
}) {
  const selectedRole = roles.find((role) => role.id === form.id);
  const isReadOnly = selectedRole?.isSystem ?? false;
  const setPermission = (permission: string, allowed: boolean) => {
    if (isReadOnly) return;
    const permissions = allowed
      ? Array.from(new Set([...form.permissions, permission]))
      : form.permissions.filter((value) => value !== permission);
    onFormChange({ ...form, permissions });
    onErrorsChange({ ...errors, permissions: undefined });
  };

  return (
    <section className="role-permission-workspace">
      <aside className="role-picker" aria-label="Roles">
        <div className="role-picker-heading"><div><p className="eyebrow">Roles</p><h3>Access bundles</h3></div><Button variant="secondary" onClick={() => { onFormChange({ id: "", name: "", permissions: [], isEnabled: true }); onErrorsChange({}); }}>New</Button></div>
        <div className="role-picker-list">
          {roles.map((role) => <button key={role.id} type="button" className={form.id === role.id ? "role-picker-item role-picker-item-active" : "role-picker-item"} onClick={() => { onFormChange({ id: role.id, name: role.name, permissions: role.permissions, isEnabled: role.isEnabled }); onErrorsChange({}); }}><span>{role.name}</span><small>{role.isSystem ? "System" : "Custom"}</small></button>)}
        </div>
      </aside>
      <div className="permission-matrix">
        <div className="role-editor-heading">
          <div className="role-editor-fields">
            <IdentityField id="role-id" label="Role id" value={form.id} error={errors.id} onChange={(id) => { onFormChange({ ...form, id }); onErrorsChange({ ...errors, id: undefined }); }} />
            <IdentityField id="role-name" label="Role name" value={form.name} error={errors.name} onChange={(name) => { onFormChange({ ...form, name }); onErrorsChange({ ...errors, name: undefined }); }} />
          </div>
          <div className="role-editor-actions">
            {isReadOnly ? <Badge variant="secondary">System role · read only</Badge> : <Button onClick={onSave}>Save role</Button>}
            {selectedRole && !selectedRole.isSystem ? <Button variant="destructive" disabled={!selectedRole.isEnabled} onClick={() => onDisable(selectedRole.id)}>Disable</Button> : null}
          </div>
        </div>
        {errors.permissions ? <span className="field-error">{errors.permissions}</span> : null}
        <div className="permission-group-list">
          {permissionGroups.map((group) => {
            const allAllowed = group.permissions.every((permission) => form.permissions.includes(permission));
            return <section className="permission-group-card" key={group.id}>
              <div className="permission-row permission-group-row"><strong>{group.name}</strong><PermissionDecision allowed={allAllowed} disabled={isReadOnly} onAllow={() => group.permissions.forEach((permission) => setPermission(permission, true))} onDeny={() => group.permissions.forEach((permission) => setPermission(permission, false))} /></div>
              {group.permissions.map((permission) => <div className="permission-row" key={permission}><code>{permission}</code><PermissionDecision allowed={form.permissions.includes(permission)} disabled={isReadOnly} onAllow={() => setPermission(permission, true)} onDeny={() => setPermission(permission, false)} /></div>)}
            </section>;
          })}
        </div>
      </div>
    </section>
  );
}

function PermissionDecision({ allowed, disabled, onAllow, onDeny }: { allowed: boolean; disabled: boolean; onAllow: () => void; onDeny: () => void }) {
  return <div className="permission-decision" aria-label={allowed ? "Allowed" : "Not allowed"}>
    <button type="button" className={allowed ? "permission-toggle permission-allow permission-toggle-active" : "permission-toggle permission-allow"} disabled={disabled} aria-label="Allow" aria-pressed={allowed} onClick={onAllow}>✓</button>
    <button type="button" className={!allowed ? "permission-toggle permission-deny permission-toggle-active" : "permission-toggle permission-deny"} disabled={disabled} aria-label="Do not allow" aria-pressed={!allowed} onClick={onDeny}>×</button>
  </div>;
}

function RecoveryBanner({ notice, onRetry, onDismiss }: { notice: RecoveryNotice; onRetry?: () => void; onDismiss?: () => void }) {
  return (
    <div className="recovery-banner recovery-banner-error" role="alert">
      <span>{notice.message}{notice.retryable ? " Your current form values are still available." : ""}</span>
      {notice.retryable && onRetry ? <Button variant="secondary" onClick={onRetry}>Retry</Button> : null}
      {onDismiss ? <Button variant="ghost" onClick={onDismiss}>Dismiss</Button> : null}
    </div>
  );
}

function SecurityItem({ title, enabled, badge, children }: { title: string; enabled: boolean; badge?: string; children: ReactNode }) {
  return (
    <div className="security-assignment-item">
      <div className="security-assignment-item-header"><strong>{title}</strong><div className="security-badge-row">{badge ? <Badge variant="secondary">{badge}</Badge> : null}<Badge variant={enabled ? "default" : "destructive"}>{enabled ? "Enabled" : "Disabled"}</Badge></div></div>
      {children}
    </div>
  );
}

function upsertBy<T extends Record<K, string>, K extends keyof T>(items: T[], nextItem: T, key: K): T[] {
  return [...items.filter((item) => item[key] !== nextItem[key]), nextItem].sort((left, right) => String(left[key]).localeCompare(String(right[key])));
}

function createInternalUserId(): string {
  return `user-${crypto.randomUUID()}`;
}

function toRecoveryNotice(error: unknown, fallbackMessage: string) {
  const message = error instanceof ApiError ? toFriendlyErrorMessage(error.errorCode, error.message) : fallbackMessage;
  return createRecoveryNotice(error, message);
}
