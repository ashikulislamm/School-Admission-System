# PHASES

Deadline: **11 Nov 2026, 23:59:59**. Commit after every checked group. Update the phase status here as work lands.

Status legend: `[ ]` todo Â· `[x]` done

## Phase 0 â€” Foundation
- [x] `.gitignore`, `.gitattributes` (`* text=auto eol=lf`), exclude assignment PDF
- [x] Solution + project scaffolding (`backend/`; `frontend/` still to do)
- [x] `docker-compose.yml` with db + healthcheck + volume (verified: stack starts, /health ok, tables created)
**Done when:** `docker compose up db` is healthy.

## Phase 1 â€” Backend core
- [x] EF model, migration, startup migrate with retry (EF's own migration lock)
- [x] Error model + global handler (validation helpers come with the endpoints)
- [x] `POST/GET /classes`, `GET /classes/{id}` with derived counts
- [x] `POST /classes/{id}/holds` (row lock algorithm)
- [x] `GET /holds/{id}` (effective status), `POST confirm`, `DELETE`, `GET /me/holds`
- [x] Expiry sweeper `BackgroundService`
- [x] CORS, env config (`HOLD_DURATION_SECONDS`), `/health`, `DATABASE_URL`/`PORT` support
**Done when:** all 8 endpoints match the contract by manual curl. (Verified 2026-11-09.)

## Phase 2 â€” Backend tests
- [ ] Testcontainers fixture
- [ ] Concurrency: 300 parallel distinct students / 120 seats â†’ exactly 120Ã—201, rest 409
- [ ] Same student 20 parallel â†’ 1Ã—201
- [ ] 20 parallel confirms â†’ `confirmedSeats == 1`
- [ ] Expiry with 1s hold, sweeper disabled â†’ seat back, status EXPIRED
- [ ] Cancel frees seat; re-apply works
- [ ] Ownership 403; confirm expired 409; validation 4xx matrix
- [ ] Two app instances, one DB, parallel load â†’ cap holds (bonus)
**Done when:** `dotnet test` green, run 5Ã— without flake.

## Phase 3 â€” Backend container
- [ ] Multi-stage Dockerfile, port 8080
- [ ] Compose wiring; verify persistence across `down`/`up`
**Done when:** API reachable at `localhost:8080` from compose.

## Phase 4 â€” Frontend foundation
- [ ] Vite + TS + Tailwind + Radix + TanStack Query + Router
- [ ] Design tokens from `DESIGN.md` into Tailwind config / CSS variables
- [ ] API client, `ApiError`, error-copy map, runtime `config.js`
- [ ] App shell: top bar with student-ID field (validated, persisted)
**Done when:** shell renders; ID persists across reload.

## Phase 5 â€” Frontend features
- [ ] Class list with 3s polling, Apply
- [ ] Application detail: status, countdown, Pay, Cancel, live EXPIRED
- [ ] My applications
- [ ] Admin: create class
- [ ] Empty / loading / error states everywhere
- [ ] Error boundary, offline/server-down handling
**Done when:** U1â€“U5 verified manually in the browser.

## Phase 6 â€” Frontend container + integration
- [ ] nginx Dockerfile, SPA fallback, entrypoint config.js
- [ ] Full `docker compose up` from a clean clone
- [ ] Two-tab and expiry walkthrough with `HOLD_DURATION_SECONDS=20`

## Phase 7 â€” Frontend tests (bonus)
- [ ] Countdown â†’ EXPIRED disables Pay
- [ ] Error mapping, ID validation, apply flow (MSW)

## Phase 8 â€” Docs & release
- [ ] `README.md` (run, URLs, tests), `ASSUMPTIONS.md` (incl. multi-process explanation), `AI_USAGE.md` (real mistakes only)
- [ ] Final clean-clone test on a fresh directory
- [ ] Public GitHub repo, push, submit link at https://q-rp.com/c/4CXx
- [ ] Verify nothing is pushed after the deadline

## Schedule
| Day | Phases |
|---|---|
| 8 Nov | 0â€“3 |
| 9 Nov | 4â€“6 |
| 10 Nov | 7â€“8, buffer |
| 11 Nov | Submit early; no new features |
