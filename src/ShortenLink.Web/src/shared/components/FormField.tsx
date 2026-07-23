import type { HTMLInputTypeAttribute } from "react";
import { Input } from "./ui/input";
import { Label } from "./ui/label";

type FormFieldProps = {
  id: string;
  label: string;
  value: string;
  error?: string;
  type?: HTMLInputTypeAttribute;
  autoComplete?: string;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  onChange: (value: string) => void;
};

export function FormField({ id, label, value, error, type = "text", autoComplete, placeholder, required, disabled, onChange }: FormFieldProps) {
  const errorId = `${id}-error`;
  return (
    <Label className="field" htmlFor={id}>
      <span className="field-label">{label}{required ? <span className="required-marker"> *</span> : null}</span>
      <Input id={id} type={type} value={value} autoComplete={autoComplete} placeholder={placeholder} disabled={disabled} aria-invalid={error ? "true" : undefined} aria-describedby={error ? errorId : undefined} onChange={(event) => onChange(event.target.value)} />
      {error ? <span id={errorId} className="field-error">{error}</span> : null}
    </Label>
  );
}
