"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { applyToClass } from "@/api/admission";
import type { ClassSummary } from "@/api/types";
import { PageHeader } from "@/components/PageHeader";
import { Empty, LoadError, Loading } from "@/components/QueryStates";
import { Alert } from "@/components/ui/Alert";
import { Button } from "@/components/ui/Button";
import { ActionTd, TBody, Table, THead, Td, Th, Tr } from "@/components/ui/Table";
import { useToast } from "@/components/ui/Toast";
import { queryKeys, useClasses } from "@/hooks/queries";
import { useStudentId } from "@/hooks/useStudentId";
import { messageFor } from "@/lib/errors";
import { cn } from "@/lib/cn";

function SeatBar({ c }: { c: ClassSummary }) {
  const used = Math.min(100, Math.round(((c.heldSeats + c.confirmedSeats) / c.totalSeats) * 100));
  return (
    <div aria-hidden className="mt-2 h-1.5 w-full max-w-48 rounded-full bg-line">
      <div className="h-1.5 rounded-full bg-accent" style={{ width: `${used}%` }} />
    </div>
  );
}

export default function ClassesPage() {
  const studentId = useStudentId();
  const router = useRouter();
  const queryClient = useQueryClient();
  const toast = useToast();
  const classes = useClasses();

  const apply = useMutation({
    mutationFn: (classId: string) => applyToClass(classId, studentId as string),
    onSuccess: (hold) => {
      toast("Seat held");
      void queryClient.invalidateQueries({ queryKey: queryKeys.classes });
      void queryClient.invalidateQueries({ queryKey: ["myHolds"] });
      router.push(`/applications/${encodeURIComponent(hold.holdId)}`);
    },
    onError: () => {
      // Counts or the student's own state changed under us: show the truth.
      void queryClient.invalidateQueries({ queryKey: queryKeys.classes });
    },
  });

  return (
    <>
      <PageHeader title="Classes" />
      {apply.isError && (
        <div className="pb-4">
          <Alert>{messageFor(apply.error)}</Alert>
        </div>
      )}
      {!studentId && <p className="pb-4 text-body font-medium text-fg">Enter a 17-digit student ID in the top bar to apply.</p>}

      {classes.isPending ? (
        <Loading />
      ) : classes.isError && !classes.data ? (
        <LoadError error={classes.error} onRetry={() => void classes.refetch()} />
      ) : classes.data && classes.data.length === 0 ? (
        <Empty>
          <p>No classes yet.</p>
          <Link href="/admin" className="font-medium text-accent hover:text-accent-hover">
            Create class
          </Link>
        </Empty>
      ) : (
        classes.data && (
          <Table label="Classes">
            <THead>
              <Th>Name</Th>
              <Th numeric>Total</Th>
              <Th numeric>Held</Th>
              <Th numeric>Confirmed</Th>
              <Th numeric>Available</Th>
              <Th action>Apply</Th>
            </THead>
            <TBody>
              {classes.data.map((c) => {
                const full = c.availableSeats <= 0;
                const pending = apply.isPending && apply.variables === c.id;
                return (
                  <Tr key={c.id}>
                    <Td label="Name">
                      <div className="max-sm:text-right">
                        <div className="font-medium">{c.name}</div>
                        <SeatBar c={c} />
                      </div>
                    </Td>
                    <Td label="Total" numeric>
                      {c.totalSeats}
                    </Td>
                    <Td label="Held" numeric>
                      {c.heldSeats}
                    </Td>
                    <Td label="Confirmed" numeric>
                      {c.confirmedSeats}
                    </Td>
                    <Td label="Available" numeric>
                      <span className={cn("font-semibold", full && "text-danger")}>{c.availableSeats}</span>
                    </Td>
                    <ActionTd>
                      <Button
                        variant="secondary"
                        disabled={full || !studentId || apply.isPending}
                        loading={pending}
                        loadingLabel="Applying…"
                        onClick={() => apply.mutate(c.id)}
                      >
                        Apply
                      </Button>
                    </ActionTd>
                  </Tr>
                );
              })}
            </TBody>
          </Table>
        )
      )}
    </>
  );
}
