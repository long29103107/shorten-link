import { useEffect, useMemo, useState } from "react";
import type { Dispatch, ReactNode, SetStateAction } from "react";
import {
  createUserApiKey,
  disableCustomSecurityRole,
  disableSecurityAssignment,
  disableSecurityUser,
  disableUserApiKey,
  listSecurityAssignments,
  listSecurityRoles,
  listSecurityUsers,
  listUserApiKeys,
  renameUserApiKey,
  upsertCustomSecurityRole,
  upsertSecurityAssignment,
  upsertSecurityUser
} from "../api/shortLinksApi";
import { getAdminPermissionState, getStoredCurrentUser, shortLinkPermissions } from "../api/adminSecurity";
import { ApiError } from "../api/http";
import type {
  SecurityAssignment,
  SecurityRole,
  SecurityUser,
  SecurityUserApiKey
} from "../types";
import { formatDateTime, toFriendlyErrorMessage } from "../types";
import { Badge } from "../../../shared/components/ui/badge";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../../../shared/components/ui/card";
import { EmptyState } from "../../../shared/components/EmptyState";
import { Input } from "../../../shared/components/ui/input";
import { Label } from "../../../shared/components/ui/label";
import { showToast } from "../../../shared/toast";
import {
  createRecoveryNotice,
  resolveOneTimeSecret,
  type RecoveryNotice
} from "../../../shared/api/recovery";

type Tab = "users" | "roles" | "apiKeys" | "assignments";

const permissionOptions = Object.values(shortLinkPermissions);
const systemRoleOptions = ["Owner", "Admin", "Editor", "Viewer"];

