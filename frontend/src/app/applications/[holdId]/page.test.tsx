import { act, fireEvent, screen, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { Suspense } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { setStudentId } from "@/lib/student-id";
import { API, server } from "@/test/server";
import { renderWithProviders } from "@/test/render";
import ApplicationDetailPage from "./page";

const STUDENT = "12345678901234567";
const NOW = Date.parse("2026-11-09T10:00:00Z");

const classHandler = http.get(`${API}/classes/c_1`, () =>
  HttpResponse.json({ id: "c_1", name: "Class 6 Morning", totalSeats: 10, heldSeats: 1, confirmedSeats: 0, availableSeats: 9 }),
);

function holdHandler(status: string, expiresAt: string) {
  return http.get(`${API}/holds/h_1`, () => HttpResponse.json({ holdId: "h_1", classId: "c_1", status, expiresAt }));
}

// React reads a promise marked as fulfilled synchronously, so `use(params)` never suspends here
// (suspense retries are scheduled with the clock this file fakes).
function resolvedParams() {
  const value = { holdId: "h_1" };
  return Object.assign(Promise.resolve(value), { status: "fulfilled" as const, value });
}

function renderDetail() {
  return renderWithProviders(
    <Suspense fallback={null}>
      <ApplicationDetailPage params={resolvedParams()} searchParams={Promise.resolve({})} />
    </Suspense>,
  );
}

// Fake only the clock and the interval; timeouts stay real so MSW and waitFor keep working.
beforeEach(() => {
  vi.useFakeTimers({ toFake: ["Date", "setInterval", "clearInterval"], now: NOW });
  setStudentId(STUDENT);
});
afterEach(() => vi.useRealTimers());

describe("application detail", () => {
  it("counts down, then shows EXPIRED without a reload and disables Pay", async () => {
    server.use(classHandler, holdHandler("HELD", new Date(NOW + 5000).toISOString()));
    renderDetail();

    const pay = await screen.findByRole("button", { name: "Pay admission fee" });
    await waitFor(() => expect(pay).toBeEnabled());
    expect(screen.getByText("HELD")).toBeInTheDocument();
    expect(screen.getByText("00:05")).toBeInTheDocument();

    await act(async () => {
      vi.advanceTimersByTime(6000);
    });

    expect(screen.getByText("EXPIRED")).toBeInTheDocument();
    expect(screen.queryByText("HELD")).not.toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Pay admission fee" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Cancel application" })).toBeDisabled();
  });

  it("turns the countdown to the warning color under 60 seconds", async () => {
    server.use(classHandler, holdHandler("HELD", new Date(NOW + 30000).toISOString()));
    renderDetail();

    const timer = await screen.findByText("00:30");
    expect(timer.className).toContain("text-warning");
  });

  it("confirms payment and hides Pay", async () => {
    let status = "HELD";
    server.use(
      classHandler,
      http.get(`${API}/holds/h_1`, () =>
        HttpResponse.json({ holdId: "h_1", classId: "c_1", status, expiresAt: new Date(NOW + 600000).toISOString() }),
      ),
      http.post(`${API}/holds/h_1/confirm`, ({ request }) => {
        expect(request.headers.get("X-Student-Id")).toBe(STUDENT);
        status = "CONFIRMED";
        return HttpResponse.json({ holdId: "h_1", status: "CONFIRMED" });
      }),
    );
    renderDetail();

    const pay = await screen.findByRole("button", { name: "Pay admission fee" });
    await waitFor(() => expect(pay).toBeEnabled());
    fireEvent.click(pay);

    expect(await screen.findByText("CONFIRMED")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Pay admission fee" })).not.toBeInTheDocument();
  });

  it("shows the expired verdict from the server in plain language when payment is refused", async () => {
    server.use(
      classHandler,
      holdHandler("HELD", new Date(NOW + 600000).toISOString()),
      http.post(`${API}/holds/h_1/confirm`, () =>
        HttpResponse.json({ code: "HOLD_EXPIRED", message: "raw" }, { status: 409 }),
      ),
    );
    renderDetail();

    const pay = await screen.findByRole("button", { name: "Pay admission fee" });
    await waitFor(() => expect(pay).toBeEnabled());
    fireEvent.click(pay);

    expect(await screen.findByRole("alert")).toHaveTextContent("This application has expired.");
  });

  it("asks for a student ID instead of enabling actions without one", async () => {
    window.localStorage.clear();
    server.use(classHandler, holdHandler("HELD", new Date(NOW + 600000).toISOString()));
    renderDetail();

    expect(await screen.findByText("Enter a student ID to pay or cancel.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Pay admission fee" })).toBeDisabled();
  });
});
