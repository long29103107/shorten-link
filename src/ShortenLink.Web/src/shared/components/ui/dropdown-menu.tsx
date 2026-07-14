import type { ButtonHTMLAttributes, HTMLAttributes, ReactNode } from "react";
import { cn } from "../../lib/utils";

type DropdownMenuProps = HTMLAttributes<HTMLDivElement>;

export function DropdownMenu({ className, ...props }: DropdownMenuProps) {
  return <div className={cn("ui-dropdown", className)} {...props} />;
}

type DropdownMenuTriggerProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  children: ReactNode;
};

export function DropdownMenuTrigger({
  className,
  type = "button",
  ...props
}: DropdownMenuTriggerProps) {
  return <button className={cn("ui-dropdown-trigger", className)} type={type} {...props} />;
}

export function DropdownMenuContent({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn("ui-dropdown-content", className)} {...props} />;
}

type DropdownMenuItemProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  inset?: boolean;
};

export function DropdownMenuItem({
  className,
  inset,
  type = "button",
  ...props
}: DropdownMenuItemProps) {
  return (
    <button
      className={cn("ui-dropdown-item", inset && "ui-dropdown-item-inset", className)}
      type={type}
      {...props}
    />
  );
}
