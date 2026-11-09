"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { createClass } from "@/api/admission";
import { PageHeader } from "@/components/PageHeader";
import { Alert } from "@/components/ui/Alert";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Input } from "@/components/ui/Input";
import { useToast } from "@/components/ui/Toast";
import { queryKeys } from "@/hooks/queries";
import { messageFor, NAME_INVALID_COPY, SEATS_INVALID_COPY } from "@/lib/errors";

const MAX_NAME = 100;
const MAX_SEATS = 100000;

export default function AdminPage() {
  const queryClient = useQueryClient();
  const toast = useToast();
  const [name, setName] = useState("");
  const [seats, setSeats] = useState("");
  const [nameError, setNameError] = useState<string | null>(null);
  const [seatsError, setSeatsError] = useState<string | null>(null);

  const create = useMutation({
    mutationFn: ({ name, totalSeats }: { name: string; totalSeats: number }) => createClass(name, totalSeats),
    onSuccess: () => {
      setName("");
      setSeats("");
      toast("Class created");
      void queryClient.invalidateQueries({ queryKey: queryKeys.classes });
    },
  });

  function onSubmit(e: FormEvent) {
    e.preventDefault();
    const trimmed = name.trim();
    const nameOk = trimmed.length >= 1 && trimmed.length <= MAX_NAME;
    const seatsOk = /^\d+$/.test(seats) && Number(seats) >= 1 && Number(seats) <= MAX_SEATS;
    setNameError(nameOk ? null : NAME_INVALID_COPY);
    setSeatsError(seatsOk ? null : SEATS_INVALID_COPY);
    if (!nameOk || !seatsOk) return;
    create.mutate({ name: trimmed, totalSeats: Number(seats) });
  }

  return (
    <>
      <PageHeader title="Create class" />
      <Card className="max-w-md p-6">
      <form onSubmit={onSubmit} noValidate className="flex flex-col gap-4">
        {create.isError && <Alert>{messageFor(create.error)}</Alert>}
        <Input
          label="Class name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          error={nameError}
          autoComplete="off"
        />
        <Input
          label="Total seats"
          value={seats}
          onChange={(e) => setSeats(e.target.value)}
          error={seatsError}
          inputMode="numeric"
          autoComplete="off"
          className="tabular-nums"
        />
        <div>
          <Button type="submit" loading={create.isPending} loadingLabel="Creating…">
            Create class
          </Button>
        </div>
      </form>
      </Card>
    </>
  );
}
