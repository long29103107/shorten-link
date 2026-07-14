import { Fragment, useEffect, useRef, useState } from "react";
import { ApiError } from "../api/http";
import {
  activateShortLink,
  deactivateShortLink,
  deleteShortLink,
  listShortLinks,
  updateShortLink
} from "../api/shortLinksApi";
import type { ShortLinkAdminItem } from "../types";
import { formatDateTime, toFriendlyErrorMessage } from "../types";
import { Badge } from "../../../shared/components/ui/badge";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../../../shared/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger
} from "../../../shared/components/ui/dropdown-menu";
import { Input } from "../../../shared/components/ui/input";
import { Label } from "../../../shared/components/ui/label";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from "../../../shared/components/ui/table";

export function ShortLinkAdminPage() {
  const [links, setLinks] = useState<ShortLinkAdminItem[]>([]);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [busyCode, setBusyCode] = useState<string | null>(null);
  const [copiedCode, setCopiedCode] = useState<string | null>(null);
  const [openMenuCode, setOpenMenuCode] = useState<string | null>(null);
  const [editingCode, setEditingCode] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({ originalUrl: "", expiredAtLocal: "" });
  const [tooltip, setTooltip] = useState<{ text: string; x: number; y: number } | null>(null);
  const copyFeedbackTimeoutRef = useRef<number | null>(null);

  const loadLinks = async () => {
    setIsLoading(true);
    setErrorMessage(null);

    try {
      setLinks(await listShortLinks(100));
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("We could not load links right now.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadLinks();

    return () => {
      if (copyFeedbackTimeoutRef.current !== null) {
        window.clearTimeout(copyFeedbackTimeoutRef.current);
      }
    };
  }, []);

  const handleCopy = async (link: ShortLinkAdminItem, trigger: HTMLElement) => {
    try {
      await navigator.clipboard.writeText(link.shortUrl);
      const rect = trigger.getBoundingClientRect();
      if (copyFeedbackTimeoutRef.current !== null) {
        window.clearTimeout(copyFeedbackTimeoutRef.current);
      }

      setCopiedCode(link.code);
      setTooltip({
        text: "Copied",
        x: rect.left,
        y: rect.top
      });
      copyFeedbackTimeoutRef.current = window.setTimeout(() => {
        setCopiedCode(null);
        setTooltip(null);
        copyFeedbackTimeoutRef.current = null;
      }, 1500);
    } catch {
      setErrorMessage("Clipboard access was blocked, so the URL could not be copied.");
    }
  };

  const handleDeactivate = async (code: string) => {
    setBusyCode(code);
    setErrorMessage(null);

    try {
      const response = await deactivateShortLink(code);
      setLinks((current) =>
        current.map((link) =>
          link.code === response.code ? { ...link, isActive: response.isActive } : link
        )
      );
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("The link could not be deactivated.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  const handleActivate = async (code: string) => {
    setBusyCode(code);
    setErrorMessage(null);

    try {
      const response = await activateShortLink(code);
      setLinks((current) =>
        current.map((link) =>
          link.code === response.code ? { ...link, isActive: response.isActive } : link
        )
      );
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("The link could not be activated.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  const startEdit = (link: ShortLinkAdminItem) => {
    setEditingCode(link.code);
    setOpenMenuCode(null);
    setErrorMessage(null);
    setEditForm({
      originalUrl: link.originalUrl,
      expiredAtLocal: toDateTimeLocalValue(link.expiredAtUtc)
    });
  };

  const handleSaveEdit = async (code: string) => {
    setBusyCode(code);
    setErrorMessage(null);

    try {
      const updated = await updateShortLink(code, {
        originalUrl: editForm.originalUrl.trim(),
        expiredAtUtc: editForm.expiredAtLocal
          ? new Date(editForm.expiredAtLocal).toISOString()
          : undefined
      });
      setLinks((current) =>
        current.map((link) => (link.code === updated.code ? updated : link))
      );
      setEditingCode(null);
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("The link could not be updated.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  const handleDelete = async (code: string) => {
    setBusyCode(code);
    setOpenMenuCode(null);
    setErrorMessage(null);

    try {
      const response = await deleteShortLink(code);
      setLinks((current) => current.filter((link) => link.code !== response.code));
      if (editingCode === response.code) {
        setEditingCode(null);
      }
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("The link could not be deleted.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  return (
    <Card className="admin-panel">
      <CardHeader className="panel-heading-wide">
        <div className="card-title-row">
          <span className="card-glyph" aria-hidden="true" />
          <div>
          <p className="eyebrow">Admin</p>
          <CardTitle>Manage generated short links.</CardTitle>
          </div>
        </div>
        <Button variant="secondary" onClick={loadLinks}>
          {isLoading ? "Refreshing..." : "Refresh"}
        </Button>
      </CardHeader>

      <CardContent>
      {errorMessage ? <p className="feedback feedback-error">{errorMessage}</p> : null}

      {isLoading ? <p className="muted-copy">Loading links...</p> : null}

      {!isLoading && links.length === 0 ? (
        <p className="muted-copy">No short links yet. Create one first, then manage it here.</p>
      ) : null}

      {!isLoading && links.length > 0 ? (
        <div className="admin-table-wrap">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Destination</TableHead>
                <TableHead>Created</TableHead>
                <TableHead>Expiry</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {links.map((link) => (
                <Fragment key={link.code}>
                <TableRow>
                  <TableCell>
                    <a href={link.shortUrl} target="_blank" rel="noreferrer">
                      {link.code}
                    </a>
                  </TableCell>
                  <TableCell className="admin-url-cell">
                    <a
                      className="destination-link"
                      href={link.originalUrl}
                      target="_blank"
                      rel="noreferrer"
                      onBlur={() => setTooltip(null)}
                      onFocus={(event) => {
                        const rect = event.currentTarget.getBoundingClientRect();
                        setTooltip({
                          text: link.originalUrl,
                          x: rect.left,
                          y: rect.top
                        });
                      }}
                      onMouseEnter={(event) =>
                        setTooltip({
                          text: link.originalUrl,
                          x: event.clientX,
                          y: event.clientY
                        })
                      }
                      onMouseLeave={() => setTooltip(null)}
                      onMouseMove={(event) =>
                        setTooltip({
                          text: link.originalUrl,
                          x: event.clientX,
                          y: event.clientY
                        })
                      }
                    >
                      {link.originalUrl}
                    </a>
                  </TableCell>
                  <TableCell>{formatDateTime(link.createdAtUtc)}</TableCell>
                  <TableCell>{formatDateTime(link.expiredAtUtc)}</TableCell>
                  <TableCell>
                    <Badge variant={link.isActive ? "default" : "destructive"}>
                      {link.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="admin-row-actions">
                      <button
                        className={copiedCode === link.code ? "copy-icon-button copy-icon-button-done" : "copy-icon-button"}
                        type="button"
                        disabled={copiedCode === link.code}
                        aria-label={`Copy short URL for ${link.code}`}
                        title={copiedCode === link.code ? "Copied" : "Copy"}
                        onClick={(event) => handleCopy(link, event.currentTarget)}
                      >
                        <span aria-hidden="true" />
                      </button>
                      <DropdownMenu>
                        <DropdownMenuTrigger
                          aria-expanded={openMenuCode === link.code}
                          aria-label={`Actions for ${link.code}`}
                          onClick={() =>
                            setOpenMenuCode((current) =>
                              current === link.code ? null : link.code
                            )
                          }
                        >
                          ...
                        </DropdownMenuTrigger>
                        {openMenuCode === link.code ? (
                          <DropdownMenuContent>
                            <DropdownMenuItem onClick={() => startEdit(link)}>
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              disabled={busyCode === link.code}
                              onClick={() => {
                                setOpenMenuCode(null);
                                if (link.isActive) {
                                  void handleDeactivate(link.code);
                                } else {
                                  void handleActivate(link.code);
                                }
                              }}
                            >
                              {busyCode === link.code
                                ? "Updating"
                                : link.isActive
                                  ? "Deactivate"
                                  : "Activate"}
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              className="danger-link"
                              disabled={busyCode === link.code}
                              onClick={() => handleDelete(link.code)}
                            >
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        ) : null}
                      </DropdownMenu>
                    </div>
                  </TableCell>
                </TableRow>
                {editingCode === link.code ? (
                  <TableRow className="edit-row">
                    <TableCell colSpan={6}>
                      <div className="edit-panel">
                        <Label className="field">
                          <span className="field-label">Destination URL</span>
                          <Input
                            type="url"
                            value={editForm.originalUrl}
                            onChange={(event) =>
                              setEditForm((current) => ({
                                ...current,
                                originalUrl: event.target.value
                              }))
                            }
                          />
                        </Label>
                        <Label className="field">
                          <span className="field-label">Expiry</span>
                          <Input
                            type="datetime-local"
                            value={editForm.expiredAtLocal}
                            onChange={(event) =>
                              setEditForm((current) => ({
                                ...current,
                                expiredAtLocal: event.target.value
                              }))
                            }
                          />
                        </Label>
                        <div className="edit-actions">
                          <Button
                            disabled={busyCode === link.code}
                            onClick={() => handleSaveEdit(link.code)}
                          >
                            {busyCode === link.code ? "Saving" : "Save"}
                          </Button>
                          <Button variant="secondary" onClick={() => setEditingCode(null)}>
                            Cancel
                          </Button>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : null}
                </Fragment>
              ))}
            </TableBody>
          </Table>
        </div>
      ) : null}
      {tooltip ? (
        <div
          className="floating-tooltip"
          style={{
            left: tooltip.x + 12,
            top: tooltip.y - 12
          }}
        >
          {tooltip.text}
        </div>
      ) : null}
      </CardContent>
    </Card>
  );
}

function toDateTimeLocalValue(value: string | null): string {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}
