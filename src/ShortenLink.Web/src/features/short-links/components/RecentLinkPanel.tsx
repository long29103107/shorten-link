import { useState } from "react";
import type { CreatedShortLink } from "../types";
import { formatDateTime } from "../types";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "../../../shared/components/ui/card";

type RecentLinkPanelProps = {
  recentLink: CreatedShortLink | null;
};

export function RecentLinkPanel({ recentLink }: RecentLinkPanelProps) {
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
      <Card className="panel-preview panel-empty">
        <CardHeader>
          <p className="eyebrow">Result</p>
          <CardTitle>Your latest link will land here.</CardTitle>
        </CardHeader>
        <CardContent>
        <p className="muted-copy">
          Create a link to get a copy-ready short URL with a random code generated
          by the app.
        </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="panel-preview">
      <CardHeader>
        <p className="eyebrow">Result</p>
        <CardTitle>{recentLink.code}</CardTitle>
      </CardHeader>

      <CardContent>
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
      </CardContent>

      <CardFooter>
        <Button onClick={handleCopy}>
          Copy short URL
        </Button>
      </CardFooter>
    </Card>
  );
}
