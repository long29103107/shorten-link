import {
  createContext,
  useContext,
  useLayoutEffect,
  useRef,
  useState,
  type ButtonHTMLAttributes,
  type CSSProperties,
  type HTMLAttributes,
  type ReactNode
} from "react";
import { createPortal } from "react-dom";
import { useClickOutside } from "../../hooks/useClickOutside";
import { cn } from "../../lib/utils";

type DropdownMenuProps = HTMLAttributes<HTMLDivElement> & {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
};

type DropdownMenuContextValue = {
  triggerElement: HTMLButtonElement | null;
  contentElement: HTMLDivElement | null;
  open: boolean;
  onOpenChange?: (open: boolean) => void;
  setTriggerElement: (element: HTMLButtonElement | null) => void;
  setContentElement: (element: HTMLDivElement | null) => void;
};

const DropdownMenuContext = createContext<DropdownMenuContextValue | null>(null);

export function DropdownMenu({
  className,
  open = false,
  onOpenChange,
  ...props
}: DropdownMenuProps) {
  const [triggerElement, setTriggerElement] = useState<HTMLButtonElement | null>(null);
  const [contentElement, setContentElement] = useState<HTMLDivElement | null>(null);
  const triggerRef = useRef<HTMLButtonElement | null>(null);
  const contentRef = useRef<HTMLDivElement | null>(null);

  triggerRef.current = triggerElement;
  contentRef.current = contentElement;

  useClickOutside(
    [triggerRef, contentRef],
    () => onOpenChange?.(false),
    open && Boolean(onOpenChange)
  );

  return (
    <DropdownMenuContext.Provider
      value={{
        triggerElement,
        contentElement,
        open,
        onOpenChange,
        setTriggerElement,
        setContentElement
      }}
    >
      <div className={cn("ui-dropdown", className)} {...props} />
    </DropdownMenuContext.Provider>
  );
}

type DropdownMenuTriggerProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  children: ReactNode;
};

export function DropdownMenuTrigger({
  className,
  type = "button",
  onClick,
  ...props
}: DropdownMenuTriggerProps) {
  const context = useContext(DropdownMenuContext);

  return (
    <button
      className={cn("ui-dropdown-trigger", className)}
      ref={context?.setTriggerElement}
      onClick={(event) => {
        onClick?.(event);
        context?.onOpenChange?.(!context.open);
      }}
      type={type}
      {...props}
    />
  );
}

type DropdownMenuContentProps = HTMLAttributes<HTMLDivElement> & {
  placement?: "bottom-end" | "right-end";
};

export function DropdownMenuContent({
  className,
  placement = "bottom-end",
  ...props
}: DropdownMenuContentProps) {
  const context = useContext(DropdownMenuContext);
  const contentRef = useRef<HTMLDivElement | null>(null);
  const [style, setStyle] = useState<CSSProperties>({ visibility: "hidden" });

  useLayoutEffect(() => {
    const trigger = context?.triggerElement;
    if (!trigger) {
      return;
    }

    const updatePosition = () => {
      const rect = trigger.getBoundingClientRect();
      const contentWidth = contentRef.current?.offsetWidth ?? 174;
      const contentHeight = contentRef.current?.offsetHeight ?? 80;
      const left = placement === "right-end"
        ? Math.max(12, Math.min(rect.right + 8, window.innerWidth - contentWidth - 12))
        : Math.max(12, Math.min(rect.right - contentWidth, window.innerWidth - contentWidth - 12));
      const top = placement === "right-end"
        ? Math.max(12, Math.min(rect.bottom - contentHeight, window.innerHeight - contentHeight - 12))
        : rect.bottom + 8;

      setStyle({
        left,
        top,
        visibility: "visible"
      });
    };

    updatePosition();
    const closeOnViewportChange = () => context?.onOpenChange?.(false);

    window.addEventListener("resize", closeOnViewportChange);
    window.addEventListener("scroll", closeOnViewportChange, true);
    return () => {
      window.removeEventListener("resize", closeOnViewportChange);
      window.removeEventListener("scroll", closeOnViewportChange, true);
    };
  }, [context, placement]);

  if (typeof document === "undefined") {
    return null;
  }

  return createPortal(
    <div
      className={cn("ui-dropdown-content", className)}
      onPointerDown={(event) => {
        event.stopPropagation();
        props.onPointerDown?.(event);
      }}
      ref={(element) => {
        contentRef.current = element;
        context?.setContentElement(element);
      }}
      style={style}
      {...props}
    />,
    document.body
  );
}

type DropdownMenuItemProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  inset?: boolean;
};

export function DropdownMenuItem({
  className,
  inset,
  type = "button",
  onClick,
  ...props
}: DropdownMenuItemProps) {
  const context = useContext(DropdownMenuContext);

  return (
    <button
      className={cn("ui-dropdown-item", inset && "ui-dropdown-item-inset", className)}
      onClick={(event) => {
        onClick?.(event);
        context?.onOpenChange?.(false);
      }}
      type={type}
      {...props}
    />
  );
}
