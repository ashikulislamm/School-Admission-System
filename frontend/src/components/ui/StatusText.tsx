import type { HoldStatus } from "@/api/types";
import { cn } from "@/lib/cn";

const COLOR: Record<HoldStatus, string> = {
  HELD: "text-warning",
  CONFIRMED: "text-success",
  EXPIRED: "text-neutral",
  CANCELLED: "text-neutral",
};

/** Plain colored uppercase word. Never boxed (DESIGN section 7). */
export function StatusText({ status }: { status: HoldStatus }) {
  return <span className={cn("text-label font-semibold uppercase", COLOR[status])}>{status}</span>;
}
