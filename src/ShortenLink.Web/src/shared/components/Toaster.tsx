import { useEffect, useState } from "react";
import { toastEventName, type ToastMessage } from "../toast";

const toastDurationMs = 4500;
const maxToasts = 3;

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
        <div key={toast.id} className={`toast toast-${toast.variant}`}>
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
