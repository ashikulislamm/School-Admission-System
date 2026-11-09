import type { HoldStatus } from "@/api/types";

/** A HELD application whose time has run out is shown as EXPIRED before the server says so. */
export function effectiveStatus(status: HoldStatus, expiresAt: string, nowMs: number): HoldStatus {
  if (status === "HELD" && nowMs > 0 && Date.parse(expiresAt) <= nowMs) return "EXPIRED";
  return status;
}

export function remainingSeconds(expiresAt: string, nowMs: number): number {
  return Math.max(0, Math.ceil((Date.parse(expiresAt) - nowMs) / 1000));
}

export function formatCountdown(seconds: number): string {
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
}
