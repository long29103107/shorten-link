export type RecoveryNotice = {
  message: string;
  retryable: boolean;
};

type RetryableFailureLike = {
  retryable: boolean;
};

export function createRecoveryNotice(error: unknown, message: string): RecoveryNotice {
  return {
    message,
    retryable: isRetryableFailure(error)
  };
}

export function shouldPreserveMutationContext(error: unknown) {
  return isRetryableFailure(error);
}

export type OneTimeSecretEvent =
  | { type: "created"; secret: string }
  | { type: "request-started" | "refreshed" | "dismissed" };

export function resolveOneTimeSecret(event: OneTimeSecretEvent) {
  return event.type === "created" ? event.secret : null;
}

function isRetryableFailure(error: unknown): error is RetryableFailureLike {
  return typeof error === "object"
    && error !== null
    && "retryable" in error
    && error.retryable === true;
}
