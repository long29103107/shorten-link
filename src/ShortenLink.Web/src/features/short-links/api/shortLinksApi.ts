import { fetchJson } from "./http";
import type {
  CreateShortLinkRequest,
  CreatedShortLink,
  DeactivatedShortLink,
  ShortLinkDetails
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

export async function deactivateShortLink(code: string): Promise<DeactivatedShortLink> {
  return fetchJson<DeactivatedShortLink>(`/api/short-links/${encodeURIComponent(code)}`, {
    method: "DELETE"
  });
}
