import { Fragment, useEffect, useRef, useState } from "react";
import { ApiError } from "../api/http";
import {
  activateShortLink,
  createShortLink,
  deactivateShortLink,
  deleteShortLink,
  getShortLinkAnalytics,
  listShortLinks,
  updateShortLink
} from "../api/shortLinksApi";
import { getAdminPermissionState } from "../api/adminSecurity";
import type { ShortLinkAdminItem, ShortLinkAnalytics, ShortLinkDiscoveryQuery } from "../types";
import { formatDateTime, toFriendlyErrorMessage } from "../types";
import { Badge } from "../../../shared/components/ui/badge";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "../../../shared/components/ui/card";
import { ConfirmDialog } from "../../../shared/components/ConfirmDialog";
import { EmptyState } from "../../../shared/components/EmptyState";
import { TableSkeleton } from "../../../shared/components/TableSkeleton";
import { showToast } from "../../../shared/toast";
import {
  createRecoveryNotice,
  shouldPreserveMutationContext,
  type RecoveryNotice
} from "../../../shared/api/recovery";
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
import { useClickOutside } from "../../../shared/hooks/useClickOutside";
import { ExpiryQuickPicks } from "../components/ExpiryQuickPicks";
import {
  defaultShortLinkDiscoveryQuery,
  createShortLinkDiscoveryChange,
  hasShortLinkDiscoveryCriteria,
  ShortLinkDiscoveryToolbar
} from "../components/ShortLinkDiscoveryToolbar";
import {
  hasShortLinkFieldErrors,
  mapShortLinkApiFieldErrors,
  validateShortLinkForm,
  type ShortLinkFieldErrors
} from "../validation";

type ShortLinkAdminPageProps = {
  onDirtyChange?: (isDirty: boolean) => void;
};

type ConfirmAction = {
  title: string;
  description: string;
  confirmLabel: string;
  variant?: "default" | "destructive";
  onConfirm: () => void;
};

