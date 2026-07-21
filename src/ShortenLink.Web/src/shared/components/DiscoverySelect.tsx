import type { ReactNode } from "react";

type DiscoverySelectProps<T extends string> = {
  label: string;
  value: T;
  disabled?: boolean;
  children: ReactNode;
  onChange: (value: T) => void;
};

export function DiscoverySelect<T extends string>({ label, value, disabled, children, onChange }: DiscoverySelectProps<T>) {
  return (
    <label className="admin-discovery-field">
      <span>{label}</span>
      <select value={value} disabled={disabled} onChange={(event) => onChange(event.target.value as T)}>{children}</select>
    </label>
  );
}
