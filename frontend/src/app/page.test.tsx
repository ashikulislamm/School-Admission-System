import { fireEvent, screen, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { setStudentId } from "@/lib/student-id";
import { API, server } from "@/test/server";
import { renderWithProviders } from "@/test/render";
import ClassesPage from "./page";

const push = vi.fn();
vi.mock("next/navigation", () => ({ useRouter: () => ({ push }), usePathname: () => "/" }));

const STUDENT = "12345678901234567";
const cls = (over = {}) => ({
  id: "c_1",
  name: "Class 6 Morning",
  totalSeats: 10,
  heldSeats: 2,
  confirmedSeats: 3,
  availableSeats: 5,
  ...over,
});

beforeEach(() => push.mockClear());

describe("class list and apply flow", () => {
  it("shows seat counts and keeps Apply disabled until a student ID is entered", async () => {
    server.use(http.get(`${API}/classes`, () => HttpResponse.json([cls()])));
    renderWithProviders(<ClassesPage />);

    expect(await screen.findByText("Class 6 Morning")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Apply" })).toBeDisabled();
    expect(screen.getByText(/Enter a 17-digit student ID/)).toBeInTheDocument();
  });

  it("applies with X-Student-Id and opens the new application", async () => {
    let header: string | null = null;
    server.use(
      http.get(`${API}/classes`, () => HttpResponse.json([cls()])),
      http.post(`${API}/classes/c_1/holds`, ({ request }) => {
        header = request.headers.get("X-Student-Id");
        return HttpResponse.json(
          { holdId: "h_1", classId: "c_1", studentId: STUDENT, status: "HELD", expiresAt: "2026-11-09T10:10:00Z" },
          { status: 201 },
        );
      }),
    );
    setStudentId(STUDENT);
    renderWithProviders(<ClassesPage />);

    const apply = await screen.findByRole("button", { name: "Apply" });
    await waitFor(() => expect(apply).toBeEnabled());
    fireEvent.click(apply);

    await waitFor(() => expect(push).toHaveBeenCalledWith("/applications/h_1"));
    expect(header).toBe(STUDENT);
  });

  it("explains a full class in plain language and does not navigate", async () => {
    server.use(
      http.get(`${API}/classes`, () => HttpResponse.json([cls()])),
      http.post(`${API}/classes/c_1/holds`, () =>
        HttpResponse.json({ code: "CLASS_FULL", message: "raw" }, { status: 409 }),
      ),
    );
    setStudentId(STUDENT);
    renderWithProviders(<ClassesPage />);

    const apply = await screen.findByRole("button", { name: "Apply" });
    await waitFor(() => expect(apply).toBeEnabled());
    fireEvent.click(apply);

    expect(await screen.findByRole("alert")).toHaveTextContent("All seats in this class are taken.");
    expect(push).not.toHaveBeenCalled();
  });

  it("disables Apply for a full class", async () => {
    server.use(http.get(`${API}/classes`, () => HttpResponse.json([cls({ availableSeats: 0, heldSeats: 7 })])));
    setStudentId(STUDENT);
    renderWithProviders(<ClassesPage />);

    const apply = await screen.findByRole("button", { name: "Apply" });
    expect(apply).toBeDisabled();
  });

  it("shows a readable error when the list cannot be loaded", async () => {
    server.use(
      http.get(`${API}/classes`, () => HttpResponse.json({ code: "WHATEVER", message: "stack trace" }, { status: 500 })),
    );
    renderWithProviders(<ClassesPage />);

    expect(await screen.findByRole("alert")).toHaveTextContent("Something went wrong. Try again.");
  });
});
