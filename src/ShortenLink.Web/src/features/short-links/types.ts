export type AppRoute =
  | { kind: "home" }
  | { kind: "admin" }
  | { kind: "detail"; code: string }
  | { kind: "status"; statusCode: HttpStatusCode };

export type HttpStatusCode = 401 | 403 | 404;

export type ShortLinkFormInput = {
  originalUrl: string;
  expiredAtLocal: string;
};

export type CreateShortLinkRequest = {
  originalUrl: string;
  expiredAtUtc: string;
};

export type UpdateShortLinkRequest = {
  originalUrl: string;
  expiredAtUtc: string;
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

export type ShortLinkAdminItem = {
  code: string;
  shortUrl: string;
  originalUrl: string;
  createdAtUtc: string;
  expiredAtUtc: string | null;
  isActive: boolean;
};

export type ShortLinkAdminPageResult = {
  items: ShortLinkAdminItem[];
  nextCursor: string | null;
  totalCount: number | null;
  page: number | null;
  pageSize: number | null;
  totalPages: number | null;
};

export type DeactivatedShortLink = {
  code: string;
  isActive: boolean;
};

export type DeletedShortLink = {
  code: string;
};

export type ApiErrorPayload = {
  errorCode: string;
  message: string;
};

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
    case "invalid_code":
      return "Enter a valid short-link code.";
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
