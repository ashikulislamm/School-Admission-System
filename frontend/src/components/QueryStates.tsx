import type { ReactNode } from "react";
import { Card } from "@/components/ui/Card";
import { Alert } from "@/components/ui/Alert";
import { isUnreachable, messageFor } from "@/lib/errors";

/** Static muted placeholder (no shimmer, DESIGN section 10). */
export function Loading() {
  return (
    <Card className="p-8 text-center text-body text-muted">
      <p>Loading</p>
    </Card>
  );
}

export function Empty({ children }: { children: ReactNode }) {
  return (
    <Card className="flex flex-col items-center gap-3 p-8 text-center text-body text-muted">{children}</Card>
  );
}

/**
 * Initial-load failure. An unreachable server is already announced by ServerStatus,
 * so only other failures get their own alert.
 */
export function LoadError({ error, onRetry }: { error: unknown; onRetry: () => void }) {
  if (isUnreachable(error)) return null;
  return (
    <div className="py-6">
      <Alert onRetry={onRetry}>{messageFor(error)}</Alert>
    </div>
  );
}
