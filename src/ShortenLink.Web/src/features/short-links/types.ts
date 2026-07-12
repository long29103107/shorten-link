export type AppRoute =
  | { kind: "home" }
  | { kind: "detail"; code: string }
  | { kind: "not-found" };

export type ShortLinkFormInput = {
  originalUrl: string;
  customAlias: string;
  expiredAtLocal: string;
};

export type CreateShortLinkRequest = {
  originalUrl: string;
  customAlias?: string;
  expiredAtUtc?: string;
};

export type CreatedShortLink = {
  code: string;
  shortUrl: string;
  originalUrl: string;
  createdAtUtc: string;
};

export type ShortLinkDetails = {
  code: string;
  originalUrl: string;
  createdAtUtc: string;
  expiredAtUtc: string | null;
  isActive: boolean;
};

export type DeactivatedShortLink = {
  code: string;
  isActive: boolean;
};

export type ApiErrorPayload = {
  errorCode: string;
  message: string;
};

export const shortLinkAliasPattern = /^[A-Za-z0-9_-]+$/;

export function formatDateTime(value: string | null): string {
  if (!value) {
    return "No expiry";
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short"
      }).format(date);
}

export function toFriendlyErrorMessage(errorCode: string, fallbackMessage: string): string {
  switch (errorCode) {
    case "duplicate_alias":
      return "That alias is already taken. Try a different code.";
    case "invalid_alias":
      return "Alias can use letters, numbers, underscores, and hyphens only.";
    case "invalid_code":
      return "Enter a valid short-link code before opening details.";
    case "invalid_expiration":
      return "Expiry needs to be in the future.";
    case "invalid_url":
      return "Paste a full http:// or https:// URL.";
    case "inactive":
      return "This link has already been deactivated.";
    case "expired":
      return "This link has expired.";
    case "not_found":
      return "We could not find that short link.";
    default:
      return fallbackMessage;
  }
}
