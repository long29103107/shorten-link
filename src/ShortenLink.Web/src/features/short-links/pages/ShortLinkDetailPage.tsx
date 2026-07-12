import { useEffect, useState } from "react";
import { ApiError } from "../api/http";
import { deactivateShortLink, getShortLinkDetails } from "../api/shortLinksApi";
import type { ShortLinkDetails } from "../types";
import { formatDateTime, toFriendlyErrorMessage } from "../types";

type ShortLinkDetailPageProps = {
  code: string;
  onBackHome: () => void;
};

export function ShortLinkDetailPage({ code, onBackHome }: ShortLinkDetailPageProps) {
  const [details, setDetails] = useState<ShortLinkDetails | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDeactivating, setIsDeactivating] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadDetails() {
      setIsLoading(true);
      setErrorMessage(null);

      try {
        const response = await getShortLinkDetails(code);
        if (!cancelled) {
          setDetails(response);
        }
      } catch (error) {
        if (cancelled) {
          return;
        }

        if (error instanceof ApiError) {
          setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
        } else {
          setErrorMessage("We could not load this short link right now.");
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadDetails();

    return () => {
      cancelled = true;
    };
  }, [code]);

  const handleDeactivate = async () => {
    setIsDeactivating(true);
    setErrorMessage(null);

    try {
      const response = await deactivateShortLink(code);
      setDetails((current) =>
        current
          ? {
              ...current,
              code: response.code,
              isActive: response.isActive
            }
          : current
      );
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("The link could not be deactivated.");
      }
    } finally {
      setIsDeactivating(false);
    }
  };

  if (isLoading) {
    return (
      <section className="panel panel-detail">
        <div className="panel-heading">
          <p className="eyebrow">Details</p>
          <h2>Loading {code}...</h2>
        </div>
      </section>
    );
  }

  if (!details) {
    return (
      <section className="panel panel-detail">
        <div className="panel-heading">
          <p className="eyebrow">Details</p>
          <h2>{code}</h2>
        </div>
        <p className="feedback feedback-error">{errorMessage ?? "This short link is missing."}</p>
        <div className="form-actions">
          <button className="action-button" type="button" onClick={onBackHome}>
            Back home
          </button>
        </div>
      </section>
    );
  }

  return (
    <section className="panel panel-detail">
      <div className="panel-heading panel-heading-wide">
        <div>
          <p className="eyebrow">Details</p>
          <h2>{details.code}</h2>
        </div>
        <span className={details.isActive ? "status-pill status-live" : "status-pill status-off"}>
          {details.isActive ? "Active" : "Deactivated"}
        </span>
      </div>

      <dl className="detail-list">
        <div>
          <dt>Destination</dt>
          <dd>
            <a href={details.originalUrl} target="_blank" rel="noreferrer">
              {details.originalUrl}
            </a>
          </dd>
        </div>
        <div>
          <dt>Created</dt>
          <dd>{formatDateTime(details.createdAtUtc)}</dd>
        </div>
        <div>
          <dt>Expiry</dt>
          <dd>{formatDateTime(details.expiredAtUtc)}</dd>
        </div>
      </dl>

      {errorMessage ? <p className="feedback feedback-error">{errorMessage}</p> : null}

      <div className="form-actions">
        <button className="action-button action-button-secondary" type="button" onClick={onBackHome}>
          Back home
        </button>
        <button
          className="action-button action-button-danger"
          type="button"
          onClick={handleDeactivate}
          disabled={!details.isActive || isDeactivating}
        >
          {isDeactivating ? "Deactivating..." : details.isActive ? "Deactivate link" : "Already inactive"}
        </button>
      </div>
    </section>
  );
}
