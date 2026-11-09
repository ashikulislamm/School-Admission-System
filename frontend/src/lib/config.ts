declare global {
  interface Window {
    __CONFIG__?: { BACKEND_URL?: string };
  }
}

export const DEFAULT_BACKEND_URL = "http://localhost:8080";

/** Backend base URL from the runtime config injected by the root layout. Never build-time. */
export function getBackendUrl(): string {
  const configured = typeof window === "undefined" ? undefined : window.__CONFIG__?.BACKEND_URL;
  const url = configured && configured.trim() !== "" ? configured.trim() : DEFAULT_BACKEND_URL;
  return url.replace(/\/+$/, "");
}
