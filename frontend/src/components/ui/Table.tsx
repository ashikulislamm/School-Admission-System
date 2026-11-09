import type { ReactNode } from "react";
import { cn } from "@/lib/cn";

// Semantic table that stacks into label/value rows on mobile (DESIGN section 6).
export function Table({ children, label }: { children: ReactNode; label: string }) {
  return (
    <div className="overflow-hidden rounded-card border border-line bg-surface shadow-card">
      <table aria-label={label} className="w-full border-collapse text-left max-sm:block">
        {children}
      </table>
    </div>
  );
}

export function THead({ children }: { children: ReactNode }) {
  return (
    <thead className="max-sm:sr-only">
      <tr className="h-11 border-b border-line bg-bg">{children}</tr>
    </thead>
  );
}

export function Th({ children, numeric, action }: { children?: ReactNode; numeric?: boolean; action?: boolean }) {
  return (
    <th scope="col" className={cn("px-4 text-label text-muted", (numeric || action) && "text-right")}>
      {action ? <span className="sr-only">{children ?? "Actions"}</span> : children}
    </th>
  );
}

export function TBody({ children }: { children: ReactNode }) {
  return <tbody className="max-sm:block">{children}</tbody>;
}

export function Tr({ children }: { children: ReactNode }) {
  return <tr className="h-16 border-b border-line last:border-b-0 hover:bg-row-hover max-sm:block max-sm:h-auto max-sm:px-4 max-sm:py-3">{children}</tr>;
}

export interface TdProps {
  children?: ReactNode;
  /** Column name shown on the left when stacked on mobile. Omit for the action cell. */
  label?: string;
  numeric?: boolean;
}

export function Td({ children, label, numeric }: TdProps) {
  return (
    <td
      data-label={label}
      className={cn(
        "px-4",
        numeric && "text-right tabular-nums",
        "max-sm:flex max-sm:items-center max-sm:justify-between max-sm:gap-4 max-sm:px-0 max-sm:py-1.5 max-sm:text-right",
        label
          ? "max-sm:before:text-label max-sm:before:text-muted max-sm:before:content-[attr(data-label)]"
          : "max-sm:justify-end",
      )}
    >
      {children}
    </td>
  );
}

/** Right-aligned cell for row actions (at most two buttons). */
export function ActionTd({ children }: { children: ReactNode }) {
  return (
    <Td>
      <div className="flex items-center justify-end gap-2">{children}</div>
    </Td>
  );
}
