"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { use, useEffect, useState } from "react";
import { cancelHold, confirmHold } from "@/api/admission";
import { PageHeader } from "@/components/PageHeader";
import { LoadError, Loading } from "@/components/QueryStates";
import { Alert } from "@/components/ui/Alert";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Countdown } from "@/components/ui/Countdown";
import { ConfirmDialog } from "@/components/ui/Dialog";
import { StatusText } from "@/components/ui/StatusText";
import { useToast } from "@/components/ui/Toast";
import { queryKeys, useClassInfo, useHold } from "@/hooks/queries";
import { useNow } from "@/hooks/useNow";
import { useStudentId } from "@/hooks/useStudentId";
import { messageFor } from "@/lib/errors";
import { effectiveStatus } from "@/lib/hold";

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-4 border-b border-line py-4 last:border-b-0 sm:grid sm:grid-cols-[160px_1fr]">
      <dt className="text-label text-muted">{label}</dt>
      <dd className="text-right sm:text-left">{children}</dd>
    </div>
  );
}

export default function ApplicationDetailPage({ params }: PageProps<"/applications/[holdId]">) {
  const { holdId } = use(params);
  const studentId = useStudentId();
  const queryClient = useQueryClient();
  const toast = useToast();
  const now = useNow();
  const [confirmingCancel, setConfirmingCancel] = useState(false);

  const hold = useHold(holdId);
  const classInfo = useClassInfo(hold.data?.classId);

  const status = hold.data ? effectiveStatus(hold.data.status, hold.data.expiresAt, now) : null;
  // The countdown reached zero while the server still says HELD: refresh everything once.
  const lapsed = hold.data?.status === "HELD" && status === "EXPIRED";

  useEffect(() => {
    if (!lapsed) return;
    void queryClient.invalidateQueries({ queryKey: queryKeys.hold(holdId) });
    void queryClient.invalidateQueries({ queryKey: queryKeys.classes });
    void queryClient.invalidateQueries({ queryKey: ["myHolds"] });
  }, [lapsed, holdId, queryClient]);

  function refreshAll() {
    void queryClient.invalidateQueries({ queryKey: queryKeys.hold(holdId) });
    void queryClient.invalidateQueries({ queryKey: queryKeys.classes });
    void queryClient.invalidateQueries({ queryKey: ["class"] });
    void queryClient.invalidateQueries({ queryKey: ["myHolds"] });
  }

  const pay = useMutation({
    mutationFn: () => confirmHold(holdId, studentId as string),
    onSuccess: () => toast("Seat confirmed"),
    onSettled: refreshAll,
  });

  const cancel = useMutation({
    mutationFn: () => cancelHold(holdId, studentId as string),
    onSuccess: () => {
      setConfirmingCancel(false);
      toast("Application cancelled");
    },
    onError: () => setConfirmingCancel(false),
    onSettled: refreshAll,
  });

  const error = pay.isError ? pay.error : cancel.isError ? cancel.error : null;
  const busy = pay.isPending || cancel.isPending;
  const canAct = status === "HELD" && studentId !== null && !busy;

  return (
    <>
      <PageHeader title="Application" />
      {hold.isPending ? (
        <Loading />
      ) : hold.isError && !hold.data ? (
        <LoadError error={hold.error} onRetry={() => void hold.refetch()} />
      ) : (
        hold.data &&
        status && (
          <div className="flex flex-col gap-4">
            {error !== null && <Alert>{messageFor(error)}</Alert>}
            <Card className="px-6">
            <dl>
              <Row label="Class">{classInfo.data?.name ?? <span className="text-muted">-</span>}</Row>
              <Row label="Status">
                <span aria-live="polite">
                  <StatusText status={status} />
                </span>
              </Row>
              <Row label="Expires">
                {status === "HELD" ? <Countdown expiresAt={hold.data.expiresAt} /> : <span className="text-muted">-</span>}
              </Row>
              <Row label="Student">
                {studentId ? <span className="tabular-nums">{studentId}</span> : <span className="text-muted">-</span>}
              </Row>
            </dl>
            </Card>

            {!studentId && status === "HELD" && (
              <p className="text-body text-muted">Enter a student ID to pay or cancel.</p>
            )}

            {(status === "HELD" || status === "EXPIRED") && (
              <div className="flex flex-wrap gap-2">
                <Button
                  disabled={!canAct}
                  loading={pay.isPending}
                  loadingLabel="Paying…"
                  onClick={() => {
                    cancel.reset();
                    pay.mutate();
                  }}
                >
                  Pay admission fee
                </Button>
                <Button
                  variant="destructive"
                  disabled={!canAct}
                  onClick={() => {
                    pay.reset();
                    setConfirmingCancel(true);
                  }}
                >
                  Cancel application
                </Button>
              </div>
            )}
          </div>
        )
      )}

      <ConfirmDialog
        open={confirmingCancel}
        onOpenChange={setConfirmingCancel}
        title="Cancel application"
        description="The seat will be released."
        confirmLabel="Cancel application"
        dismissLabel="Keep application"
        loading={cancel.isPending}
        loadingLabel="Cancelling…"
        onConfirm={() => cancel.mutate()}
      />
    </>
  );
}
