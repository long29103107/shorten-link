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

export function DropdownMenuContent({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
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
      const left = Math.max(12, Math.min(rect.right - contentWidth, window.innerWidth - contentWidth - 12));

      setStyle({
        left,
        top: rect.bottom + 8,
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
  }, [context]);

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
