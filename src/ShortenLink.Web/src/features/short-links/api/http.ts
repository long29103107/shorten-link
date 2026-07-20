import type { ApiErrorPayload } from "../types";
import { showToast } from "../../../shared/toast";
import {
  classifyFetchFailure,
  classifyHttpFailure,
  type ApiFailure
} from "../../../shared/api/apiFailure";
import { getAdminApiKeyHeader } from "./adminSecurity";

type FetchJsonOptions = RequestInit & {
  suppressAuthRedirect?: boolean;
};

export class ApiError extends Error {
  readonly status: number | null;
  readonly errorCode: string;
  readonly kind: ApiFailure["kind"];
  readonly retryable: boolean;
  readonly shouldNavigateToAuth: boolean;
  readonly failure: ApiFailure;

  constructor(failure: ApiFailure) {
    super(failure.message);
    this.name = "ApiError";
    this.status = failure.status;
    this.errorCode = failure.errorCode;
    this.kind = failure.kind;
    this.retryable = failure.retryable;
    this.shouldNavigateToAuth = failure.shouldNavigateToAuth;
    this.failure = failure;
  }
}

export async function fetchJson<T>(input: RequestInfo | URL, init?: FetchJsonOptions): Promise<T> {
  const { suppressAuthRedirect = false, ...requestInit } = init ?? {};
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

  const payload = (await safeReadError(response)) ?? {
    errorCode: "unexpected_error",
    message: `Request failed with status ${response.status}.`
  };
  const failure = classifyHttpFailure(response.status, payload);

  if (failure.shouldNavigateToAuth && !suppressAuthRedirect) {
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
  const path = status === 401 ? "/unauthorized" : "/forbidden";
  if (window.location.pathname !== path) {
    window.history.pushState({}, "", path);
  }

  window.dispatchEvent(new PopStateEvent("popstate"));
}
