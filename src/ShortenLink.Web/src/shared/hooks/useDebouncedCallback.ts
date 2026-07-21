import { useCallback, useEffect, useRef } from "react";

export function useDebouncedCallback<TArgs extends unknown[]>(
  callback: (...args: TArgs) => void,
  delayMs = 300
) {
  const callbackRef = useRef(callback);
  const timeoutRef = useRef<number | null>(null);
  const pendingArgsRef = useRef<TArgs | null>(null);

  useEffect(() => {
    callbackRef.current = callback;
  }, [callback]);

  const cancel = useCallback(() => {
    if (timeoutRef.current !== null) window.clearTimeout(timeoutRef.current);
    timeoutRef.current = null;
    pendingArgsRef.current = null;
  }, []);

  const flush = useCallback(() => {
    if (pendingArgsRef.current === null) return;
    const args = pendingArgsRef.current;
    cancel();
    callbackRef.current(...args);
  }, [cancel]);

  const invoke = useCallback((...args: TArgs) => {
    cancel();
    pendingArgsRef.current = args;
    timeoutRef.current = window.setTimeout(flush, Math.max(0, delayMs));
  }, [cancel, delayMs, flush]);

  useEffect(() => cancel, [cancel]);

  return { invoke, cancel, flush } as const;
}
