import { fetchJson } from "./http";
import type {
  CreateShortLinkRequest,
  CreatedShortLink,
  DeactivatedShortLink,
  DeletedShortLink,
  ShortLinkAdminItem,
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

export async function listShortLinks(limit = 100): Promise<ShortLinkAdminItem[]> {
  return fetchJson<ShortLinkAdminItem[]>(`/api/short-links?limit=${limit}`);
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
