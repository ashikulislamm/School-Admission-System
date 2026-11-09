import { ApiError } from "@/api/client";

// All user-facing error copy lives here (RULES section 3, DESIGN section 9).
// Never render raw server or JS messages.
export const UNREACHABLE_COPY = "Can't reach the server. Retrying.";
export const UNKNOWN_COPY = "Something went wrong. Try again.";
export const INVALID_STUDENT_ID_COPY = "Enter a 17-digit birth registration number.";

const COPY: Record<string, string> = {
  CLASS_FULL: "All seats in this class are taken.",
  HOLD_EXPIRED: "This application has expired.",
  HOLD_NOT_ACTIVE: "This application is no longer active.",
  ALREADY_APPLIED: "You already have an application for this class.",
  NOT_OWNER: "This application belongs to someone else.",
  NOT_FOUND: "That item was not found.",
  INVALID_STUDENT_ID: INVALID_STUDENT_ID_COPY,
  INVALID_INPUT: "Check the highlighted fields.",
  SERVICE_UNAVAILABLE: UNREACHABLE_COPY,
  NETWORK_ERROR: UNREACHABLE_COPY,
};

/** True when the server could not be reached or is down, so a retry may help. */
export function isUnreachable(error: unknown): boolean {
  return error instanceof ApiError && (error.code === "NETWORK_ERROR" || error.code === "SERVICE_UNAVAILABLE");
}

export function messageFor(error: unknown): string {
  if (error instanceof ApiError) {
    return COPY[error.code] ?? UNKNOWN_COPY;
  }
  return UNKNOWN_COPY;
}
