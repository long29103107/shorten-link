import { FormEvent, useMemo, useState } from "react";
import { ApiError } from "../api/http";
import { createShortLink } from "../api/shortLinksApi";
import type { CreatedShortLink, ShortLinkFormInput } from "../types";
import { shortLinkAliasPattern, toFriendlyErrorMessage } from "../types";

type CreateShortLinkFormProps = {
  onCreated: (createdLink: CreatedShortLink) => void;
  onOpenDetails: (code: string) => void;
};

const initialForm: ShortLinkFormInput = {
  originalUrl: "",
  customAlias: "",
  expiredAtLocal: ""
};

export function CreateShortLinkForm({
  onCreated,
  onOpenDetails
}: CreateShortLinkFormProps) {
  const [form, setForm] = useState(initialForm);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const localValidationMessage = useMemo(() => {
    if (!form.originalUrl.trim()) {
      return "Paste a full destination URL to shorten.";
    }

    try {
      const url = new URL(form.originalUrl);
      if (url.protocol !== "http:" && url.protocol !== "https:") {
        return "Use an http:// or https:// link.";
      }
    } catch {
      return "The destination URL does not look valid yet.";
    }

    if (form.customAlias.trim() && !shortLinkAliasPattern.test(form.customAlias.trim())) {
      return "Alias can use letters, numbers, underscores, and hyphens only.";
    }

    if (form.expiredAtLocal) {
      const expiry = new Date(form.expiredAtLocal);
      if (Number.isNaN(expiry.getTime()) || expiry.getTime() <= Date.now()) {
        return "Choose an expiry time in the future.";
      }
    }

    return null;
  }, [form]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (localValidationMessage) {
      setErrorMessage(localValidationMessage);
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    try {
      const createdLink = await createShortLink({
        originalUrl: form.originalUrl.trim(),
        customAlias: form.customAlias.trim() || undefined,
        expiredAtUtc: form.expiredAtLocal
          ? new Date(form.expiredAtLocal).toISOString()
          : undefined
      });

      onCreated(createdLink);
    } catch (error) {
      if (error instanceof ApiError) {
        setErrorMessage(toFriendlyErrorMessage(error.errorCode, error.message));
      } else {
        setErrorMessage("We hit an unexpected problem while creating the link.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="panel panel-form" onSubmit={handleSubmit}>
      <div className="panel-heading">
        <p className="eyebrow">Create</p>
        <h2>Ship a short link in one pass.</h2>
      </div>

      <label className="field">
        <span className="field-label">Destination URL</span>
        <input
          className="text-input"
          type="url"
          placeholder="https://example.com/really/long/path"
          value={form.originalUrl}
          onChange={(event) =>
            setForm((current) => ({ ...current, originalUrl: event.target.value }))
          }
        />
      </label>

      <div className="field-grid">
        <label className="field">
          <span className="field-label">Custom alias</span>
          <input
            className="text-input"
            placeholder="launch-kit"
            value={form.customAlias}
            onChange={(event) =>
              setForm((current) => ({ ...current, customAlias: event.target.value }))
            }
          />
        </label>

        <label className="field">
          <span className="field-label">Expiry</span>
          <input
            className="text-input"
            type="datetime-local"
            value={form.expiredAtLocal}
            onChange={(event) =>
              setForm((current) => ({ ...current, expiredAtLocal: event.target.value }))
            }
          />
        </label>
      </div>

      {errorMessage ? <p className="feedback feedback-error">{errorMessage}</p> : null}

      <div className="form-actions">
        <button className="action-button" type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Creating..." : "Create short link"}
        </button>
        <button
          className="action-button action-button-secondary"
          type="button"
          onClick={() => setForm(initialForm)}
          disabled={isSubmitting}
        >
          Clear
        </button>
      </div>

      <button
        className="text-link"
        type="button"
        onClick={() => onOpenDetails(form.customAlias.trim() || "")}
        disabled={!form.customAlias.trim()}
      >
        Jump to alias details
      </button>
    </form>
  );
}
