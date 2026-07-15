import { useEffect, type RefObject } from "react";

export function useClickOutside(
  refs: Array<RefObject<HTMLElement | null>>,
  onClickOutside: () => void,
  enabled = true
) {
  useEffect(() => {
    if (!enabled) {
      return;
    }

    const handlePointerDown = (event: PointerEvent) => {
      const target = event.target;
      if (!(target instanceof Node)) {
        return;
      }

      const eventPath = event.composedPath();
      const isInside = refs.some((ref) => {
        const element = ref.current;
        return element
          ? element.contains(target) || eventPath.includes(element)
          : false;
      });
      if (!isInside) {
        onClickOutside();
      }
    };

    document.addEventListener("pointerdown", handlePointerDown);
    return () => document.removeEventListener("pointerdown", handlePointerDown);
  }, [enabled, onClickOutside, refs]);
}
