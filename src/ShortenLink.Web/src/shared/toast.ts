export type ToastVariant = "error" | "info" | "warning" | "success";

export type ToastMessage = {
  id: string;
  title: string;
  message?: string;
  variant: ToastVariant;
};

export const toastEventName = "shorten-link:toast";

export const defaultErrorToastSuppressionWindowMs = 5000;

type ToastInput = Omit<ToastMessage, "id">;

export class ToastDeduplicator {
  private readonly emittedAtByKey = new Map<string, number>();

  constructor(
    private readonly suppressionWindowMs = defaultErrorToastSuppressionWindowMs,
    private readonly maxEntries = 100,
    private readonly now: () => number = () => Date.now()
  ) {}

  shouldEmit(toast: ToastInput) {
    if (toast.variant !== "error") {
      return true;
    }

    const now = this.now();
    this.removeExpired(now);
    const key = createToastDeduplicationKey(toast);
    const previousEmission = this.emittedAtByKey.get(key);
    if (previousEmission !== undefined && now - previousEmission < this.suppressionWindowMs) {
      return false;
    }

    this.emittedAtByKey.delete(key);
    this.emittedAtByKey.set(key, now);
    this.trimToLimit();
    return true;
  }

  get trackedErrorCount() {
    return this.emittedAtByKey.size;
  }

  private removeExpired(now: number) {
    for (const [key, emittedAt] of this.emittedAtByKey) {
      if (now - emittedAt >= this.suppressionWindowMs) {
        this.emittedAtByKey.delete(key);
      }
    }
  }

  private trimToLimit() {
    while (this.emittedAtByKey.size > this.maxEntries) {
      const oldestKey = this.emittedAtByKey.keys().next().value;
      if (oldestKey === undefined) {
        return;
      }
      this.emittedAtByKey.delete(oldestKey);
    }
  }
}

export function createToastDeduplicationKey(toast: ToastInput) {
  return `${toast.variant}\u0000${toast.title}\u0000${toast.message ?? ""}`;
}

const toastDeduplicator = new ToastDeduplicator();

export function showToast(toast: ToastInput) {
  if (!toastDeduplicator.shouldEmit(toast)) {
    return false;
  }

  window.dispatchEvent(
    new CustomEvent<ToastMessage>(toastEventName, {
      detail: {
        ...toast,
        id: crypto.randomUUID()
      }
    })
  );
  return true;
}
