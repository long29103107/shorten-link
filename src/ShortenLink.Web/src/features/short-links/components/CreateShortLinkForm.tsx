import { FormEvent, useMemo, useState } from "react";
import { ApiError } from "../api/http";
import { createShortLink } from "../api/shortLinksApi";
import type { CreatedShortLink, ShortLinkFormInput } from "../types";
import { toFriendlyErrorMessage } from "../types";
import { Button } from "../../../shared/components/ui/button";
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "../../../shared/components/ui/card";
import { Input } from "../../../shared/components/ui/input";
import { Label } from "../../../shared/components/ui/label";
import { showToast } from "../../../shared/toast";
import { ExpiryQuickPicks } from "./ExpiryQuickPicks";

type CreateShortLinkFormProps = {
  onCreated: (createdLink: CreatedShortLink) => void;
};

const initialForm: ShortLinkFormInput = {
  originalUrl: "",
  expiredAtLocal: ""
};

export function CreateShortLinkForm({
  onCreated
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
        expiredAtUtc: form.expiredAtLocal
          ? new Date(form.expiredAtLocal).toISOString()
          : undefined
      });

      onCreated(createdLink);
      showToast({
        title: "Short link created",
        message: createdLink.code,
        variant: "success"
      });
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
    <Card className="panel-form">
      <form onSubmit={handleSubmit}>
      <CardHeader>
        <p className="eyebrow">Create</p>
        <CardTitle>Ship a short link in one pass.</CardTitle>
      </CardHeader>

      <CardContent>
      <Label className="field">
        <span className="field-label">Destination URL</span>
        <Input
          type="url"
          placeholder="https://example.com/really/long/path"
          value={form.originalUrl}
          onChange={(event) =>
            setForm((current) => ({ ...current, originalUrl: event.target.value }))
          }
        />
      </Label>

      <div className="field-grid">
        <Label className="field">
          <span className="field-label">Expiry</span>
          <Input
            type="datetime-local"
            value={form.expiredAtLocal}
            onChange={(event) =>
              setForm((current) => ({ ...current, expiredAtLocal: event.target.value }))
            }
          />
          <ExpiryQuickPicks
            onChange={(expiredAtLocal) =>
              setForm((current) => ({ ...current, expiredAtLocal }))
            }
          />
        </Label>
      </div>

      {errorMessage ? <p className="feedback feedback-error">{errorMessage}</p> : null}
      </CardContent>

      <CardFooter style={{ flexDirection: "row-reverse" }}>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Creating..." : "Create short link"}
        </Button>
        <Button
          variant="secondary"
          onClick={() => setForm(initialForm)}
          disabled={isSubmitting}
        >
          Clear
        </Button>
      </CardFooter>
      </form>
    </Card>
  );
}
