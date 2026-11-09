"use client";

import { Button } from "@/components/ui/Button";
import { UNKNOWN_COPY } from "@/lib/errors";
import "./globals.css";

// Last resort when the root layout itself fails; it must render its own html and body.
export default function GlobalError() {
  return (
    <html lang="en">
      <body className="min-h-screen bg-bg font-sans text-body text-fg">
        <main className="mx-auto flex w-full max-w-[1040px] flex-col items-start gap-3 px-4 py-6 sm:px-6">
          <div role="alert" className="w-full rounded-control border border-danger bg-surface px-4 py-3 text-danger">
            {UNKNOWN_COPY}
          </div>
          <Button variant="secondary" onClick={() => window.location.reload()}>
            Reload
          </Button>
        </main>
      </body>
    </html>
  );
}
