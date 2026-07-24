import { useEffect, useState } from "react";
import {
  deleteShortLinkShare,
  listShortLinkShares,
  upsertShortLinkShare
} from "../api/shortLinksApi";
import type { ShortLinkAdminItem, ShortLinkShare } from "../types";
import { ConfirmDialog } from "../../../shared/components/ConfirmDialog";
import { Button } from "../../../shared/components/ui/button";
import { Input } from "../../../shared/components/ui/input";
import { Label } from "../../../shared/components/ui/label";

type ShortLinkShareDialogProps = {
  link: ShortLinkAdminItem | null;
  onClose: () => void;
};

export function ShortLinkShareDialog({ link, onClose }: ShortLinkShareDialogProps) {
  const [shares, setShares] = useState<ShortLinkShare[]>([]);
  const [username, setUsername] = useState("");
  const [access, setAccess] = useState<"View" | "Edit">("View");
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pendingRemoval, setPendingRemoval] = useState<ShortLinkShare | null>(null);

  useEffect(() => {
    if (!link) return;
    setIsLoading(true);
    setError(null);
    void listShortLinkShares(link.code)
      .then((result) => setShares(result.items))
      .catch(() => setError("Sharing information could not be loaded."))
      .finally(() => setIsLoading(false));
  }, [link]);

  if (!link) return null;

  const saveShare = async () => {
    if (!username.trim()) {
      setError("Enter the username to share with.");
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      const saved = await upsertShortLinkShare(link.code, username.trim(), access);
      setShares((current) => [
        ...current.filter((share) => share.userId !== saved.userId),
        saved
      ]);
      setUsername("");
      setAccess("View");
    } catch {
      setError("The user could not be added. Check the username and try again.");
    } finally {
      setIsSaving(false);
    }
  };

  const removeShare = async () => {
    if (!pendingRemoval) return;
    setIsSaving(true);
    setError(null);
    try {
      await deleteShortLinkShare(link.code, pendingRemoval.userId);
      setShares((current) => current.filter((share) => share.userId !== pendingRemoval.userId));
      setPendingRemoval(null);
    } catch {
      setError("Shared access could not be removed.");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <>
      <div className="dialog-backdrop" role="presentation">
        <div className="edit-dialog share-dialog" role="dialog" aria-modal="true" aria-labelledby="share-dialog-title">
          <div>
            <p className="eyebrow">Sharing</p>
            <h2 id="share-dialog-title">Share {link.code}</h2>
            <p className="muted-copy">View can inspect the link and analytics. Edit can also update and activate or deactivate it.</p>
          </div>

          <div className="share-form">
            <Label className="field">
              <span className="field-label">Username</span>
              <Input
                placeholder="user@example.com"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
              />
            </Label>
            <Label className="field">
              <span className="field-label">Access</span>
              <select value={access} onChange={(event) => setAccess(event.target.value as "View" | "Edit")}>
                <option value="View">View</option>
                <option value="Edit">Edit</option>
              </select>
            </Label>
            <Button disabled={isSaving} onClick={() => void saveShare()}>
              {isSaving ? "Saving..." : "Add or update"}
            </Button>
          </div>

          {error ? <p className="field-error">{error}</p> : null}
          <div className="share-list">
            {isLoading ? <p className="muted-copy">Loading shared access...</p> : null}
            {!isLoading && shares.length === 0 ? <p className="muted-copy">This link is private.</p> : null}
            {shares.map((share) => (
              <div className="share-list-item" key={share.userId}>
                <div>
                  <strong>{share.displayName ?? share.username ?? share.userId}</strong>
                  {share.username ? <small>@{share.username}</small> : null}
                </div>
                <span>{share.access}</span>
                <Button variant="ghost" disabled={isSaving} onClick={() => setPendingRemoval(share)}>
                  Remove
                </Button>
              </div>
            ))}
          </div>

          <div className="dialog-actions">
            <Button variant="secondary" disabled={isSaving} onClick={onClose}>Close</Button>
          </div>
        </div>
      </div>
      <ConfirmDialog
        open={pendingRemoval !== null}
        title="Remove shared access?"
        description={`Remove access for ${pendingRemoval?.displayName ?? pendingRemoval?.username ?? "this user"}?`}
        confirmLabel="Remove access"
        cancelLabel="Cancel"
        variant="destructive"
        onConfirm={() => void removeShare()}
        onCancel={() => setPendingRemoval(null)}
      />
    </>
  );
}
