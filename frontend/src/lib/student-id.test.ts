import { afterEach, describe, expect, it } from "vitest";
import { getStudentId, isValidStudentId, sanitizeStudentId, setStudentId } from "./student-id";

afterEach(() => window.localStorage.clear());

describe("student ID validation", () => {
  it("accepts exactly 17 digits", () => {
    expect(isValidStudentId("12345678901234567")).toBe(true);
  });
  it.each(["", "1234567890123456", "123456789012345678", "1234567890123456a", "1234 678901234567"])(
    "rejects %j",
    (value) => expect(isValidStudentId(value)).toBe(false),
  );
  it("keeps digits only and caps at 17", () => {
    expect(sanitizeStudentId("12ab34-5")).toBe("12345");
    expect(sanitizeStudentId("123456789012345678999")).toHaveLength(17);
  });
});

describe("student ID storage", () => {
  it("persists only a valid ID", () => {
    setStudentId("12345678901234567");
    expect(getStudentId()).toBe("12345678901234567");
  });
  it("clears the stored ID when given a partial or null value", () => {
    setStudentId("12345678901234567");
    setStudentId("123");
    expect(getStudentId()).toBeNull();
    setStudentId("12345678901234567");
    setStudentId(null);
    expect(getStudentId()).toBeNull();
  });
  it("ignores a tampered stored value", () => {
    window.localStorage.setItem("studentId", "not-an-id");
    expect(getStudentId()).toBeNull();
  });
});