export function ShortLinkAdminPage({ onDirtyChange }: ShortLinkAdminPageProps) {
  const [links, setLinks] = useState<ShortLinkAdminItem[]>([]);
  const [actionError, setActionError] = useState<string | null>(null);
  const [listFailure, setListFailure] = useState<(RecoveryNotice & { pageNumber: number }) | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [busyCode, setBusyCode] = useState<string | null>(null);
  const [isBulkDeleting, setIsBulkDeleting] = useState(false);
  const [isBulkUpdating, setIsBulkUpdating] = useState(false);
  const [isCreating, setIsCreating] = useState(false);
  const [copiedCode, setCopiedCode] = useState<string | null>(null);
  const [openMenuCode, setOpenMenuCode] = useState<string | null>(null);
  const [editingCode, setEditingCode] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({ originalUrl: "", expiredAtLocal: "" });
  const [initialEditForm, setInitialEditForm] = useState({ originalUrl: "", expiredAtLocal: "" });
  const [fieldErrors, setFieldErrors] = useState<ShortLinkFieldErrors>({});
  const [editorRequestError, setEditorRequestError] = useState<string | null>(null);
  const [tooltip, setTooltip] = useState<{ text: string; x: number; y: number } | null>(null);
  const [confirmAction, setConfirmAction] = useState<ConfirmAction | null>(null);
  const [selectedCodes, setSelectedCodes] = useState<Set<string>>(() => new Set());
  const [pageSize, setPageSize] = useState(25);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(1);
  const [discoveryQuery, setDiscoveryQuery] = useState<ShortLinkDiscoveryQuery>(
    defaultShortLinkDiscoveryQuery
  );
  const [isPageSizeMenuOpen, setIsPageSizeMenuOpen] = useState(false);
  const [analyticsCode, setAnalyticsCode] = useState<string | null>(null);
  const [analyticsData, setAnalyticsData] = useState<ShortLinkAnalytics | null>(null);
  const [analyticsError, setAnalyticsError] = useState<string | null>(null);
  const [isAnalyticsRetryable, setIsAnalyticsRetryable] = useState(false);
  const [isAnalyticsLoading, setIsAnalyticsLoading] = useState(false);
  const copyFeedbackTimeoutRef = useRef<number | null>(null);
  const pageSizeMenuRef = useRef<HTMLLabelElement | null>(null);
  const adminPermissions = getAdminPermissionState();

  const selectedLinks = links.filter((link) => selectedCodes.has(link.code));
  const selectedCount = selectedCodes.size;
  const allPageCodes = links.map((link) => link.code);
  const isPageSelected = allPageCodes.length > 0 && allPageCodes.every((code) => selectedCodes.has(code));
  const canBulkDeactivate = adminPermissions.canDeactivate && selectedLinks.some((link) => link.isActive);
  const canBulkActivate = adminPermissions.canActivate && selectedLinks.some((link) => !link.isActive);
  const canBulkDelete = adminPermissions.canDelete;
  const hasBulkActions = canBulkDeactivate || canBulkActivate || canBulkDelete;
  const shouldShowList = !isLoading && links.length > 0;
  const editingLink = editingCode
    ? links.find((link) => link.code === editingCode) ?? null
    : null;
  const isEditorOpen = isCreating || editingLink !== null;
  const hasEditChanges = isEditorOpen
    && (editForm.originalUrl !== initialEditForm.originalUrl
      || editForm.expiredAtLocal !== initialEditForm.expiredAtLocal);

  const loadLinks = async (nextPageNumber = pageNumber) => {
    setIsLoading(true);
    setListFailure(null);

    try {
      const result = await listShortLinks(pageSize, nextPageNumber, discoveryQuery);
      setLinks(result.items);
      setTotalCount(result.totalCount ?? result.items.length);
      setTotalPages(result.totalPages ?? 1);
      setSelectedCodes(new Set());
      setPageNumber(result.page ?? nextPageNumber);
    } catch (error) {
      const message = error instanceof ApiError
        ? toFriendlyErrorMessage(error.errorCode, error.message)
        : "We could not load links right now.";
      setListFailure({
        ...createRecoveryNotice(error, message),
        pageNumber: nextPageNumber
      });
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadLinks(1);

    return () => {
      if (copyFeedbackTimeoutRef.current !== null) {
        window.clearTimeout(copyFeedbackTimeoutRef.current);
      }
    };
  }, [pageSize, discoveryQuery]);

  useEffect(() => {
    onDirtyChange?.(hasEditChanges);
  }, [hasEditChanges, onDirtyChange]);

  useClickOutside(
    [pageSizeMenuRef],
    () => setIsPageSizeMenuOpen(false),
    isPageSizeMenuOpen
  );

  useEffect(() => {
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      if (!hasEditChanges) {
        return;
      }

      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [hasEditChanges]);

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
      setActionError("Clipboard access was blocked, so the URL could not be copied.");
    }
  };

  const handleDeactivate = async (code: string) => {
    setBusyCode(code);
    setActionError(null);

    try {
      const response = await deactivateShortLink(code);
      setLinks((current) =>
        current.map((link) =>
          link.code === response.code ? { ...link, isActive: response.isActive } : link
        )
      );
      showToast({
        title: "Short link deactivated",
        message: code,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError) {
        setActionError(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setActionError("The link could not be deactivated.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  const handleActivate = async (code: string) => {
    setBusyCode(code);
    setActionError(null);

    try {
      const response = await activateShortLink(code);
      setLinks((current) =>
        current.map((link) =>
          link.code === response.code ? { ...link, isActive: response.isActive } : link
        )
      );
      showToast({
        title: "Short link activated",
        message: code,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError) {
        setActionError(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setActionError("The link could not be activated.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  const startEdit = (link: ShortLinkAdminItem) => {
    if (!adminPermissions.canUpdate) {
      return;
    }

    setIsCreating(false);
    setEditingCode(link.code);
    setOpenMenuCode(null);
    setActionError(null);
    setEditorRequestError(null);
    setFieldErrors({});
    const nextForm = {
      originalUrl: link.originalUrl,
      expiredAtLocal: toDateTimeLocalValue(link.expiredAtUtc)
    };
    setEditForm(nextForm);
    setInitialEditForm(nextForm);
  };

  const startCreate = () => {
    if (!adminPermissions.canCreate) {
      return;
    }

    const nextForm = { originalUrl: "", expiredAtLocal: "" };
    setIsCreating(true);
    setEditingCode(null);
    setOpenMenuCode(null);
    setActionError(null);
    setEditorRequestError(null);
    setFieldErrors({});
    setEditForm(nextForm);
    setInitialEditForm(nextForm);
  };

  const closeEditor = () => {
    setIsCreating(false);
    setEditingCode(null);
    setFieldErrors({});
    setInitialEditForm({ originalUrl: "", expiredAtLocal: "" });
    setEditorRequestError(null);
  };

  const validateEditorForm = () => {
    return validateShortLinkForm(editForm);
  };

  const applyApiFieldError = (error: ApiError) => {
    const nextErrors = mapShortLinkApiFieldErrors(error.fieldErrors);
    setFieldErrors(nextErrors);
    return hasShortLinkFieldErrors(nextErrors);
  };

  const handleCreate = async () => {
    const nextErrors = validateEditorForm();
    if (hasShortLinkFieldErrors(nextErrors)) {
      setFieldErrors(nextErrors);
      return;
    }
    setFieldErrors({});

    const payload = {
      originalUrl: editForm.originalUrl.trim(),
      expiredAtUtc: new Date(editForm.expiredAtLocal).toISOString()
    };

    setBusyCode("__create__");
    setActionError(null);
    setEditorRequestError(null);

    try {
      const created = await createShortLink(payload);
      closeEditor();
      await loadLinks(1);
      showToast({
        title: "Short link created",
        message: created.code,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError && applyApiFieldError(error)) {
        return;
      }

      const message =
        error instanceof ApiError
          ? toFriendlyErrorMessage(error.errorCode, error.message)
          : "The link could not be created.";

      if (shouldPreserveMutationContext(error)) {
        setEditorRequestError(message);
        return;
      }

      closeEditor();
      showToast({
        title: "Create failed",
        message,
        variant: "error"
      });
    } finally {
      setBusyCode(null);
    }
  };

  const handleSaveEdit = async (code: string) => {
    const nextErrors = validateEditorForm();
    if (hasShortLinkFieldErrors(nextErrors)) {
      setFieldErrors(nextErrors);
      return;
    }
    setFieldErrors({});

    const payload = {
      originalUrl: editForm.originalUrl.trim(),
      expiredAtUtc: new Date(editForm.expiredAtLocal).toISOString()
    };

    setBusyCode(code);
    setActionError(null);
    setEditorRequestError(null);

    try {
      const updated = await updateShortLink(code, payload);
      setLinks((current) =>
        current.map((link) => (link.code === updated.code ? updated : link))
      );
      closeEditor();
      showToast({
        title: "Short link updated",
        message: code,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError && applyApiFieldError(error)) {
        return;
      }

      const message =
        error instanceof ApiError
          ? toFriendlyErrorMessage(error.errorCode, error.message)
          : "The link could not be updated.";

      if (shouldPreserveMutationContext(error)) {
        setEditorRequestError(message);
        return;
      }

      closeEditor();
      showToast({
        title: "Update failed",
        message,
        variant: "error"
      });
    } finally {
      setBusyCode(null);
    }
  };

  const handleDelete = async (code: string) => {
    setBusyCode(code);
    setOpenMenuCode(null);
    setActionError(null);

    try {
      const response = await deleteShortLink(code);
      setLinks((current) => current.filter((link) => link.code !== response.code));
      setSelectedCodes((current) => {
        const next = new Set(current);
        next.delete(response.code);
        return next;
      });
      if (editingCode === response.code) {
        setEditingCode(null);
      }
      if (analyticsCode === response.code) {
        closeAnalyticsPanel();
      }
      showToast({
        title: "Short link deleted",
        message: response.code,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError) {
        setActionError(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setActionError("The link could not be deleted.");
      }
    } finally {
      setBusyCode(null);
    }
  };

  const handleBulkDelete = async () => {
    const codes = Array.from(selectedCodes);
    if (codes.length === 0) {
      return;
    }

    setIsBulkDeleting(true);
    setActionError(null);

    try {
      await Promise.all(codes.map((code) => deleteShortLink(code)));
      setLinks((current) => current.filter((link) => !selectedCodes.has(link.code)));
      setSelectedCodes(new Set());
      if (editingCode && selectedCodes.has(editingCode)) {
        setEditingCode(null);
      }
      showToast({
        title: "Selected links deleted",
        message: `${codes.length} link${codes.length === 1 ? "" : "s"} removed`,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError) {
        setActionError(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setActionError("Selected links could not be deleted.");
      }
    } finally {
      setIsBulkDeleting(false);
    }
  };

  const handleBulkStatusChange = async (nextIsActive: boolean) => {
    const codes = Array.from(selectedCodes);
    if (codes.length === 0) {
      return;
    }

    setIsBulkUpdating(true);
    setActionError(null);

    try {
      const responses = await Promise.all(
        codes.map((code) =>
          nextIsActive ? activateShortLink(code) : deactivateShortLink(code)
        )
      );
      const updatedCodes = new Set(responses.map((response) => response.code));
      setLinks((current) =>
        current.map((link) =>
          updatedCodes.has(link.code) ? { ...link, isActive: nextIsActive } : link
        )
      );
      setSelectedCodes(new Set());
      showToast({
        title: nextIsActive ? "Selected links activated" : "Selected links deactivated",
        message: `${codes.length} link${codes.length === 1 ? "" : "s"} updated`,
        variant: "success"
      });
    } catch (error) {
      if (error instanceof ApiError) {
        setActionError(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setActionError(nextIsActive
          ? "Selected links could not be activated."
          : "Selected links could not be deactivated.");
      }
    } finally {
      setIsBulkUpdating(false);
    }
  };

  const requestDelete = (code: string) => {
    if (!adminPermissions.canDelete) {
      return;
    }

    setOpenMenuCode(null);
    setConfirmAction({
      title: "Delete short link?",
      description: `This will permanently delete ${code}. This action cannot be undone.`,
      confirmLabel: "Delete",
      variant: "destructive",
      onConfirm: () => void handleDelete(code)
    });
  };

  const requestStatusChange = (link: ShortLinkAdminItem) => {
    if ((link.isActive && !adminPermissions.canDeactivate)
      || (!link.isActive && !adminPermissions.canActivate)) {
      return;
    }

    setOpenMenuCode(null);
    setConfirmAction({
      title: link.isActive ? "Deactivate short link?" : "Activate short link?",
      description: link.isActive
        ? `Deactivate ${link.code}? Redirects for this link will stop working.`
        : `Activate ${link.code}? Redirects for this link will start working again.`,
      confirmLabel: link.isActive ? "Deactivate" : "Activate",
      variant: link.isActive ? "destructive" : "default",
      onConfirm: () => {
        if (link.isActive) {
          void handleDeactivate(link.code);
        } else {
          void handleActivate(link.code);
        }
      }
    });
  };

  const loadAnalytics = async (code: string) => {
    setAnalyticsData(null);
    setAnalyticsError(null);
    setIsAnalyticsRetryable(false);
    setIsAnalyticsLoading(true);

    try {
      const analytics = await getShortLinkAnalytics(code);
      setAnalyticsData(analytics);
    } catch (error) {
      if (error instanceof ApiError) {
        setAnalyticsError(toFriendlyErrorMessage(error.errorCode, error.message));
        setIsAnalyticsRetryable(error.retryable);
      } else {
        setAnalyticsError("Analytics could not be loaded.");
      }
    } finally {
      setIsAnalyticsLoading(false);
    }
  };

  const openAnalyticsPanel = (link: ShortLinkAdminItem) => {
    if (!adminPermissions.canReadAnalytics) {
      return;
    }

    setAnalyticsCode(link.code);
    setOpenMenuCode(null);
    void loadAnalytics(link.code);
  };

  const closeAnalyticsPanel = () => {
    setAnalyticsCode(null);
    setAnalyticsData(null);
    setAnalyticsError(null);
    setIsAnalyticsRetryable(false);
    setIsAnalyticsLoading(false);
  };

  const requestBulkDelete = () => {
    if (!adminPermissions.canDelete) {
      return;
    }

    setConfirmAction({
      title: "Delete selected links?",
      description: `This will permanently delete ${selectedCount} selected link${selectedCount === 1 ? "" : "s"}. This action cannot be undone.`,
      confirmLabel: "Delete selected",
      variant: "destructive",
      onConfirm: () => void handleBulkDelete()
    });
  };

  const requestBulkStatusChange = (nextIsActive: boolean) => {
    if ((nextIsActive && !adminPermissions.canActivate)
      || (!nextIsActive && !adminPermissions.canDeactivate)) {
      return;
    }

    setConfirmAction({
      title: nextIsActive ? "Activate selected links?" : "Deactivate selected links?",
      description: nextIsActive
        ? `Activate ${selectedCount} selected link${selectedCount === 1 ? "" : "s"}?`
        : `Deactivate ${selectedCount} selected link${selectedCount === 1 ? "" : "s"}? Redirects for these links will stop working.`,
      confirmLabel: nextIsActive ? "Activate selected" : "Deactivate selected",
      variant: nextIsActive ? "default" : "destructive",
      onConfirm: () => void handleBulkStatusChange(nextIsActive)
    });
  };

  const toggleSelected = (code: string) => {
    setSelectedCodes((current) => {
      const next = new Set(current);
      if (next.has(code)) {
        next.delete(code);
      } else {
        next.add(code);
      }

      return next;
    });
  };

  const togglePageSelected = () => {
    setSelectedCodes((current) => {
      const next = new Set(current);
      if (isPageSelected) {
        allPageCodes.forEach((code) => next.delete(code));
      } else {
        allPageCodes.forEach((code) => next.add(code));
      }

      return next;
    });
  };

  const goToPage = (nextPageNumber: number) => {
    void loadLinks(Math.max(1, Math.min(nextPageNumber, totalPages)));
  };

  const handleDiscoveryChange = (nextQuery: ShortLinkDiscoveryQuery) => {
    const change = createShortLinkDiscoveryChange(nextQuery);
    setPageNumber(change.pageNumber);
    setDiscoveryQuery(change.query);
  };

  const hasRowActions = (link: ShortLinkAdminItem) =>
    adminPermissions.canReadAnalytics
    || adminPermissions.canUpdate
    || (link.isActive ? adminPermissions.canDeactivate : adminPermissions.canActivate)
    || adminPermissions.canDelete;

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
        <div className="admin-header-actions">
          <Button
            disabled={!adminPermissions.canCreate}
            title={adminPermissions.canCreate ? "Create" : "Missing short_links.create permission"}
            onClick={startCreate}
          >
            Create
          </Button>
        </div>
      </CardHeader>

      <CardContent>
      <ShortLinkDiscoveryToolbar
        value={discoveryQuery}
        disabled={isLoading}
        onChange={handleDiscoveryChange}
      />

      {shouldShowList && selectedCount > 0 && hasBulkActions ? (
      <div className="admin-bulk-bar">
        <div className="admin-toolbar-group">
          {canBulkDeactivate ? (
          <Button
            variant="secondary"
            disabled={isBulkUpdating || isBulkDeleting}
            onClick={() => requestBulkStatusChange(false)}
          >
            {isBulkUpdating ? "Updating..." : `Deactivate selected (${selectedCount})`}
          </Button>
          ) : null}
          {canBulkActivate ? (
          <Button
            variant="secondary"
            disabled={isBulkUpdating || isBulkDeleting}
            onClick={() => requestBulkStatusChange(true)}
          >
            {isBulkUpdating ? "Updating..." : `Activate selected (${selectedCount})`}
          </Button>
          ) : null}
          {canBulkDelete ? (
          <Button
            variant="destructive"
            disabled={isBulkDeleting || isBulkUpdating}
            onClick={requestBulkDelete}
          >
            {isBulkDeleting ? "Deleting..." : `Delete selected (${selectedCount})`}
          </Button>
          ) : null}
          <Button
            variant="secondary"
            disabled={isBulkDeleting || isBulkUpdating}
            onClick={() => setSelectedCodes(new Set())}
          >
            Clear selected
          </Button>
        </div>
      </div>
      ) : null}

      {isLoading ? <TableSkeleton /> : null}

      {!isLoading && listFailure && links.length === 0 ? (
        <EmptyState
          title="Links could not be loaded"
          description={listFailure.message}
          action={listFailure.retryable
            ? <Button variant="secondary" onClick={() => void loadLinks(listFailure.pageNumber)}>Retry</Button>
            : undefined}
        />
      ) : null}

      {!isLoading && listFailure && links.length > 0 ? (
        <div className="recovery-banner" role="alert">
          <span>{listFailure.message}</span>
          {listFailure.retryable ? (
            <Button variant="secondary" onClick={() => void loadLinks(listFailure.pageNumber)}>
              Retry
            </Button>
          ) : null}
        </div>
      ) : null}

      {actionError ? (
        <div className="recovery-banner recovery-banner-error" role="alert">
          <span>{actionError}</span>
          <Button variant="ghost" onClick={() => setActionError(null)}>Dismiss</Button>
        </div>
      ) : null}

      {!isLoading && !listFailure && !shouldShowList ? (
        <EmptyState
          title={hasShortLinkDiscoveryCriteria(discoveryQuery) ? "No matching links" : "No data"}
          description={hasShortLinkDiscoveryCriteria(discoveryQuery)
            ? "Try a different search, status, or sort selection."
            : "Create a short link first, then manage it here."}
          action={hasShortLinkDiscoveryCriteria(discoveryQuery)
            ? <Button variant="secondary" onClick={() => handleDiscoveryChange(defaultShortLinkDiscoveryQuery)}>Clear filters</Button>
            : undefined}
        />
      ) : null}

      {shouldShowList ? (
        <div className="admin-table-wrap">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>
                  <input
                    type="checkbox"
                    className="bulk-checkbox"
                    checked={isPageSelected}
                    disabled={links.length === 0}
                    aria-label="Select all links on this page"
                    onChange={togglePageSelected}
                  />
                </TableHead>
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
                    <input
                      type="checkbox"
                      className="bulk-checkbox"
                      checked={selectedCodes.has(link.code)}
                      aria-label={`Select ${link.code}`}
                      onChange={() => toggleSelected(link.code)}
                    />
                  </TableCell>
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
                      {hasRowActions(link) ? (
                      <DropdownMenu
                        open={openMenuCode === link.code}
                        onOpenChange={(open) =>
                          setOpenMenuCode(open ? link.code : null)
                        }
                      >
                        <DropdownMenuTrigger
                          aria-expanded={openMenuCode === link.code}
                          aria-label={`Actions for ${link.code}`}
                        >
                          ...
                        </DropdownMenuTrigger>
                        {openMenuCode === link.code ? (
                          <DropdownMenuContent>
                            {adminPermissions.canReadAnalytics ? (
                            <DropdownMenuItem onClick={() => void openAnalyticsPanel(link)}>
                              Analytics
                            </DropdownMenuItem>
                            ) : null}
                            {adminPermissions.canUpdate ? (
                            <DropdownMenuItem onClick={() => startEdit(link)}>
                              Edit
                            </DropdownMenuItem>
                            ) : null}
                            {(link.isActive
                              ? adminPermissions.canDeactivate
                              : adminPermissions.canActivate) ? (
                            <DropdownMenuItem
                              disabled={busyCode === link.code}
                              onClick={() => requestStatusChange(link)}
                            >
                              {busyCode === link.code
                                ? "Updating"
                                : link.isActive
                                  ? "Deactivate"
                                  : "Activate"}
                            </DropdownMenuItem>
                            ) : null}
                            {adminPermissions.canDelete ? (
                            <DropdownMenuItem
                              className="danger-link"
                              disabled={busyCode === link.code}
                              onClick={() => requestDelete(link.code)}
                            >
                              Delete
                            </DropdownMenuItem>
                            ) : null}
                          </DropdownMenuContent>
                        ) : null}
                      </DropdownMenu>
                      ) : null}
                    </div>
                  </TableCell>
                </TableRow>
                </Fragment>
              ))}
            </TableBody>
          </Table>
        </div>
      ) : null}
      {shouldShowList ? (
      <div className="admin-pagination">
        <div className="pagination-summary">
          <span>{totalCount} items</span>
          <span>Page {pageNumber} of {totalPages}</span>
        </div>
        <div className="pagination-controls">
          <label className="pagination-page-size" ref={pageSizeMenuRef}>
            <button
              type="button"
              className="pagination-page-size-trigger"
              aria-expanded={isPageSizeMenuOpen}
              aria-haspopup="listbox"
              onClick={() => setIsPageSizeMenuOpen((current) => !current)}
            >
              {pageSize}
            </button>
            <span>/ page</span>
            {isPageSizeMenuOpen ? (
              <div className="pagination-page-size-menu" role="listbox">
                {[10, 25, 50, 100].map((size) => (
                  <button
                    key={size}
                    type="button"
                    className={size === pageSize ? "pagination-page-size-option pagination-page-size-option-active" : "pagination-page-size-option"}
                    role="option"
                    aria-selected={size === pageSize}
                    onClick={() => {
                      setPageSize(size);
                      setIsPageSizeMenuOpen(false);
                    }}
                  >
                    {size}
                  </button>
                ))}
              </div>
            ) : null}
          </label>
        <div className="pagination-pages" aria-label="Pagination">
          {pageNumber > 1 ? (
          <button type="button" className="pagination-arrow pagination-arrow-prev" aria-label="Previous page" onClick={() => goToPage(pageNumber - 1)}>
            ‹
          </button>
          ) : null}
          {getVisiblePages(pageNumber, totalPages).map((item, index) =>
            item === "gap" ? (
              <span key={`gap-${index}`} className="pagination-gap">...</span>
            ) : (
              <button
                key={item}
                type="button"
                className={item === pageNumber ? "pagination-page pagination-page-active" : "pagination-page"}
                onClick={() => goToPage(item)}
              >
                {item}
              </button>
            )
          )}
          {pageNumber < totalPages ? (
          <button type="button" className="pagination-arrow pagination-arrow-next" aria-label="Next page" onClick={() => goToPage(pageNumber + 1)}>
            ›
          </button>
          ) : null}
        </div>
        </div>
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
      {isEditorOpen ? (
        <div className="dialog-backdrop" role="presentation">
          <div
            className="edit-dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="edit-dialog-title"
          >
            <div>
              <p className="eyebrow">{isCreating ? "Create" : "Edit"}</p>
              <h2 id="edit-dialog-title">
                {isCreating ? "Create short link" : `Update ${editingLink?.code}`}
              </h2>
            </div>
            <Label className="field">
              <span className="field-label">
                Destination URL <span className="required-marker">*</span>
              </span>
              <Input
                type="url"
                required
                aria-invalid={fieldErrors.originalUrl ? "true" : undefined}
                aria-describedby={fieldErrors.originalUrl ? "editor-original-url-error" : undefined}
                value={editForm.originalUrl}
                onChange={(event) => {
                  const { value } = event.target;
                  setEditForm((current) => ({
                    ...current,
                    originalUrl: value
                  }));
                  setFieldErrors((current) => ({
                    ...current,
                    originalUrl: undefined
                  }));
                }}
              />
              {fieldErrors.originalUrl ? (
                <span id="editor-original-url-error" className="field-error">
                  {fieldErrors.originalUrl}
                </span>
              ) : null}
            </Label>
            <Label className="field">
              <span className="field-label">
                Expiry <span className="required-marker">*</span>
              </span>
              <Input
                type="datetime-local"
                required
                aria-invalid={fieldErrors.expiredAtLocal ? "true" : undefined}
                aria-describedby={fieldErrors.expiredAtLocal ? "editor-expiry-error" : undefined}
                value={editForm.expiredAtLocal}
                onChange={(event) => {
                  const { value } = event.target;
                  setEditForm((current) => ({
                    ...current,
                    expiredAtLocal: value
                  }));
                  setFieldErrors((current) => ({
                    ...current,
                    expiredAtLocal: undefined
                  }));
                }}
              />
              {fieldErrors.expiredAtLocal ? (
                <span id="editor-expiry-error" className="field-error">
                  {fieldErrors.expiredAtLocal}
                </span>
              ) : null}
              <ExpiryQuickPicks
                onChange={(expiredAtLocal) => {
                  setEditForm((current) => ({
                    ...current,
                    expiredAtLocal
                  }));
                  setFieldErrors((current) => ({
                    ...current,
                    expiredAtLocal: undefined
                  }));
                }}
              />
            </Label>
            {editorRequestError ? (
              <div className="recovery-banner recovery-banner-error" role="alert">
                <span>{editorRequestError} Your changes are still here; choose Save to try again.</span>
              </div>
            ) : null}
            <div className="dialog-actions">
              <Button
                variant="secondary"
                onClick={closeEditor}
              >
                Cancel
              </Button>
              <Button
                disabled={busyCode === (isCreating ? "__create__" : editingLink?.code)}
                onClick={() => {
                  if (isCreating) {
                    void handleCreate();
                  } else if (editingLink) {
                    void handleSaveEdit(editingLink.code);
                  }
                }}
              >
                {busyCode === (isCreating ? "__create__" : editingLink?.code)
                  ? "Saving"
                  : isCreating
                    ? "Create"
                    : "Save"}
              </Button>
            </div>
          </div>
        </div>
      ) : null}
      {analyticsCode ? (
        <div className="dialog-backdrop" role="presentation">
          <div
            className="analytics-dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="analytics-dialog-title"
          >
            <div className="analytics-dialog-header">
              <div>
                <p className="eyebrow">Analytics</p>
                <h2 id="analytics-dialog-title">{analyticsCode}</h2>
              </div>
              <Button variant="secondary" onClick={closeAnalyticsPanel}>
                Close
              </Button>
            </div>

            {isAnalyticsLoading ? (
              <div className="analytics-loading">
                <span className="skeleton skeleton-button" />
                <span className="skeleton skeleton-url" />
                <span className="skeleton skeleton-url" />
              </div>
            ) : null}

            {!isAnalyticsLoading && analyticsError ? (
              <EmptyState
                title="Analytics unavailable"
                description={analyticsError}
                action={isAnalyticsRetryable
                  ? <Button variant="secondary" onClick={() => void loadAnalytics(analyticsCode)}>Retry</Button>
                  : undefined}
              />
            ) : null}

            {!isAnalyticsLoading && !analyticsError && analyticsData ? (
              <div className="analytics-panel">
                <div className="analytics-metrics">
                  <div>
                    <span>Clicks</span>
                    <strong>{analyticsData.clickCount}</strong>
                  </div>
                  <div>
                    <span>Last clicked</span>
                    <strong>{analyticsData.lastClickedAtUtc ? formatDateTime(analyticsData.lastClickedAtUtc) : "No clicks yet"}</strong>
                  </div>
                </div>

                {analyticsData.recentClicks.length === 0 ? (
                  <EmptyState
                    title="No clicks yet"
                    description="Redirect analytics will appear here after visitors use this short link."
                  />
                ) : (
                  <div className="analytics-activity-list">
                    {analyticsData.recentClicks.map((click, index) => (
                      <div className="analytics-activity-item" key={`${click.clickedAtUtc}-${index}`}>
                        <div>
                          <span className="activity-time">{formatDateTime(click.clickedAtUtc)}</span>
                          <strong>{click.userAgent || "Unknown user agent"}</strong>
                        </div>
                        <dl>
                          <div>
                            <dt>Referrer</dt>
                            <dd>{click.referrer || "Direct or unavailable"}</dd>
                          </div>
                          <div>
                            <dt>Remote IP</dt>
                            <dd>{click.remoteIpAddress || "Unavailable"}</dd>
                          </div>
                        </dl>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ) : null}
          </div>
        </div>
      ) : null}
      </CardContent>
      <ConfirmDialog
        open={confirmAction !== null}
        title={confirmAction?.title ?? ""}
        description={confirmAction?.description ?? ""}
        confirmLabel={confirmAction?.confirmLabel ?? "Confirm"}
        variant={confirmAction?.variant}
        onConfirm={() => {
          const action = confirmAction;
          setConfirmAction(null);
          action?.onConfirm();
        }}
        onCancel={() => setConfirmAction(null)}
      />
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

function getVisiblePages(currentPage: number, totalPages: number): Array<number | "gap"> {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  if (currentPage <= 4) {
    return [1, 2, 3, 4, 5, "gap", totalPages];
  }

  if (currentPage >= totalPages - 3) {
    return [1, "gap", totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
  }

  return [1, "gap", currentPage - 1, currentPage, currentPage + 1, "gap", totalPages];
}
