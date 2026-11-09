# PHASES

Deadline: **11 Nov 2026, 23:59:59**. Commit after every checked group. Update the phase status here as work lands.

Status legend: `[ ]` todo · `[x]` done

## Phase 0 : Foundation
- [x] `.gitignore`, `.gitattributes` (`* text=auto eol=lf`), exclude assignment PDF
- [x] Solution + project scaffolding (`backend/`; `frontend/` still to do)
- [x] `docker-compose.yml` with db + healthcheck + volume (verified: stack starts, /health ok, tables created)
**Done when:** `docker compose up db` is healthy.

## Phase 1 : Backend core
- [x] EF model, migration, startup migrate with retry (EF's own migration lock)
- [x] Error model + global handler (validation helpers come with the endpoints)
- [x] `POST/GET /classes`, `GET /classes/{id}` with derived counts
- [x] `POST /classes/{id}/holds` (row lock algorithm)
- [x] `GET /holds/{id}` (effective status), `POST confirm`, `DELETE`, `GET /me/holds`
- [x] Expiry sweeper `BackgroundService`
- [x] CORS, env config (`HOLD_DURATION_SECONDS`), `/health`, `DATABASE_URL`/`PORT` support
**Done when:** all 8 endpoints match the contract by manual curl. (Verified 2026-11-09.)

## Phase 2 : Backend tests
- [x] Testcontainers fixture
- [x] Concurrency: 300 parallel distinct students / 120 seats → exactly 120×201, rest 409
- [x] Same student 20 parallel → 1×201
- [x] 20 parallel confirms → `confirmedSeats == 1`
- [x] Expiry with 1s hold, sweeper disabled → seat back, status EXPIRED
- [x] Cancel frees seat; re-apply works
- [x] Ownership 403; confirm expired 409; validation 4xx matrix
- [x] Two app instances, one DB, parallel load → cap holds (bonus)
**Done when:** `dotnet test` green, run 5× without flake.

## Phase 3 : Backend container
- [x] Multi-stage Dockerfile, port 8080
- [x] Compose wiring; verify persistence across `down`/`up`
**Done when:** API reachable at `localhost:8080` from compose. (Verified 2026-11-09: rebuilt image, full lifecycle via curl, data intact after `down`/`up`.)

## Phase 4 : Frontend foundation
- [x] Next.js 16 scaffold (App Router, TS strict, Tailwind 4, standalone output) — initialized 2026-11-09
- [x] Add Radix + lucide-react + TanStack Query
- [x] Design tokens from `DESIGN.md` into Tailwind config / CSS variables
- [x] API client, `ApiError`, error-copy map, runtime config via root layout (`window.__CONFIG__`)
- [x] App shell: top bar with student-ID field (validated, persisted)
**Done when:** shell renders; ID persists across reload. (2026-11-09: build + lint clean, SSR HTML and runtime `BACKEND_URL` verified, ID persistence logic verified in Node. Still to eyeball in a real browser: `npm --prefix frontend run dev`.)

## Phase 5 : Frontend features
- [x] Class list with 3s polling, Apply
- [x] Application detail: status, countdown, Pay, Cancel, live EXPIRED
- [x] My applications
- [x] Admin: create class
- [x] Empty / loading / error states everywhere
- [x] Error boundary, offline/server-down handling
**Done when:** U1–U5 verified manually in the browser. (2026-11-09: all features implemented; type check, lint and build clean, all routes return 200. Still to eyeball in a real browser against the running backend.)

## Phase 6 : Frontend container + integration
- [x] Next standalone Dockerfile (`node:22-alpine`), `BACKEND_URL` read at request time (frontend service added to compose; standalone server verified locally with a runtime `BACKEND_URL`; image build itself still to run with Docker)
- [x] Full `docker compose up` from a clean clone (2026-11-09: `docker compose up --build` verified on the working tree: 3 containers healthy, UI 200 with runtime `BACKEND_URL`, CORS preflight from :3000 ok, hold 20s, full class 409, confirm idempotent. Clean-clone run still pending.)
- [x] Two-tab and expiry walkthrough with `HOLD_DURATION_SECONDS=20` (confirmed done by the user 2026-11-09)

## Phase 7 : Frontend tests (bonus)
- [x] Countdown → EXPIRED disables Pay
- [x] Error mapping, ID validation, apply flow (MSW)
**Done when:** `npm --prefix frontend test` green. (2026-11-09: 33 tests in 5 files, stable over 3 runs; breaking `effectiveStatus` on purpose fails the unit and UI expiry tests.)

## Phase 8 : Docs & release
- [x] `../README.md` (run, URLs, tests), `../ASSUMPTIONS.md` (incl. multi-process explanation), `../AI_USAGE.md` (real mistakes only) (written 2026-11-09)
- [ ] Final clean-clone test on a fresh directory
- [ ] Public GitHub repo, push, submit link at https://q-rp.com/c/4CXx
- [ ] Verify nothing is pushed after the deadline


