"use client";

import { useNow } from "@/hooks/useNow";
import { formatCountdown, remainingSeconds } from "@/lib/hold";
import { cn } from "@/lib/cn";

/** mm:ss until `expiresAt`, derived from the server clock estimate. Warning color under 60s. */
export function Countdown({ expiresAt }: { expiresAt: string }) {
  const now = useNow();
  if (now === 0) return <span className="font-semibold tabular-nums text-muted">--:--</span>;
  const seconds = remainingSeconds(expiresAt, now);
  return (
    <span className={cn("font-semibold tabular-nums", seconds < 60 ? "text-warning" : "text-fg")}>
      {formatCountdown(seconds)}
    </span>
  );
}
