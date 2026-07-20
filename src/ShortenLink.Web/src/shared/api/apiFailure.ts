export type ApiFailureKind =
  | "network"
  | "timeout"
  | "rate-limit"
  | "server"
  | "validation"
  | "authentication"
  | "authorization"
  | "not-found"
  | "unexpected";

export type ApiFailure = {
  kind: ApiFailureKind;
  status: number | null;
  errorCode: string;
  message: string;
  retryable: boolean;
  shouldNavigateToAuth: boolean;
};

export type ApiFailurePayload = {
  errorCode: string;
  message: string;
};

export function classifyHttpFailure(status: number, payload: ApiFailurePayload): ApiFailure {
  if (status === 401) {
    return createFailure("authentication", status, payload, false, true);
  }

  if (status === 403) {
    return createFailure("authorization", status, payload, false, true);
  }

  if (status === 404) {
    return createFailure("not-found", status, payload, false, false);
  }

  if (status === 408) {
    return createFailure("timeout", status, payload, true, false);
  }

  if (status === 429) {
    return createFailure("rate-limit", status, payload, true, false);
  }

  if (status >= 500) {
    return createFailure("server", status, payload, true, false);
  }

  if (status === 400 || status === 409 || status === 422) {
    return createFailure("validation", status, payload, false, false);
  }

  return createFailure("unexpected", status, payload, false, false);
}

export function classifyFetchFailure(error: unknown): ApiFailure {
  if (isAbortError(error)) {
    return {
      kind: "timeout",
      status: null,
      errorCode: "request_timeout",
      message: "The request timed out.",
      retryable: true,
      shouldNavigateToAuth: false
    };
  }

  return {
    kind: "network",
    status: null,
    errorCode: "network_error",
    message: "The server could not be reached.",
    retryable: true,
    shouldNavigateToAuth: false
  };
}

function createFailure(
  kind: ApiFailureKind,
  status: number,
  payload: ApiFailurePayload,
  retryable: boolean,
  shouldNavigateToAuth: boolean
): ApiFailure {
  return {
    kind,
    status,
    errorCode: payload.errorCode,
    message: payload.message,
    retryable,
    shouldNavigateToAuth
  };
}

function isAbortError(error: unknown) {
  return typeof error === "object"
    && error !== null
    && "name" in error
    && error.name === "AbortError";
}
