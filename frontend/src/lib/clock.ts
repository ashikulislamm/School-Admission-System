// Offset between the server clock and this browser's clock, measured from response `Date` headers.
// The header has 1s resolution (truncated), so half a second is added to centre the error.
let offsetMs = 0;

export function updateClockOffset(dateHeader: string | null, receivedAtMs: number = Date.now()): void {
  if (!dateHeader) return;
  const serverMs = Date.parse(dateHeader);
  if (Number.isNaN(serverMs)) return;
  offsetMs = serverMs + 500 - receivedAtMs;
}

/** Best estimate of the server's current time in epoch milliseconds. */
export function serverNow(): number {
  return Date.now() + offsetMs;
}

export function getClockOffset(): number {
  return offsetMs;
}
