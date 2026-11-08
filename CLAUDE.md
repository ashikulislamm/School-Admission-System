# CLAUDE.md

School Admission System — take-home for Onnorokom Projukti Limited. First-come-first-served seat holds with payment confirmation. Deadline **11 Nov 2026 23:59:59**.

## Stack
.NET 8 minimal API + EF Core + PostgreSQL 16 · React 18 + Vite + TypeScript + Tailwind + TanStack Query · Docker Compose.

## Docs map
@PRD.md — requirements and frozen API contract
@ARCHITECTURE.md — design, schema, concurrency algorithm
@RULES.md — non-negotiable rules
@DESIGN.md — UI design system (read before any UI work)
@PHASES.md — roadmap and status
@AGENT.md — agent workflow
@MEMORY.md — project memory (also `backend/MEMORY.md`, `frontend/MEMORY.md`)

## Commands
```
docker compose up --build        # whole app; UI http://localhost:3000, API http://localhost:8080
docker compose down              # keeps data (named volume); add -v to wipe
dotnet test backend              # needs Docker running (Testcontainers)
dotnet run --project backend/src/Admission.Api
npm --prefix frontend run dev    # Vite dev server
npm --prefix frontend test       # Vitest
```

## Env vars
Backend: `ConnectionStrings__Default`, `HOLD_DURATION_SECONDS` (default 600), `CORS_ORIGINS`.
Frontend (runtime): `BACKEND_URL` → `window.__CONFIG__`.

## Critical rules (full list in RULES.md)
- `held + confirmed ≤ totalSeats` enforced under a class-row `FOR UPDATE` lock; counts derived, never stored.
- Expiry uses DB `now()`; lazy evaluation is the source of truth, sweeper is hygiene.
- API contract is frozen; only add fields. Invalid input is always 4xx.
- UI follows DESIGN.md: no gradients, no badges, no filler text, one accent color.
- Windows host: keep LF line endings for scripts.

## Workflow
Follow `PHASES.md` in order. Update `PHASES.md` and the relevant `MEMORY.md` as you go. **Claude never commits or pushes; the user makes every commit.** Suggest a Conventional Commit message after each unit of work. Never commit the assignment PDF.
