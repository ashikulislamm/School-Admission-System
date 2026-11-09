import type { ReactNode } from "react";
import { Button } from "./Button";

export interface AlertProps {
  children: ReactNode;
  /** Shows a Retry button when a retry makes sense. */
  onRetry?: () => void;
}

/** Inline error: one plain sentence, danger text on a surface with a danger border. */
export function Alert({ children, onRetry }: AlertProps) {
  return (
    <div
      role="alert"
      className="flex w-full items-center justify-between gap-4 rounded-control border border-danger bg-surface px-4 py-3 text-body text-danger"
    >
      <p>{children}</p>
      {onRetry && (
        <Button variant="secondary" onClick={onRetry}>
          Retry
        </Button>
      )}
    </div>
  );
}
