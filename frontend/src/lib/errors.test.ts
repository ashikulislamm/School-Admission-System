import { describe, expect, it } from "vitest";
import { ApiError } from "@/api/client";
import { isUnreachable, messageFor, UNKNOWN_COPY, UNREACHABLE_COPY } from "./errors";

describe("messageFor", () => {
  it.each([
    ["CLASS_FULL", "All seats in this class are taken."],
    ["HOLD_EXPIRED", "This application has expired."],
    ["ALREADY_APPLIED", "You already have an application for this class."],
    ["INVALID_STUDENT_ID", "Enter a 17-digit birth registration number."],
    ["INVALID_INPUT", "Check the highlighted fields."],
  ])("maps %s to plain language", (code, copy) => {
    expect(messageFor(new ApiError(409, code, "raw server text"))).toBe(copy);
  });

  it("never shows raw server or JS messages", () => {
    expect(messageFor(new ApiError(500, "SOMETHING_NEW", "NullReferenceException at line 4"))).toBe(UNKNOWN_COPY);
    expect(messageFor(new Error("boom"))).toBe(UNKNOWN_COPY);
  });

  it("uses the server-down copy for network failures", () => {
    expect(messageFor(new ApiError(0, "NETWORK_ERROR", "x"))).toBe(UNREACHABLE_COPY);
    expect(isUnreachable(new ApiError(0, "NETWORK_ERROR", "x"))).toBe(true);
    expect(isUnreachable(new ApiError(503, "SERVICE_UNAVAILABLE", "x"))).toBe(true);
    expect(isUnreachable(new ApiError(409, "CLASS_FULL", "x"))).toBe(false);
  });
});