export function SecurityManagementPage() {
  const adminPermissions = getAdminPermissionState();
  const currentUser = getStoredCurrentUser();
  const [activeTab, setActiveTab] = useState<Tab>("users");
  const [isLoading, setIsLoading] = useState(false);
  const [readFailure, setReadFailure] = useState<RecoveryNotice | null>(null);
  const [actionFailure, setActionFailure] = useState<RecoveryNotice | null>(null);
  const [users, setUsers] = useState<SecurityUser[]>([]);
  const [systemRoles, setSystemRoles] = useState<SecurityRole[]>([]);
  const [customRoles, setCustomRoles] = useState<SecurityRole[]>([]);
  const [apiKeys, setApiKeys] = useState<SecurityUserApiKey[]>([]);
  const [assignments, setAssignments] = useState<SecurityAssignment[]>([]);
  const [rawApiKey, setRawApiKey] = useState<string | null>(null);
  const [userForm, setUserForm] = useState({
    id: "",
    username: "",
    displayName: "",
    password: "",
    roleIds: [] as string[],
    isEnabled: true
  });
  const [roleForm, setRoleForm] = useState({
    id: "",
    name: "",
    permissions: [] as string[],
    isEnabled: true
  });
  const [apiKeyName, setApiKeyName] = useState("");
  const [assignmentForm, setAssignmentForm] = useState({
    name: "",
    credentialKey: "",
    roles: [] as string[],
    permissions: [] as string[],
    isEnabled: true
  });

  const roleOptions = useMemo(
    () => [...systemRoleOptions, ...customRoles.map((role) => role.id)],
    [customRoles]
  );

  const loadSecurity = async () => {
    setIsLoading(true);
    setReadFailure(null);
    setRawApiKey(resolveOneTimeSecret({ type: "refreshed" }));

    try {
      const [rolesResult, keysResult] = await Promise.all([
        adminPermissions.canManageSecurityAssignments ? listSecurityRoles() : Promise.resolve({ systemRoles: [], customRoles: [] }),
        currentUser ? listUserApiKeys() : Promise.resolve({ items: [] })
      ]);

      setSystemRoles(rolesResult.systemRoles);
      setCustomRoles(rolesResult.customRoles);
      setApiKeys(keysResult.items);

      if (adminPermissions.canManageSecurityAssignments) {
        const [usersResult, assignmentsResult] = await Promise.all([
          listSecurityUsers(),
          listSecurityAssignments()
        ]);
        setUsers(usersResult.items);
        setAssignments(assignmentsResult.items);
      }
    } catch (error) {
      setReadFailure(toRecoveryNotice(error, "Security data could not be loaded."));
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadSecurity();
  }, []);

  const toggleListValue = <TForm extends Record<string, unknown>, TField extends keyof TForm & string>(
    setter: Dispatch<SetStateAction<TForm>>,
    field: TField,
    value: string
  ) => {
    setter((current) => {
      const currentValues = Array.isArray(current[field]) ? current[field] as string[] : [];
      const values = new Set(currentValues);
      values.has(value) ? values.delete(value) : values.add(value);
      return { ...current, [field]: Array.from(values) };
    });
  };

  const saveUser = async () => {
    if (!userForm.id.trim() || !userForm.username.trim() || !userForm.displayName.trim()) {
      setActionFailure({ message: "Complete the user fields.", retryable: false });
      return;
    }

    setActionFailure(null);
    try {
      const user = await upsertSecurityUser({
        id: userForm.id.trim(),
        username: userForm.username.trim(),
        displayName: userForm.displayName.trim(),
        password: userForm.password.trim() || null,
        roleIds: userForm.roleIds,
        isEnabled: userForm.isEnabled
      });
      setUsers((current) => upsertBy(current, user, "id"));
      setUserForm({ id: "", username: "", displayName: "", password: "", roleIds: [], isEnabled: true });
      showToast({ title: "User saved", message: user.username, variant: "success" });
    } catch (error) {
      setActionFailure(toRecoveryNotice(error, "User could not be saved."));
    }
  };

  const saveRole = async () => {
    if (!roleForm.id.trim() || !roleForm.name.trim()) {
      setActionFailure({ message: "Complete the custom role fields.", retryable: false });
      return;
    }

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
      setActionFailure(toRecoveryNotice(error, "Role could not be saved."));
    }
  };

  const createApiKey = async () => {
    if (!apiKeyName.trim()) {
      setActionFailure({ message: "Name this API key.", retryable: false });
      return;
    }

    setActionFailure(null);
    setRawApiKey(resolveOneTimeSecret({ type: "request-started" }));
    try {
      const created = await createUserApiKey(apiKeyName.trim());
      setApiKeys((current) => upsertBy(current, created.apiKey, "id"));
      setRawApiKey(resolveOneTimeSecret({ type: "created", secret: created.rawApiKey }));
      setApiKeyName("");
      showToast({ title: "API key created", message: created.apiKey.displayName, variant: "success" });
    } catch (error) {
      setActionFailure(toRecoveryNotice(error, "API key could not be created."));
    }
  };

  const saveAssignment = async () => {
    if (!assignmentForm.name.trim() || !assignmentForm.credentialKey.trim()) {
      setActionFailure({ message: "Complete the assignment fields.", retryable: false });
      return;
    }

    setActionFailure(null);
    try {
      const assignment = await upsertSecurityAssignment({
        name: assignmentForm.name.trim(),
        credentialKey: assignmentForm.credentialKey.trim(),
        roles: assignmentForm.roles,
        permissions: assignmentForm.permissions,
        isEnabled: assignmentForm.isEnabled
      });
      setAssignments((current) => upsertBy(current, assignment, "credentialKeyHash"));
      setAssignmentForm({ name: "", credentialKey: "", roles: [], permissions: [], isEnabled: true });
      showToast({ title: "Assignment saved", message: assignment.name, variant: "success" });
    } catch (error) {
      setActionFailure(toRecoveryNotice(error, "Assignment could not be saved."));
    }
  };

  const runSecurityAction = async <T,>(
    operation: () => Promise<T>,
    applyResult: (result: T) => void,
    fallbackMessage: string
  ) => {
    setActionFailure(null);
    try {
      applyResult(await operation());
    } catch (error) {
      setActionFailure(toRecoveryNotice(error, fallbackMessage));
    }
  };

  if (!currentUser) {
    return (
      <Card className="admin-panel">
        <CardContent>
          <EmptyState title="Sign in required" description="Sign in to manage users, roles, and personal API keys." />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="admin-panel">
      <CardHeader className="panel-heading-wide">
        <div className="card-title-row">
          <span className="card-glyph" aria-hidden="true" />
          <div>
            <p className="eyebrow">Identity</p>
            <CardTitle>Manage users, roles, and API keys.</CardTitle>
          </div>
        </div>
        <Button variant="secondary" disabled={isLoading} onClick={() => void loadSecurity()}>
          Refresh
        </Button>
      </CardHeader>
      <CardContent>
        {readFailure ? (
          <div className="recovery-banner" role="alert">
            <span>{readFailure.message}</span>
            {readFailure.retryable ? (
              <Button variant="secondary" onClick={() => void loadSecurity()}>Retry</Button>
            ) : null}
          </div>
        ) : null}
        {actionFailure ? (
          <div className="recovery-banner recovery-banner-error" role="alert">
            <span>
              {actionFailure.message}
              {actionFailure.retryable ? " Your current form values are still available." : ""}
            </span>
            <Button variant="ghost" onClick={() => setActionFailure(null)}>Dismiss</Button>
          </div>
        ) : null}
        <div className="security-tabs" role="tablist" aria-label="Security sections">
          {(["users", "roles", "apiKeys", "assignments"] as Tab[]).map((tab) => (
            <Button
              key={tab}
              variant={activeTab === tab ? "default" : "secondary"}
              onClick={() => setActiveTab(tab)}
            >
              {tab === "apiKeys" ? "API keys" : tab[0].toUpperCase() + tab.slice(1)}
            </Button>
          ))}
        </div>

        {activeTab === "users" ? (
          <section className="security-dialog-grid">
            {adminPermissions.canManageSecurityAssignments ? (
              <div className="security-assignment-form">
                <h3>User</h3>
                <Input placeholder="id" value={userForm.id} onChange={(event) => setUserForm((value) => ({ ...value, id: event.target.value }))} />
                <Input placeholder="username" value={userForm.username} onChange={(event) => setUserForm((value) => ({ ...value, username: event.target.value }))} />
                <Input placeholder="display name" value={userForm.displayName} onChange={(event) => setUserForm((value) => ({ ...value, displayName: event.target.value }))} />
                <Input type="password" placeholder="password for create or replacement" value={userForm.password} onChange={(event) => setUserForm((value) => ({ ...value, password: event.target.value }))} />
                <ChoiceGroup values={roleOptions} selected={userForm.roleIds} onToggle={(role) => toggleListValue(setUserForm, "roleIds", role)} />
                <EnabledChoice checked={userForm.isEnabled} onChange={(checked) => setUserForm((value) => ({ ...value, isEnabled: checked }))} />
                <Button onClick={() => void saveUser()}>Save user</Button>
              </div>
            ) : <PermissionEmpty />}
            <ItemList
              items={users}
              empty="No users"
              render={(user) => (
                <SecurityItem key={user.id} title={user.username} enabled={user.isEnabled}>
                  <p>{user.displayName}</p>
                  <p>{user.roleIds.join(", ") || "No roles"}</p>
                  <p>{formatDateTime(user.createdAtUtc)}</p>
                  <Button variant="secondary" onClick={() => setUserForm({ id: user.id, username: user.username, displayName: user.displayName, password: "", roleIds: user.roleIds, isEnabled: user.isEnabled })}>Edit</Button>
                  <Button variant="destructive" disabled={!user.isEnabled} onClick={() => void runSecurityAction(
                    () => disableSecurityUser(user.id),
                    (result) => setUsers((current) => current.map((item) => item.id === result.id ? { ...item, isEnabled: result.isEnabled } : item)),
                    "User could not be disabled."
                  )}>Disable</Button>
                </SecurityItem>
              )}
            />
          </section>
        ) : null}

        {activeTab === "roles" ? (
          <section className="security-dialog-grid">
            {adminPermissions.canManageSecurityAssignments ? (
              <div className="security-assignment-form">
                <h3>Custom role</h3>
                <Input placeholder="id" value={roleForm.id} onChange={(event) => setRoleForm((value) => ({ ...value, id: event.target.value }))} />
                <Input placeholder="name" value={roleForm.name} onChange={(event) => setRoleForm((value) => ({ ...value, name: event.target.value }))} />
                <ChoiceGroup values={permissionOptions} selected={roleForm.permissions} onToggle={(permission) => toggleListValue(setRoleForm, "permissions", permission)} />
                <EnabledChoice checked={roleForm.isEnabled} onChange={(checked) => setRoleForm((value) => ({ ...value, isEnabled: checked }))} />
                <Button onClick={() => void saveRole()}>Save role</Button>
              </div>
            ) : <PermissionEmpty />}
            <ItemList
              items={[...systemRoles, ...customRoles]}
              empty="No roles"
              render={(role) => (
                <SecurityItem key={role.id} title={role.name} enabled={role.isEnabled} badge={role.isSystem ? "System" : "Custom"}>
                  <p>{role.permissions.join(", ") || "No permissions"}</p>
                  {!role.isSystem ? <Button variant="secondary" onClick={() => setRoleForm({ id: role.id, name: role.name, permissions: role.permissions, isEnabled: role.isEnabled })}>Edit</Button> : null}
                  {!role.isSystem ? <Button variant="destructive" disabled={!role.isEnabled} onClick={() => void runSecurityAction(
                    () => disableCustomSecurityRole(role.id),
                    (result) => setCustomRoles((current) => current.map((item) => item.id === result.id ? { ...item, isEnabled: result.isEnabled } : item)),
                    "Role could not be disabled."
                  )}>Disable</Button> : null}
                </SecurityItem>
              )}
            />
          </section>
        ) : null}

        {activeTab === "apiKeys" ? (
          <section className="security-dialog-grid">
            <div className="security-assignment-form">
              <h3>Personal API key</h3>
              <Input placeholder="display name" value={apiKeyName} onChange={(event) => setApiKeyName(event.target.value)} />
              <Button onClick={() => void createApiKey()}>Create API key</Button>
              {rawApiKey ? (
                <div className="one-time-secret">
                  <p className="eyebrow">One-time key</p>
                  <code>{rawApiKey}</code>
                  <Button
                    variant="secondary"
                    onClick={() => setRawApiKey(resolveOneTimeSecret({ type: "dismissed" }))}
                  >
                    Dismiss
                  </Button>
                </div>
              ) : null}
            </div>
            <ItemList
              items={apiKeys}
              empty="No API keys"
              render={(apiKey) => (
                <SecurityItem key={apiKey.id} title={apiKey.displayName} enabled={apiKey.isEnabled}>
                  <p>{formatDateTime(apiKey.createdAtUtc)}</p>
                  <Button variant="secondary" onClick={() => {
                    const nextName = window.prompt("API key name", apiKey.displayName);
                    if (nextName) {
                      void runSecurityAction(
                        () => renameUserApiKey(apiKey.id, nextName),
                        (result) => setApiKeys((current) => upsertBy(current, result, "id")),
                        "API key could not be renamed."
                      );
                    }
                  }}>Rename</Button>
                  <Button variant="destructive" disabled={!apiKey.isEnabled} onClick={() => void runSecurityAction(
                    () => disableUserApiKey(apiKey.id),
                    (result) => setApiKeys((current) => current.map((item) => item.id === result.id ? { ...item, isEnabled: result.isEnabled } : item)),
                    "API key could not be disabled."
                  )}>Disable</Button>
                </SecurityItem>
              )}
            />
          </section>
        ) : null}

        {activeTab === "assignments" ? (
          <section className="security-dialog-grid">
            {adminPermissions.canManageSecurityAssignments ? (
              <div className="security-assignment-form">
                <h3>Legacy assignment</h3>
                <Input placeholder="name" value={assignmentForm.name} onChange={(event) => setAssignmentForm((value) => ({ ...value, name: event.target.value }))} />
                <Input type="password" placeholder="credential key" value={assignmentForm.credentialKey} onChange={(event) => setAssignmentForm((value) => ({ ...value, credentialKey: event.target.value }))} />
                <ChoiceGroup values={systemRoleOptions} selected={assignmentForm.roles} onToggle={(role) => toggleListValue(setAssignmentForm, "roles", role)} />
                <ChoiceGroup values={permissionOptions} selected={assignmentForm.permissions} onToggle={(permission) => toggleListValue(setAssignmentForm, "permissions", permission)} />
                <EnabledChoice checked={assignmentForm.isEnabled} onChange={(checked) => setAssignmentForm((value) => ({ ...value, isEnabled: checked }))} />
                <Button onClick={() => void saveAssignment()}>Save assignment</Button>
              </div>
            ) : <PermissionEmpty />}
            <ItemList
              items={assignments}
              empty="No assignments"
              render={(assignment) => (
                <SecurityItem key={assignment.credentialKeyHash} title={assignment.name} enabled={assignment.isEnabled}>
                  <code>{assignment.credentialKeyHash}</code>
                  <p>{[...assignment.roles, ...assignment.permissions].join(", ") || "No grants"}</p>
                  <Button variant="destructive" disabled={!assignment.isEnabled} onClick={() => void runSecurityAction(
                    () => disableSecurityAssignment(assignment.credentialKeyHash),
                    (result) => setAssignments((current) => current.map((item) => item.credentialKeyHash === result.credentialKeyHash ? { ...item, isEnabled: result.isEnabled } : item)),
                    "Assignment could not be disabled."
                  )}>Disable</Button>
                </SecurityItem>
              )}
            />
          </section>
        ) : null}
      </CardContent>
    </Card>
  );
}

