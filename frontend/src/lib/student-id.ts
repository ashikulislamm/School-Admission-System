// Student identity: a 17-digit birth registration number kept in localStorage.
// Only a valid ID is ever stored, so `getStudentId()` is either valid or null.
export const STUDENT_ID_KEY = "studentId";
export const STUDENT_ID_LENGTH = 17;

const CHANGE_EVENT = "studentid:change";

export function isValidStudentId(value: string): boolean {
  return /^\d{17}$/.test(value);
}

/** Keeps digits only and caps the length. */
export function sanitizeStudentId(value: string): string {
  return value.replace(/\D/g, "").slice(0, STUDENT_ID_LENGTH);
}

export function getStudentId(): string | null {
  try {
    const value = window.localStorage.getItem(STUDENT_ID_KEY);
    return value !== null && isValidStudentId(value) ? value : null;
  } catch {
    return null;
  }
}

/** Stores a valid ID, or clears the stored one when given null or an invalid value. */
export function setStudentId(value: string | null): void {
  try {
    if (value !== null && isValidStudentId(value)) {
      window.localStorage.setItem(STUDENT_ID_KEY, value);
    } else {
      window.localStorage.removeItem(STUDENT_ID_KEY);
    }
  } catch {
    // Storage unavailable (private mode): nothing is persisted.
  }
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

export function subscribeStudentId(onChange: () => void): () => void {
  window.addEventListener(CHANGE_EVENT, onChange);
  window.addEventListener("storage", onChange);
  return () => {
    window.removeEventListener(CHANGE_EVENT, onChange);
    window.removeEventListener("storage", onChange);
  };
}
