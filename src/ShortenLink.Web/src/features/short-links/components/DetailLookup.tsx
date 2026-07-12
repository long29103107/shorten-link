import { FormEvent, useState } from "react";

type DetailLookupProps = {
  onOpenDetails: (code: string) => void;
};

export function DetailLookup({ onOpenDetails }: DetailLookupProps) {
  const [code, setCode] = useState("");

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const trimmedCode = code.trim();
    if (!trimmedCode) {
      return;
    }

    onOpenDetails(trimmedCode);
  };

  return (
    <form className="lookup-form" onSubmit={handleSubmit}>
      <label className="field field-inline">
        <span className="field-label">Open details</span>
        <input
          className="text-input"
          value={code}
          onChange={(event) => setCode(event.target.value)}
          placeholder="short code"
        />
      </label>
      <button className="action-button action-button-secondary" type="submit">
        Inspect
      </button>
    </form>
  );
}
