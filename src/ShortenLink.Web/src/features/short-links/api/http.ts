import type { ApiErrorPayload } from "../types";
import { showToast } from "../../../shared/toast";

export class ApiError extends Error {
  readonly status: number;
  readonly errorCode: string;

  constructor(status: number, payload: ApiErrorPayload) {
    super(payload.message);
    this.name = "ApiError";
    this.status = status;
    this.errorCode = payload.errorCode;
  }
}

export async function fetchJson<T>(input: RequestInfo | URL, init?: RequestInit): Promise<T> {
  const response = await fetch(input, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {})
    }
  });

  if (response.ok) {
    if (response.status === 204) {
      return undefined as T;
    }

    return (await response.json()) as T;
  }

  const payload = (await safeReadError(response)) ?? {
    errorCode: "unexpected_error",
    message: `Request failed with status ${response.status}.`
  };

  showToast({
    title: "Request failed",
    message: payload.message,
    variant: "error"
  });

  throw new ApiError(response.status, payload);
}

async function safeReadError(response: Response): Promise<ApiErrorPayload | null> {
  try {
    return (await response.json()) as ApiErrorPayload;
  } catch {
    return null;
  }
}
