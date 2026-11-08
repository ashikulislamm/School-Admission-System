# PHASES

Deadline: **11 Nov 2026, 23:59:59**. Commit after every checked group. Update the phase status here as work lands.

Status legend: `[ ]` todo · `[x]` done

## Phase 0 — Foundation
- [ ] `.gitignore`, `.gitattributes` (`* text=auto eol=lf`), exclude assignment PDF
- [ ] Solution + project scaffolding (`backend/`, `frontend/`)
- [ ] `docker-compose.yml` with db + healthcheck + volume
**Done when:** `docker compose up db` is healthy.

## Phase 1 — Backend core
- [ ] EF model, migration, startup migrate with advisory lock + retry
- [ ] Error model + global handler + validation helpers
- [ ] `POST/GET /classes`, `GET /classes/{id}` with derived counts
- [ ] `POST /classes/{id}/holds` (row lock algorithm)
- [ ] `GET /holds/{id}` (effective status), `POST confirm`, `DELETE`, `GET /me/holds`
- [ ] Expiry sweeper `BackgroundService`
- [ ] CORS, env config (`HOLD_DURATION_SECONDS`)
**Done when:** all 8 endpoints match the contract by manual curl.

## Phase 2 — Backend tests
- [ ] Testcontainers fixture
- [ ] Concurrency: 300 parallel distinct students / 120 seats → exactly 120×201, rest 409
- [ ] Same student 20 parallel → 1×201
- [ ] 20 parallel confirms → `confirmedSeats == 1`
- [ ] Expiry with 1s hold, sweeper disabled → seat back, status EXPIRED
- [ ] Cancel frees seat; re-apply works
- [ ] Ownership 403; confirm expired 409; validation 4xx matrix
- [ ] Two app instances, one DB, parallel load → cap holds (bonus)
**Done when:** `dotnet test` green, run 5× without flake.

## Phase 3 — Backend container
- [ ] Multi-stage Dockerfile, port 8080
- [ ] Compose wiring; verify persistence across `down`/`up`
**Done when:** API reachable at `localhost:8080` from compose.

## Phase 4 — Frontend foundation
- [ ] Vite + TS + Tailwind + Radix + TanStack Query + Router
- [ ] Design tokens from `DESIGN.md` into Tailwind config / CSS variables
- [ ] API client, `ApiError`, error-copy map, runtime `config.js`
- [ ] App shell: top bar with student-ID field (validated, persisted)
**Done when:** shell renders; ID persists across reload.

## Phase 5 — Frontend features
- [ ] Class list with 3s polling, Apply
- [ ] Application detail: status, countdown, Pay, Cancel, live EXPIRED
- [ ] My applications
- [ ] Admin: create class
- [ ] Empty / loading / error states everywhere
- [ ] Error boundary, offline/server-down handling
**Done when:** U1–U5 verified manually in the browser.

## Phase 6 — Frontend container + integration
- [ ] nginx Dockerfile, SPA fallback, entrypoint config.js
- [ ] Full `docker compose up` from a clean clone
- [ ] Two-tab and expiry walkthrough with `HOLD_DURATION_SECONDS=20`

## Phase 7 — Frontend tests (bonus)
- [ ] Countdown → EXPIRED disables Pay
- [ ] Error mapping, ID validation, apply flow (MSW)

## Phase 8 — Docs & release
- [ ] `README.md` (run, URLs, tests), `ASSUMPTIONS.md` (incl. multi-process explanation), `AI_USAGE.md` (real mistakes only)
- [ ] Final clean-clone test on a fresh directory
- [ ] Public GitHub repo, push, submit link at https://q-rp.com/c/4CXx
- [ ] Verify nothing is pushed after the deadline

## Schedule
| Day | Phases |
|---|---|
| 8 Nov | 0–3 |
| 9 Nov | 4–6 |
| 10 Nov | 7–8, buffer |
| 11 Nov | Submit early; no new features |
