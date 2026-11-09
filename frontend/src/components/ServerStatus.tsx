"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useSyncExternalStore } from "react";
import { Alert } from "@/components/ui/Alert";
import { isUnreachable, UNREACHABLE_COPY } from "@/lib/errors";

/** Persistent alert while any on-screen query cannot reach the server. Clears when it recovers. */
export function ServerStatus() {
  const cache = useQueryClient().getQueryCache();
  const down = useSyncExternalStore(
    (onChange) => cache.subscribe(onChange),
    () => cache.getAll().some((q) => q.getObserversCount() > 0 && q.state.status === "error" && isUnreachable(q.state.error)),
    () => false,
  );
  if (!down) return null;
  return (
    <div className="mb-4">
      <Alert>{UNREACHABLE_COPY}</Alert>
    </div>
  );
}
