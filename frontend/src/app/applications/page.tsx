"use client";

import { PageHeader } from "@/components/PageHeader";
import { Empty, LoadError, Loading } from "@/components/QueryStates";
import { Countdown } from "@/components/ui/Countdown";
import { ButtonLink } from "@/components/ui/ButtonLink";
import { StatusText } from "@/components/ui/StatusText";
import { ActionTd, TBody, Table, THead, Td, Th, Tr } from "@/components/ui/Table";
import { useMyHolds } from "@/hooks/queries";
import { useNow } from "@/hooks/useNow";
import { useStudentId } from "@/hooks/useStudentId";
import { effectiveStatus } from "@/lib/hold";

export default function ApplicationsPage() {
  const studentId = useStudentId();
  const holds = useMyHolds(studentId);
  const now = useNow();

  return (
    <>
      <PageHeader title="Applications" />
      {!studentId ? (
        <Empty>
          <p>Enter a student ID to see your applications.</p>
        </Empty>
      ) : holds.isPending ? (
        <Loading />
      ) : holds.isError && !holds.data ? (
        <LoadError error={holds.error} onRetry={() => void holds.refetch()} />
      ) : holds.data && holds.data.length === 0 ? (
        <Empty>
          <p>No applications yet.</p>
        </Empty>
      ) : (
        holds.data && (
          <Table label="Applications">
            <THead>
              <Th>Class</Th>
              <Th>Status</Th>
              <Th>Expires</Th>
              <Th action>Open</Th>
            </THead>
            <TBody>
              {holds.data.map((h) => {
                const status = effectiveStatus(h.status, h.expiresAt, now);
                return (
                  <Tr key={h.holdId}>
                    <Td label="Class">
                      <span className="font-medium">{h.className}</span>
                    </Td>
                    <Td label="Status">
                      <StatusText status={status} />
                    </Td>
                    <Td label="Expires">{status === "HELD" ? <Countdown expiresAt={h.expiresAt} /> : <span className="text-muted">-</span>}</Td>
                    <ActionTd>
                      <ButtonLink href={`/applications/${encodeURIComponent(h.holdId)}`}>Open</ButtonLink>
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
