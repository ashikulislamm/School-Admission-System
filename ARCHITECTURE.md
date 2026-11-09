# ARCHITECTURE

## 1. Overview
```
Browser ──(JSON, X-Student-Id)──► Backend :8080 ──► PostgreSQL (named volume)
 Next.js server (standalone) :3000      ASP.NET Core 10       all state lives here
```
The backend is stateless: no in-memory locks, caches or counters. That is what makes multi-process safety work.

## 2. Repository layout
```
/
  backend/
    src/Admission.Api/        Program.cs, Endpoints/, Services/, Data/, Models/, Errors/
    tests/Admission.Tests/    xUnit + Testcontainers
    Dockerfile  MEMORY.md
  frontend/
    src/ app/ (routes) api/ components/ features/ hooks/ lib/
    next.config.ts  Dockerfile  MEMORY.md
  docker-compose.yml
  *.md (PRD, ARCHITECTURE, PHASES, RULES, DESIGN, AGENT, CLAUDE, MEMORY, README, ASSUMPTIONS, AI_USAGE)
  .gitattributes  (eol=lf)
```

## 3. Backend
- .NET 10 minimal API, EF Core 10 + Npgsql. Raw SQL only for `FOR UPDATE` and the sweeper.
- Layers: `Endpoints` (HTTP mapping, validation) → `Services` (transactions, rules) → `Data` (DbContext).
- Global exception handler → ProblemDetails with stable `code` + `message`.
- CORS: allow the frontend origin (configurable); expose the `Date` response header (the UI derives its clock offset from it).
- Config: `ConnectionStrings__Default`, `HOLD_DURATION_SECONDS`, `CORS_ORIGINS`, `ASPNETCORE_URLS=http://+:8080`.
- Startup: apply EF migrations (EF Core takes its own database lock, so two processes are safe), with retry until DB is ready.

## 4. Data model
```
classes(id text PK, name text NOT NULL, total_seats int NOT NULL CHECK (total_seats > 0), created_at timestamptz)
holds(id text PK, class_id text FK, student_id char(17) NOT NULL,
      status text CHECK IN ('HELD','CONFIRMED','EXPIRED','CANCELLED'),
      created_at timestamptz, expires_at timestamptz NOT NULL, confirmed_at timestamptz NULL)
UNIQUE INDEX ux_holds_active ON holds(class_id, student_id) WHERE status IN ('HELD','CONFIRMED')
INDEX ix_holds_class_status ON holds(class_id, status, expires_at)
INDEX ix_holds_student ON holds(student_id, created_at DESC)
```
IDs: `c_` / `h_` + 12 random url-safe chars. Seat counts are **derived** from `holds`, never stored.

## 5. Concurrency design (the core)
**Apply** — one short transaction, Read Committed:
1. `SELECT 1 FROM classes WHERE id=@c FOR UPDATE` (serializes appliers per class, across processes).
2. Expire this student's lapsed `HELD` rows in the class (`UPDATE ... SET status='EXPIRED' WHERE status='HELD' AND expires_at <= now()`).
3. Student already active here → `409 ALREADY_APPLIED`.
4. `active = count(CONFIRMED) + count(HELD AND expires_at > now())`; `active >= total` → `409 CLASS_FULL`.
5. Insert HELD with `expires_at = now() + HOLD_DURATION_SECONDS`.

**Confirm** — `SELECT ... FROM holds WHERE id=@h FOR UPDATE`:
not owner → 403 · CONFIRMED → 200 (no-op) · HELD and `expires_at > now()` → CONFIRMED, 200 · else 409 (`HOLD_EXPIRED` / `HOLD_NOT_ACTIVE`).

**Cancel** — lock row; owner only; HELD → CANCELLED (204); CANCELLED → 204 (idempotent); EXPIRED → 204 no-op; CONFIRMED → 409.

**Clock** — always the DB clock. App clocks are never used for expiry decisions. The code uses `clock_timestamp()` wherever this document says `now()`, because `now()` is frozen at transaction start and goes stale after waiting on a lock.

**Expiry — two layers**
1. *Lazy (correctness):* "active" is evaluated against `expires_at > now()` in every count and read. Effective status of a lapsed `HELD` row is reported as `EXPIRED` even before it is persisted.
2. *Sweeper (hygiene):* `BackgroundService`, ~1s, idempotent `UPDATE`. Safe with N processes.

**Safety nets:** partial unique index (rule B3), `CHECK total_seats > 0`, unique-violation `23505` mapped to 409.

**Why this satisfies the bonus:** every decision happens inside a Postgres row lock on the class; no process-local state is consulted.

## 6. Frontend
- Next.js 16 (App Router, `output: "standalone"`), React 19, TypeScript, Tailwind CSS 4, Radix primitives, lucide-react, TanStack Query. Next provides routing, build and the Node server only: every page is a client component (`"use client"`), so the UI behaves as a SPA. `cacheComponents` is off (hidden-route state retention is unwanted).
- Before writing Next code, read the bundled docs in `frontend/node_modules/next/dist/docs/` (this Next version differs from older releases).
- Server state only via TanStack Query. Client state: student ID (`localStorage`), nothing else global.
- Polling: classes 3s, open hold 3s, my holds 5s; pause on hidden tab.
- Countdown: derived from server `expiresAt` + measured clock offset (from response `Date` header). At zero → render EXPIRED locally, disable Pay, invalidate query.
- Errors: `api/client.ts` throws typed `ApiError{status,code,message}`; `lib/errors.ts` maps code → user copy. Network failure → "server unreachable". Root error boundary.
- Runtime config: the root layout is a server component that calls `await connection()` and renders `<script>window.__CONFIG__ = { BACKEND_URL }</script>` from `process.env.BACKEND_URL`; no rebuild needed. `NEXT_PUBLIC_*` is never used (inlined at build).

### Routes
`/` classes · `/applications` my applications · `/applications/:holdId` detail · `/admin` create class.

## 7. Docker
| Service | Image | Port | Notes |
|---|---|---|---|
| db | postgres:16-alpine | – | volume `pgdata`, `pg_isready` healthcheck |
| backend | multi-stage dotnet sdk→aspnet | 8080 | `depends_on: db (healthy)` |
| frontend | node build → `node:22-alpine` standalone | 3000 | `node server.js`; `BACKEND_URL` read at request time |
Optional `docker-compose.multi.yml` adds `backend2` on 8081 for the multi-process demo.

## 8. Testing strategy
- Backend: xUnit + `WebApplicationFactory` + Testcontainers Postgres. Concurrency, idempotency, expiry (1s hold, sweeper off), validation, ownership, two-instance test.
- Frontend: Vitest + React Testing Library + MSW. Countdown/expiry, error mapping, ID validation, apply flow.

## 9. Key decisions (ADR summary)
| Decision | Why |
|---|---|
| Derived counts, not counters | One source of truth; no drift |
| Row lock on class, Read Committed | Simple, provably correct, multi-process safe |
| Lazy expiry + sweeper | Seat returns exactly at expiry regardless of sweeper timing |
| DB clock | No cross-process skew |
| Next.js standalone, runtime env in root layout | Env-configurable backend URL without rebuild; no nginx or entrypoint script |
