"use client";

import * as RadixDialog from "@radix-ui/react-dialog";
import { Button } from "./Button";

export interface ConfirmDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  confirmLabel: string;
  dismissLabel: string;
  loading?: boolean;
  loadingLabel?: string;
  onConfirm: () => void;
}

/** Only used for destructive confirmation (DESIGN section 7). Esc and overlay click dismiss. */
export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel,
  dismissLabel,
  loading,
  loadingLabel,
  onConfirm,
}: ConfirmDialogProps) {
  return (
    <RadixDialog.Root open={open} onOpenChange={onOpenChange}>
      <RadixDialog.Portal>
        <RadixDialog.Overlay className="fixed inset-0 bg-[rgba(0,0,0,0.4)]" />
        <RadixDialog.Content className="fixed top-1/2 left-1/2 w-[calc(100%-32px)] max-w-[400px] -translate-x-1/2 -translate-y-1/2 rounded-card border border-line bg-surface p-6 shadow-overlay">
          <RadixDialog.Title className="text-section text-fg">{title}</RadixDialog.Title>
          <RadixDialog.Description className="mt-1 text-body text-muted">{description}</RadixDialog.Description>
          <div className="mt-4 flex justify-end gap-2">
            <RadixDialog.Close asChild>
              <Button variant="secondary" disabled={loading}>
                {dismissLabel}
              </Button>
            </RadixDialog.Close>
            <Button variant="destructive" loading={loading} loadingLabel={loadingLabel} onClick={onConfirm}>
              {confirmLabel}
            </Button>
          </div>
        </RadixDialog.Content>
      </RadixDialog.Portal>
    </RadixDialog.Root>
  );
}
