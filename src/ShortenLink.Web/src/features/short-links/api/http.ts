import type { ApiErrorPayload, SecurityLoginResponse } from "../types";
import { showToast } from "../../../shared/toast";
import {
  classifyFetchFailure,
  classifyHttpFailure,
  type ApiFailure
} from "../../../shared/api/apiFailure";
import { clearStoredSession, getAdminApiKeyHeader, getStoredRefreshToken, storeSession } from "./adminSecurity";

type FetchJsonOptions = RequestInit & {
  suppressAuthRedirect?: boolean;
  skipRefresh?: boolean;
};

let refreshPromise: Promise<boolean> | null = null;

export class ApiError extends Error {
  readonly status: number | null;
  readonly errorCode: string;
  readonly kind: ApiFailure["kind"];
  readonly retryable: boolean;
  readonly shouldNavigateToAuth: boolean;
  readonly failure: ApiFailure;
  readonly fieldErrors: Record<string, string>;

  constructor(failure: ApiFailure) {
    super(failure.message);
    this.name = "ApiError";
    this.status = failure.status;
    this.errorCode = failure.errorCode;
    this.kind = failure.kind;
    this.retryable = failure.retryable;
    this.shouldNavigateToAuth = failure.shouldNavigateToAuth;
    this.failure = failure;
    this.fieldErrors = failure.fieldErrors;
  }
}

export async function fetchJson<T>(input: RequestInfo | URL, init?: FetchJsonOptions): Promise<T> {
  const { suppressAuthRedirect = false, skipRefresh = false, ...requestInit } = init ?? {};
  let response: Response;
  try {
    response = await fetch(input, {
      ...requestInit,
      headers: {
        "Content-Type": "application/json",
        ...getAdminApiKeyHeader(),
        ...(requestInit.headers ?? {})
      }
    });
  } catch (error) {
    const failure = classifyFetchFailure(error);
    showFailureToast(failure);
    throw new ApiError(failure);
  }

  if (response.ok) {
    if (response.status === 204) {
      return undefined as T;
    }

    return (await response.json()) as T;
  }

  if (response.status === 401 && !suppressAuthRedirect && !skipRefresh) {
    const refreshed = await refreshSession();
    if (refreshed) {
      return fetchJson<T>(input, { ...init, skipRefresh: true });
    }
  }

  const payload = (await safeReadError(response)) ?? {
    errorCode: "unexpected_error",
    message: `Request failed with status ${response.status}.`
  };
  const failure = classifyHttpFailure(response.status, payload);

  if (failure.shouldNavigateToAuth && !suppressAuthRedirect) {
    if (response.status === 401) {
      clearStoredSession();
    }
    navigateToStatusPage(response.status);
    throw new ApiError(failure);
  }

  showFailureToast(failure);

  throw new ApiError(failure);
}

function showFailureToast(failure: ApiFailure) {
  showToast({
    title: failure.retryable ? "Server temporarily unavailable" : "Request failed",
    message: failure.message,
    variant: "error"
  });
}

async function safeReadError(response: Response): Promise<ApiErrorPayload | null> {
  try {
    return (await response.json()) as ApiErrorPayload;
  } catch {
    return null;
  }
}

function navigateToStatusPage(status: number) {
  const path = status === 401 ? "/login" : "/forbidden";
  if (window.location.pathname !== path) {
    window.history.pushState({}, "", path);
  }

  window.dispatchEvent(new PopStateEvent("popstate"));
}

async function refreshSession(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = performRefresh();
  try {
    return await refreshPromise;
  } finally {
    refreshPromise = null;
  }
}

async function performRefresh(): Promise<boolean> {
  const refreshToken = getStoredRefreshToken();
  if (!refreshToken) {
    return false;
  }

  try {
    const response = await fetch("/api/security/refresh", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken })
    });
    if (!response.ok) {
      clearStoredSession();
      return false;
    }

    const session = await response.json() as SecurityLoginResponse;
    storeSession(session.accessToken, session.refreshToken, session.user);
    return true;
  } catch {
    return false;
  }
}
