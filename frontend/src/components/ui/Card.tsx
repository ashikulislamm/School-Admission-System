import type { HTMLAttributes } from "react";
import { cn } from "@/lib/cn";

/** White panel on the soft background: 1px border, 12px radius, hairline shadow. */
export function Card({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn("rounded-card border border-line bg-surface shadow-card", className)} {...props} />;
}
