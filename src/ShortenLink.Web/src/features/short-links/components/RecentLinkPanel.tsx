import { useState } from "react";
import type { CreatedShortLink } from "../types";
import { formatDateTime } from "../types";

type RecentLinkPanelProps = {
  recentLink: CreatedShortLink | null;
  onOpenDetails: (code: string) => void;
};

export function RecentLinkPanel({ recentLink, onOpenDetails }: RecentLinkPanelProps) {
  const [copyState, setCopyState] = useState<"idle" | "copied" | "error">("idle");

  const handleCopy = async () => {
    if (!recentLink) {
      return;
    }

    try {
      await navigator.clipboard.writeText(recentLink.shortUrl);
      setCopyState("copied");
      window.setTimeout(() => setCopyState("idle"), 1600);
    } catch {
      setCopyState("error");
    }
  };

  if (!recentLink) {
    return (
      <section className="panel panel-preview panel-empty">
        <div className="panel-heading">
          <p className="eyebrow">Result</p>
          <h2>Your latest link will land here.</h2>
        </div>
        <p className="muted-copy">
          Create a link to get a copy-ready short URL, then jump straight into the
          detail view to review or deactivate it.
        </p>
      </section>
    );
  }

  return (
    <section className="panel panel-preview">
      <div className="panel-heading">
        <p className="eyebrow">Result</p>
        <h2>{recentLink.code}</h2>
      </div>

      <dl className="detail-list">
        <div>
          <dt>Short URL</dt>
          <dd>
            <a href={recentLink.shortUrl} target="_blank" rel="noreferrer">
              {recentLink.shortUrl}
            </a>
          </dd>
        </div>
        <div>
          <dt>Destination</dt>
          <dd>{recentLink.originalUrl}</dd>
        </div>
        <div>
          <dt>Created</dt>
          <dd>{formatDateTime(recentLink.createdAtUtc)}</dd>
        </div>
      </dl>

      {copyState === "copied" ? <p className="feedback">Short URL copied.</p> : null}
      {copyState === "error" ? (
        <p className="feedback feedback-error">
          Clipboard access was blocked, so the URL could not be copied.
        </p>
      ) : null}

      <div className="form-actions">
        <button className="action-button" type="button" onClick={handleCopy}>
          Copy short URL
        </button>
        <button
          className="action-button action-button-secondary"
          type="button"
          onClick={() => onOpenDetails(recentLink.code)}
        >
          Open details
        </button>
      </div>
    </section>
  );
}
