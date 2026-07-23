import { useEffect, useId, type FormEvent, type ReactNode } from "react";
import { Button } from "./ui/button";

type FormDialogProps = {
  open: boolean;
  title: string;
  children: ReactNode;
  submitLabel: string;
  description?: string;
  cancelLabel?: string;
  isSubmitting?: boolean;
  onSubmit: () => void | Promise<void>;
  onCancel: () => void;
};

export function FormDialog({
  open,
  title,
  children,
  submitLabel,
  description,
  cancelLabel = "Cancel",
  isSubmitting = false,
  onSubmit,
  onCancel
}: FormDialogProps) {
  const titleId = useId();
  const descriptionId = useId();

  useEffect(() => {
    if (!open) return;
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isSubmitting) onCancel();
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isSubmitting, onCancel, open]);

  if (!open) return null;

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void onSubmit();
  };

  return (
    <div className="dialog-backdrop" role="presentation" onMouseDown={(event) => {
      if (event.target === event.currentTarget && !isSubmitting) onCancel();
    }}>
      <form className="form-dialog" role="dialog" aria-modal="true" aria-labelledby={titleId} aria-describedby={description ? descriptionId : undefined} onSubmit={handleSubmit}>
        <header className="form-dialog-header">
          <div>
            <h2 id={titleId}>{title}</h2>
            {description ? <p id={descriptionId}>{description}</p> : null}
          </div>
          <button className="dialog-close" type="button" aria-label="Close dialog" disabled={isSubmitting} onClick={onCancel}>×</button>
        </header>
        <div className="form-dialog-body">{children}</div>
        <footer className="dialog-actions">
          <Button type="button" variant="secondary" disabled={isSubmitting} onClick={onCancel}>{cancelLabel}</Button>
          <Button type="submit" disabled={isSubmitting}>{isSubmitting ? "Saving..." : submitLabel}</Button>
        </footer>
      </form>
    </div>
  );
}
