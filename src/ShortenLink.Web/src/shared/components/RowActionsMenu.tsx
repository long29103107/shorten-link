import { useState } from "react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger
} from "./ui/dropdown-menu";

export type RowAction = {
  id: string;
  label: string;
  onSelect: () => void;
  disabled?: boolean;
  destructive?: boolean;
};

type RowActionsMenuProps = {
  label: string;
  actions: readonly RowAction[];
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
};

export function RowActionsMenu({
  label,
  actions,
  open: controlledOpen,
  onOpenChange
}: RowActionsMenuProps) {
  const [internalOpen, setInternalOpen] = useState(false);
  const open = controlledOpen ?? internalOpen;
  const setOpen = onOpenChange ?? setInternalOpen;

  if (actions.length === 0) return null;

  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger aria-expanded={open} aria-label={label}>
        ...
      </DropdownMenuTrigger>
      {open ? (
        <DropdownMenuContent>
          {actions.map((action) => (
            <DropdownMenuItem
              key={action.id}
              className={action.destructive ? "danger-link" : undefined}
              disabled={action.disabled}
              onClick={action.onSelect}
            >
              {action.label}
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      ) : null}
    </DropdownMenu>
  );
}
