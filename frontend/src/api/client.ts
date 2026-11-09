import { updateClockOffset } from "@/lib/clock";
import { getBackendUrl } from "@/lib/config";

/** Every failed request becomes one of these. `status` is 0 when the server was never reached. */
export class ApiError extends Error {
  readonly status: number;
  readonly code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
  }
}

export interface RequestOptions {
  method?: "GET" | "POST" | "DELETE";
  body?: unknown;
  /** Sent as X-Student-Id when present. */
  studentId?: string | null;
  signal?: AbortSignal;
}

interface ErrorBody {
  code?: unknown;
  message?: unknown;
}

async function toApiError(res: Response): Promise<ApiError> {
  let code = "UNKNOWN";
  let message = "Request failed.";
  try {
    const body = (await res.json()) as ErrorBody;
    if (typeof body.code === "string") code = body.code;
    if (typeof body.message === "string") message = body.message;
  } catch {
    // Non-JSON error body (for example a proxy page): keep the generic values.
  }
  if (code === "UNKNOWN" && res.status === 503) code = "SERVICE_UNAVAILABLE";
  return new ApiError(res.status, code, message);
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = {};
  if (options.studentId) headers["X-Student-Id"] = options.studentId;
  if (options.body !== undefined) headers["Content-Type"] = "application/json";

  let res: Response;
  try {
    res = await fetch(`${getBackendUrl()}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
      signal: options.signal,
      cache: "no-store",
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError") throw error;
    throw new ApiError(0, "NETWORK_ERROR", "The server could not be reached.");
  }

  updateClockOffset(res.headers.get("Date"));

  if (!res.ok) throw await toApiError(res);
  if (res.status === 204) return undefined as T;

  try {
    return (await res.json()) as T;
  } catch {
    throw new ApiError(res.status, "BAD_RESPONSE", "The server sent an unreadable response.");
  }
}
