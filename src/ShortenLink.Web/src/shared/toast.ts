export type ToastVariant = "error" | "info" | "warning" | "success";

export type ToastMessage = {
  id: string;
  title: string;
  message?: string;
  variant: ToastVariant;
};

export const toastEventName = "shorten-link:toast";

export function showToast(toast: Omit<ToastMessage, "id">) {
  window.dispatchEvent(
    new CustomEvent<ToastMessage>(toastEventName, {
      detail: {
        ...toast,
        id: crypto.randomUUID()
      }
    })
  );
}
