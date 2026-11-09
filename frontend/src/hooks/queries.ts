"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { getClass, getHold, listClasses, listMyHolds } from "@/api/admission";

export const queryKeys = {
  classes: ["classes"] as const,
  class: (id: string) => ["class", id] as const,
  hold: (id: string) => ["hold", id] as const,
  myHolds: (studentId: string) => ["myHolds", studentId] as const,
};

// Polling intervals from ARCHITECTURE section 6. Hidden tabs pause (QueryClient default).
export function useClasses() {
  return useQuery({
    queryKey: queryKeys.classes,
    queryFn: ({ signal }) => listClasses(signal),
    refetchInterval: 3000,
    placeholderData: keepPreviousData,
  });
}

export function useClassInfo(id: string | undefined) {
  return useQuery({
    queryKey: queryKeys.class(id ?? ""),
    queryFn: ({ signal }) => getClass(id as string, signal),
    enabled: id !== undefined,
    refetchInterval: 3000,
    placeholderData: keepPreviousData,
  });
}

export function useHold(id: string) {
  return useQuery({
    queryKey: queryKeys.hold(id),
    queryFn: ({ signal }) => getHold(id, signal),
    // Only a HELD application can still change on its own.
    refetchInterval: (query) => (query.state.data && query.state.data.status !== "HELD" ? false : 3000),
    placeholderData: keepPreviousData,
  });
}

export function useMyHolds(studentId: string | null) {
  return useQuery({
    queryKey: queryKeys.myHolds(studentId ?? ""),
    queryFn: ({ signal }) => listMyHolds(studentId as string, signal),
    enabled: studentId !== null,
    refetchInterval: 5000,
    placeholderData: keepPreviousData,
  });
}
