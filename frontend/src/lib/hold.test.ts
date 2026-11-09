import { describe, expect, it } from "vitest";
import { effectiveStatus, formatCountdown, remainingSeconds } from "./hold";

const expiresAt = "2026-11-09T10:00:00.000Z";
const at = Date.parse(expiresAt);

describe("effectiveStatus", () => {
  it("keeps HELD before expiry", () => {
    expect(effectiveStatus("HELD", expiresAt, at - 1000)).toBe("HELD");
  });
  it("reports a lapsed HELD as EXPIRED, including exactly at expiry", () => {
    expect(effectiveStatus("HELD", expiresAt, at)).toBe("EXPIRED");
    expect(effectiveStatus("HELD", expiresAt, at + 5000)).toBe("EXPIRED");
  });
  it("never changes terminal states", () => {
    expect(effectiveStatus("CONFIRMED", expiresAt, at + 5000)).toBe("CONFIRMED");
    expect(effectiveStatus("CANCELLED", expiresAt, at + 5000)).toBe("CANCELLED");
  });
  it("treats an unmeasured clock (0) as not expired", () => {
    expect(effectiveStatus("HELD", expiresAt, 0)).toBe("HELD");
  });
});

describe("countdown", () => {
  it("rounds up and never goes negative", () => {
    expect(remainingSeconds(expiresAt, at - 1500)).toBe(2);
    expect(remainingSeconds(expiresAt, at + 9000)).toBe(0);
  });
  it("formats mm:ss", () => {
    expect(formatCountdown(0)).toBe("00:00");
    expect(formatCountdown(65)).toBe("01:05");
    expect(formatCountdown(600)).toBe("10:00");
  });
});
