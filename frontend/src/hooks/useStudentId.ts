"use client";

import { useSyncExternalStore } from "react";
import { getStudentId, subscribeStudentId } from "@/lib/student-id";

/** The valid stored student ID, or null. Null on the server and before hydration. */
export function useStudentId(): string | null {
  return useSyncExternalStore(subscribeStudentId, getStudentId, () => null);
}
