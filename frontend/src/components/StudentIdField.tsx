"use client";

import { useId, useState } from "react";
import { Input } from "@/components/ui/Input";
import { useStudentId } from "@/hooks/useStudentId";
import { INVALID_STUDENT_ID_COPY } from "@/lib/errors";
import { cn } from "@/lib/cn";
import { isValidStudentId, sanitizeStudentId, setStudentId } from "@/lib/student-id";

/**
 * The identity input. What is typed is the identity: only a complete 17-digit ID is stored,
 * and a partial one clears it so no request is ever sent under a stale ID.
 */
export function StudentIdField() {
  const stored = useStudentId();
  const [draft, setDraft] = useState<string | null>(null);
  const messageId = useId();

  const value = draft ?? stored ?? "";
  const invalid = value.length > 0 && !isValidStudentId(value);

  function onChange(raw: string) {
    const digits = sanitizeStudentId(raw);
    setDraft(digits);
    setStudentId(isValidStudentId(digits) ? digits : null);
  }

  return (
    <div className="flex flex-wrap items-center justify-end gap-x-3 max-sm:w-full">
      {invalid && (
        <p
          id={messageId}
          role="status"
          className={cn("text-label font-normal text-danger", "max-sm:order-last max-sm:w-full max-sm:pb-2")}
        >
          {INVALID_STUDENT_ID_COPY}
        </p>
      )}
      <Input
        label="Student ID"
        hideLabel
        hideErrorText
        describedBy={invalid ? messageId : undefined}
        error={invalid ? INVALID_STUDENT_ID_COPY : null}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        inputMode="numeric"
        autoComplete="off"
        maxLength={17}
        placeholder="Student ID"
        className="w-52 tabular-nums"
      />
    </div>
  );
}
