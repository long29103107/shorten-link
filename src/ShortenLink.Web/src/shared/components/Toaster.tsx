import { CSSProperties, useEffect, useState } from "react";
import { toastEventName, type ToastMessage } from "../toast";

const toastDurationMs = 4500;
const maxToasts = 3;

const toastColors = {
  success: {
    background: "#22C55E",
    border: "#16A34A",
    color: "#FFFFFF"
  },
  info: {
    background: "#3B82F6",
    border: "#2563EB",
    color: "#FFFFFF"
  },
  warning: {
    background: "#F59E0B",
    border: "#D97706",
    color: "#FFFFFF"
  },
  error: {
    background: "#EF4444",
    border: "#DC2626",
    color: "#FFFFFF"
  }
};

export function Toaster() {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  useEffect(() => {
    const handleToast = (event: Event) => {
      const toast = (event as CustomEvent<ToastMessage>).detail;
      setToasts((current) => [...current, toast].slice(-maxToasts));

      window.setTimeout(() => {
        setToasts((current) => current.filter((item) => item.id !== toast.id));
      }, toastDurationMs);
    };

    window.addEventListener(toastEventName, handleToast);
    return () => window.removeEventListener(toastEventName, handleToast);
  }, []);

  if (toasts.length === 0) {
    return null;
  }

  return (
    <div className="toast-region" aria-live="polite" aria-label="Notifications">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={`toast toast-${toast.variant}`}
          style={{
            "--toast-background": toastColors[toast.variant].background,
            "--toast-border": toastColors[toast.variant].border,
            "--toast-color": toastColors[toast.variant].color
          } as CSSProperties}
        >
          <div>
            <p className="toast-title">{toast.title}</p>
            {toast.message ? <p className="toast-message">{toast.message}</p> : null}
          </div>
          <button
            type="button"
            className="toast-close"
            aria-label="Dismiss notification"
            onClick={() =>
              setToasts((current) => current.filter((item) => item.id !== toast.id))
            }
          >
            x
          </button>
        </div>
      ))}
    </div>
  );
}
