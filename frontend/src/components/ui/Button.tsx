import type { ButtonHTMLAttributes } from "react";
import { cn } from "@/lib/cn";

export type Variant ="primary" | "secondary" | "destructive";

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  loading?: boolean;
  /** Replaces the label while loading, for example "Paying…". */
  loadingLabel?: string;
}

const VARIANTS: Record<Variant, string> = {
  primary: "bg-accent text-on-accent border border-transparent shadow-card enabled:hover:bg-accent-hover",
  secondary: "bg-surface text-fg border border-line-strong shadow-card enabled:hover:bg-row-hover",
  destructive: "bg-surface text-danger border border-line-strong shadow-card enabled:hover:bg-row-hover",
};

/** Class string shared with ButtonLink so links styled as buttons look identical. */
export function buttonClass(variant: Variant): string {
  return cn(
    "relative inline-flex h-9 items-center justify-center rounded-control px-3.5 text-body font-medium",
    VARIANTS[variant],
  );
}

export function Button({
  variant = "primary",
  loading = false,
  loadingLabel = "Working…",
  disabled,
  className,
  children,
  type = "button",
  ...props
}: ButtonProps) {
  const inactive = disabled || loading;
  return (
    <button
      type={type}
      disabled={inactive}
      aria-busy={loading || undefined}
      className={cn(
        buttonClass(variant),
        inactive && "cursor-not-allowed opacity-50",
        className,
      )}
      {...props}
    >
      {/* The original label stays in flow (invisible) so the button width does not change. */}
      <span className={cn(loading && "invisible")}>{children}</span>
      {loading && <span className="absolute inset-0 flex items-center justify-center">{loadingLabel}</span>}
    </button>
  );
}