function ChoiceGroup({ values, selected, onToggle }: { values: readonly string[]; selected: string[]; onToggle: (value: string) => void }) {
  return (
    <fieldset className="security-choice-group security-permission-grid">
      {values.map((value) => (
        <label className="security-choice" key={value}>
          <input type="checkbox" checked={selected.includes(value)} onChange={() => onToggle(value)} />
          <span>{value}</span>
        </label>
      ))}
    </fieldset>
  );
}

function EnabledChoice({ checked, onChange }: { checked: boolean; onChange: (checked: boolean) => void }) {
  return (
    <label className="security-choice security-enabled-choice">
      <input type="checkbox" checked={checked} onChange={(event) => onChange(event.target.checked)} />
      <span>Enabled</span>
    </label>
  );
}

function PermissionEmpty() {
  return <EmptyState title="Permission required" description="This section requires security.assignments.manage." />;
}

function ItemList<T>({ items, empty, render }: { items: T[]; empty: string; render: (item: T) => ReactNode }) {
  if (items.length === 0) {
    return <EmptyState title={empty} description="Refresh or create an item to populate this section." />;
  }

  return <div className="security-assignment-items">{items.map(render)}</div>;
}

function SecurityItem({ title, enabled, badge, children }: { title: string; enabled: boolean; badge?: string; children: ReactNode }) {
  return (
    <div className="security-assignment-item">
      <div className="security-assignment-item-header">
        <strong>{title}</strong>
        <div className="security-badge-row">
          {badge ? <Badge variant="secondary">{badge}</Badge> : null}
          <Badge variant={enabled ? "default" : "destructive"}>{enabled ? "Enabled" : "Disabled"}</Badge>
        </div>
      </div>
      {children}
    </div>
  );
}

function upsertBy<T extends Record<K, string>, K extends keyof T>(items: T[], nextItem: T, key: K): T[] {
  return [
    ...items.filter((item) => item[key] !== nextItem[key]),
    nextItem
  ].sort((left, right) => String(left[key]).localeCompare(String(right[key])));
}

function toRecoveryNotice(error: unknown, fallbackMessage: string) {
  const message = error instanceof ApiError
    ? toFriendlyErrorMessage(error.errorCode, error.message)
    : fallbackMessage;
  return createRecoveryNotice(error, message);
}
