import type { ReactNode } from "react";

/** Page title with an optional primary action on the right, above a 1px divider. */
export function PageHeader({ title, action }: { title: string; action?: ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-4 border-b border-line pb-4">
      <h1 className="text-title text-fg">{title}</h1>
      {action}
    </div>
  );
}
