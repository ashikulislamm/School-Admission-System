import { apiFetch } from "./client";
import type { ClassSummary, CreatedClass, Hold, HoldConfirmed, HoldCreated, MyHold } from "./types";

// One function per endpoint. Ids are URL-encoded; the student ID is passed where the API needs it.
const enc = encodeURIComponent;

export const createClass = (name: string, totalSeats: number) =>
  apiFetch<CreatedClass>("/classes", { method: "POST", body: { name, totalSeats } });

export const listClasses = (signal?: AbortSignal) => apiFetch<ClassSummary[]>("/classes", { signal });

export const getClass = (id: string, signal?: AbortSignal) =>
  apiFetch<ClassSummary>(`/classes/${enc(id)}`, { signal });

export const applyToClass = (classId: string, studentId: string) =>
  apiFetch<HoldCreated>(`/classes/${enc(classId)}/holds`, { method: "POST", studentId });

export const getHold = (holdId: string, signal?: AbortSignal) =>
  apiFetch<Hold>(`/holds/${enc(holdId)}`, { signal });

export const confirmHold = (holdId: string, studentId: string) =>
  apiFetch<HoldConfirmed>(`/holds/${enc(holdId)}/confirm`, { method: "POST", studentId });

export const cancelHold = (holdId: string, studentId: string) =>
  apiFetch<void>(`/holds/${enc(holdId)}`, { method: "DELETE", studentId });

export const listMyHolds = (studentId: string, signal?: AbortSignal) =>
  apiFetch<MyHold[]>("/me/holds", { studentId, signal });
