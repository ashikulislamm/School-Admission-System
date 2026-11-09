"use client";

import * as RadixToast from "@radix-ui/react-toast";
import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";

interface ToastItem {
  id: number;
  message: string;
}

const ToastContext = createContext<(message: string) => void>(() => {});

/** Shows a short bottom-right toast (3s). Messages are fixed strings, never server text. */
export function useToast(): (message: string) => void {
  return useContext(ToastContext);
}

let nextId = 1;

export function ToastProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<ToastItem[]>([]);

  const show = useCallback((message: string) => {
    setItems((prev) => [...prev, { id: nextId++, message }]);
  }, []);
  const remove = useCallback((id: number) => {
    setItems((prev) => prev.filter((t) => t.id !== id));
  }, []);
  const value = useMemo(() => show, [show]);

  return (
    <ToastContext.Provider value={value}>
      <RadixToast.Provider duration={3000} label="Notification">
        {children}
        {items.map((t) => (
          <RadixToast.Root
            key={t.id}
            type="background"
            onOpenChange={(open) => {
              if (!open) remove(t.id);
            }}
            className="rounded-card border border-line bg-surface px-4 py-3 text-body text-fg shadow-overlay"
          >
            <RadixToast.Description>{t.message}</RadixToast.Description>
          </RadixToast.Root>
        ))}
        <RadixToast.Viewport className="fixed right-4 bottom-4 z-50 flex w-[320px] max-w-[calc(100vw-32px)] flex-col gap-2 outline-none" />
      </RadixToast.Provider>
    </ToastContext.Provider>
  );
}
