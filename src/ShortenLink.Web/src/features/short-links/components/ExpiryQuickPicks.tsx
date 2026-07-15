type ExpiryQuickPicksProps = {
  onChange: (expiredAtLocal: string) => void;
};

const expiryOptions = [
  { label: "+30m", minutes: 30 },
  { label: "+60m", minutes: 60 },
  { label: "+180m", minutes: 180 },
  { label: "+6h", minutes: 6 * 60 },
  { label: "+12h", minutes: 12 * 60 }
];

export function ExpiryQuickPicks({ onChange }: ExpiryQuickPicksProps) {
  return (
    <div className="expiry-quick-picks" aria-label="Quick expiry choices">
      {expiryOptions.map((option) => (
        <button
          key={option.label}
          type="button"
          className="expiry-quick-button"
          onClick={() => onChange(toDateTimeLocalValue(addMinutes(new Date(), option.minutes)))}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}

function addMinutes(date: Date, minutes: number): Date {
  return new Date(date.getTime() + minutes * 60_000);
}

function toDateTimeLocalValue(date: Date): string {
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}
