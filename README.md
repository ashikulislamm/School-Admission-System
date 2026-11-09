# School Admission System

First-come-first-served online admission for a private school. Each shift of each class is a separate class with a fixed number of seats. Applying **holds** a seat for a limited time; paying the fee **confirms** it; otherwise the seat returns automatically.

Backend: .NET 10 minimal API, EF Core, PostgreSQL 16. Frontend: Next.js 16 (React 19, TypeScript, Tailwind, TanStack Query). Everything runs with Docker.

## Run with Docker

Only Docker (with Compose) is needed.

```
docker compose up --build
```

| What | URL |
|---|---|
| Frontend (open this) | http://localhost:3000 |
| Backend API | http://localhost:8080 (health check: `/health`) |
| PostgreSQL (host access, optional) | `localhost:5433` (user, password and database: `admission`) |

Data lives in the named volume `pgdata`, so it survives `docker compose down` followed by `docker compose up`. Add `-v` to `down` only if you want to wipe it.

### Configuration (environment variables, all optional)

| Variable | Default | Meaning |
|---|---|---|
| `HOLD_DURATION_SECONDS` | `600` | How long an application holds a seat |
| `BACKEND_URL` | `http://localhost:8080` | Backend URL the frontend gives to the browser (read when the container starts, no rebuild) |
| `CORS_ORIGINS` | `http://localhost:3000` | Origins the backend accepts |
| `ConnectionStrings__Default` | set by compose | Database connection string |
| `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `DB_HOST_PORT` | `admission` ×3, `5433` | Database settings (development defaults) |

For a quick look at expiry, use a short hold (PowerShell):

```
$env:HOLD_DURATION_SECONDS = "20"
docker compose up --build
```

## Using the app

1. **Admin** tab: create a class with a name and a number of seats.
2. Type a 17-digit birth registration number in the top bar. It is remembered in the browser.
3. **Classes** tab: seat counts refresh every 3 seconds. Click **Apply** to hold a seat.
4. On the application page watch the countdown. **Pay admission fee** confirms (payment is simulated), **Cancel application** releases the seat. At zero the status changes to `EXPIRED` by itself and Pay is disabled.
5. **Applications** tab: every application of the entered student, newest first.

There is no login by design (see [ASSUMPTIONS.md](ASSUMPTIONS.md)): the typed ID is the identity.

## Run the tests

Backend (needs Docker running, because tests start a real PostgreSQL with Testcontainers):

```
dotnet test backend
```

It includes the concurrency test (300 parallel applicants for 120 seats gives exactly 120 holds), same-student races, parallel confirms, expiry, ownership, validation, and a two-process test on one database.

Frontend (no Docker needed; run `npm --prefix frontend install` once):

```
npm --prefix frontend test
```

Covers expiry logic and the live `EXPIRED` state, error messages, student ID validation, and the apply and pay flows against a mocked API.

## Run without Docker (development)

```
docker compose up db                              # PostgreSQL only
dotnet run --project backend/src/Admission.Api    # copy backend/.env.example to backend/.env first
npm --prefix frontend install
npm --prefix frontend run dev                     # http://localhost:3000
```

## API

Base URL `http://localhost:8080`, JSON, student identity in the `X-Student-Id` header.

| Method and path | Purpose |
|---|---|
| `POST /classes` | Create a class |
| `GET /classes` | List classes with seat counts |
| `GET /classes/{id}` | One class |
| `POST /classes/{id}/holds` | Apply (hold a seat); `409` when full |
| `GET /holds/{id}` | Application status |
| `POST /holds/{id}/confirm` | Pay the fee (idempotent) |
| `DELETE /holds/{id}` | Cancel |
| `GET /me/holds` | All applications of this student |

Errors are `{ "code": "...", "message": "..." }` with `400`, `403`, `404` or `409`. Invalid input never returns `500`.


## Repository layout

```
backend/    .NET API (src/Admission.Api) and tests (tests/Admission.Tests), Dockerfile
frontend/   Next.js app, Vitest tests, Dockerfile
docker-compose.yml
ASSUMPTIONS.md   AI_USAGE.md   (this README, plus CLAUDE.md and AGENTS.md for AI agents)
docs/       PRD.md  ARCHITECTURE.md  RULES.md  DESIGN.md  PHASES.md  MEMORY.md
```
