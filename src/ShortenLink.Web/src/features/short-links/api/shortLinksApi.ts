import { fetchJson } from "./http";
import type {
  CreateShortLinkRequest,
  CreatedShortLink,
  DeactivatedShortLink,
  DeletedShortLink,
  SecurityAssignment,
  SecurityAssignmentDisabled,
  SecurityAssignmentsList,
  SecurityAssignmentUpsertRequest,
  ShortLinkAnalytics,
  ShortLinkAdminItem,
  ShortLinkAdminPageResult,
  ShortLinkDetails,
  UpdateShortLinkRequest
} from "../types";

export async function createShortLink(
  request: CreateShortLinkRequest
): Promise<CreatedShortLink> {
  return fetchJson<CreatedShortLink>("/api/short-links", {
    method: "POST",
    body: JSON.stringify(request)
  });
}

export async function getShortLinkDetails(code: string): Promise<ShortLinkDetails> {
  return fetchJson<ShortLinkDetails>(`/api/short-links/${encodeURIComponent(code)}`);
}

export async function getShortLinkAnalytics(code: string): Promise<ShortLinkAnalytics> {
  return fetchJson<ShortLinkAnalytics>(`/api/short-links/${encodeURIComponent(code)}/analytics`);
}

export async function listShortLinks(
  limit = 25,
  page = 1
): Promise<ShortLinkAdminPageResult> {
  const params = new URLSearchParams({
    limit: String(limit),
    page: String(page)
  });

  return fetchJson<ShortLinkAdminPageResult>(`/api/short-links?${params.toString()}`);
}

export async function deactivateShortLink(code: string): Promise<DeactivatedShortLink> {
  return fetchJson<DeactivatedShortLink>(`/api/short-links/${encodeURIComponent(code)}/deactivate`, {
    method: "POST"
  });
}

export async function activateShortLink(code: string): Promise<DeactivatedShortLink> {
  return fetchJson<DeactivatedShortLink>(`/api/short-links/${encodeURIComponent(code)}/activate`, {
    method: "POST"
  });
}

export async function updateShortLink(
  code: string,
  request: UpdateShortLinkRequest
): Promise<ShortLinkAdminItem> {
  return fetchJson<ShortLinkAdminItem>(`/api/short-links/${encodeURIComponent(code)}`, {
    method: "PUT",
    body: JSON.stringify(request)
  });
}

export async function deleteShortLink(code: string): Promise<DeletedShortLink> {
  return fetchJson<DeletedShortLink>(`/api/short-links/${encodeURIComponent(code)}`, {
    method: "DELETE"
  });
}

export async function listSecurityAssignments(): Promise<SecurityAssignmentsList> {
  return fetchJson<SecurityAssignmentsList>("/api/security/assignments");
}

export async function upsertSecurityAssignment(
  request: SecurityAssignmentUpsertRequest
): Promise<SecurityAssignment> {
  return fetchJson<SecurityAssignment>("/api/security/assignments", {
    method: "PUT",
    body: JSON.stringify(request)
  });
}

export async function disableSecurityAssignment(
  credentialKeyHash: string
): Promise<SecurityAssignmentDisabled> {
  return fetchJson<SecurityAssignmentDisabled>(
    `/api/security/assignments/${encodeURIComponent(credentialKeyHash)}/disable`,
    { method: "POST" }
  );
}
