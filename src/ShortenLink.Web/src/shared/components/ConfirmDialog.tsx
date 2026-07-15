import { Button } from "./ui/button";

type ConfirmDialogVariant = "default" | "destructive";

type ConfirmDialogProps = {
  open: boolean;
  title: string;
  description: string;
  confirmLabel: string;
  cancelLabel?: string;
  variant?: ConfirmDialogVariant;
  onConfirm: () => void;
  onCancel: () => void;
};

export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel,
  cancelLabel = "Cancel",
  variant = "default",
  onConfirm,
  onCancel
}: ConfirmDialogProps) {
  if (!open) {
    return null;
  }

  return (
    <div className="dialog-backdrop" role="presentation">
      <div
        className="confirm-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-title"
        aria-describedby="confirm-dialog-description"
      >
        <div>
          <h2 id="confirm-dialog-title">{title}</h2>
          <p id="confirm-dialog-description">{description}</p>
        </div>
        <div className="dialog-actions">
          <Button variant="secondary" onClick={onCancel}>
            {cancelLabel}
          </Button>
          <Button
            variant={variant === "destructive" ? "destructive" : "default"}
            onClick={onConfirm}
          >
            {confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}
