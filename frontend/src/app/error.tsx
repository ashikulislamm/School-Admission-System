"use client";

import { Alert } from "@/components/ui/Alert";
import { Button } from "@/components/ui/Button";
import { UNKNOWN_COPY } from "@/lib/errors";

// Route-level boundary: never a blank page. Next passes `retry` in this version (not `reset`).
export default function ErrorBoundary({ retry }: { error: unknown; retry: () => void }) {
  return (
    <div className="flex flex-col items-start gap-3 py-6">
      <Alert>{UNKNOWN_COPY}</Alert>
      <Button variant="secondary" onClick={() => retry()}>
        Reload
      </Button>
    </div>
  );
}
