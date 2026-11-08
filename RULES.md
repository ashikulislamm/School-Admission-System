# RULES

Non-negotiable. If a rule must be broken, record why in `ASSUMPTIONS.md`.

## 1. Invariants (never violate)
1. `held + confirmed ≤ totalSeats`, always. Enforced inside a DB row lock, never by app-side checks alone.
2. Seat counts are derived from `holds`; no stored counters.
3. Expiry decisions use the DB clock (`now()`); never `DateTime.UtcNow` for business logic.
4. The backend holds no state in memory that affects correctness.
5. No input may produce a 500. Validate, then return 4xx.
6. The API contract in `PRD.md` §6 is frozen. Add fields freely; never remove or rename.
7. Confirm and cancel are idempotent.

## 2. Backend
- Transactions are short; do no I/O outside the DB inside them.
- Lock order: class row, then hold row. Never reverse it.
- All errors are `{ code, message }`; codes are `UPPER_SNAKE` and stable (`CLASS_FULL`, `ALREADY_APPLIED`, `HOLD_EXPIRED`, `HOLD_NOT_ACTIVE`, `NOT_OWNER`, `NOT_FOUND`, `INVALID_INPUT`, `INVALID_STUDENT_ID`).
- Messages are user-readable sentences, no stack traces or SQL.
- Validate: `X-Student-Id` = `^\d{17}$`; class name trimmed, 1–100 chars; `totalSeats` integer 1–100000; reject bad JSON.
- Timestamps are UTC, serialized ISO-8601 with `Z`.
- Config only via env vars; no secrets in source. Compose defaults are dev-only values.
- Nullable reference types on; warnings as errors in CI build.
- Public methods that touch the DB are async and take a `CancellationToken`.

## 3. Frontend
- TypeScript strict. No `any`; no `@ts-ignore` without a comment.
- Server state via TanStack Query only; no duplicated copies in `useState`.
- All user-facing strings for errors come from `lib/errors.ts`; never render raw server/JS messages.
- UI is never the authority: disable controls for UX, but always handle the server's 409/403.
- No hard-coded backend URL; read `window.__CONFIG__.BACKEND_URL`, fallback `http://localhost:8080`.
- Every async view has loading, empty, and error states.
- Follow `DESIGN.md` exactly. No new colors, radii, shadows or fonts outside the tokens.
- Accessible: semantic HTML, labelled inputs, visible focus, keyboard operable, `aria-live` for errors and countdown changes.
- No `console.log` in committed code.

## 4. Git
- The user makes all commits; AI agents only suggest messages and never run `git commit`/`push`.
- Commit small and often, Conventional Commits: `feat(api): …`, `fix(ui): …`, `test: …`, `docs: …`, `chore: …`.
- One logical change per commit; the build must pass at every commit.
- Default branch `main` (rename from `master`). No force-push to `main`.
- Never commit: the assignment PDF, `.env` with real secrets, `bin/ obj/ node_modules/ dist/`.
- Commit trailer for AI-assisted commits as configured by the tooling.

## 5. Testing
- A change to concurrency, confirm, cancel, or expiry logic requires a test.
- Concurrency tests use real Postgres (Testcontainers); never mock the DB for them.
- No sleeps longer than needed; prefer `HOLD_DURATION_SECONDS=1` fixtures.
- Tests must be deterministic; fix flakes, never retry-mask them.

## 6. Docker
- `docker compose up` on a clean clone must work with only Docker installed.
- Named volume for Postgres; never use anonymous volumes.
- Shell scripts use LF endings.
- Images pinned to major tags (`postgres:16-alpine`, `dotnet/aspnet:8.0`, `node:20-alpine`, `nginx:1.27-alpine`).

## 7. Documentation
- Update the relevant `MEMORY.md` after any non-obvious decision, gotcha, or fix.
- Update `PHASES.md` checkboxes as work completes.
- `AI_USAGE.md` records real mistakes and how they were caught; never fabricate.
- `ASSUMPTIONS.md` stays short: assumption → reason.

## 8. Definition of done (any task)
Builds · tests pass · matches `DESIGN.md` (UI) · errors handled · docs/memory updated · committed.
