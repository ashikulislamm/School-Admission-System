# MEMORY — backend

Notes for `backend/`. Read `ARCHITECTURE.md` §3–5 and `RULES.md` §1–2 first. Keep entries short and dated.

## Status
- Foundation done (2026-11-08): config, error handling, EF model + `Initial` migration, startup migrate with retry, `/health`, Dockerfile, compose, 14 unit tests passing.
- Verified in Docker (user-confirmed 2026-11-08): `docker compose up -d --build` works, `/health` returns ok, tables exist.
- Next: Phase 1 endpoints (classes, holds, confirm, cancel, /me/holds), sweeper.

## Layout (`src/Admission.Api`)
- `Configuration/` `AppSettings` (validated env config), `ConnectionStringHelper` (URL or key-value, SSL for cloud hosts), `DotEnv` (dev-only .env loader).
- `Errors/` `ApiException` + `ErrorCodes` + `GlobalExceptionHandler` (all exceptions → `{code,message}`).
- `Models/` `ClassEntity`, `Hold`, `HoldStatus`. `Data/` `AppDbContext`, `DatabaseInitializer`, `DesignTimeDbContextFactory`, `Migrations/`.
- `Endpoints/` `HealthEndpoints` (so far).

## Fixed facts
- net10.0 minimal API, EF Core 10.0.14 + Npgsql 10.0.3 (both pinned in the csproj).
- Env: `ConnectionStrings__Default` or `DATABASE_URL` (URL or key-value), `HOLD_DURATION_SECONDS` (600), `CORS_ORIGINS`, `PORT` (default 8080).
- `backend/.env` is loaded only when `ASPNETCORE_ENVIRONMENT=Development`; real env vars win. Template: `backend/.env.example`.
- Local run: `dotnet run --project src/Admission.Api` (profile listens on 8080; needs `.env` pointing at Postgres on localhost:5433).
- IDs: `c_`/`h_` + 12 url-safe chars.
- Error codes: `INVALID_INPUT`, `INVALID_STUDENT_ID`, `NOT_FOUND`, `NOT_OWNER`, `CLASS_FULL`, `ALREADY_APPLIED`, `HOLD_EXPIRED`, `HOLD_NOT_ACTIVE`, `METHOD_NOT_ALLOWED`, `SERVICE_UNAVAILABLE`, `INTERNAL_ERROR`.

## Algorithms (summary)
- Apply: lock class row → expire this student's lapsed holds → duplicate check → count active → insert with `now() + duration`.
- Confirm: lock hold row → owner? → CONFIRMED no-op / HELD&unexpired → CONFIRMED / else 409.
- Cancel: lock hold row → owner → HELD→CANCELLED; CANCELLED/EXPIRED no-op 204; CONFIRMED 409.
- Lock order: class then hold. Effective status: `HELD` with `expires_at <= now()` is reported `EXPIRED`.

## Decisions
- 2026-11-08: Schema uses snake_case columns set explicitly (no naming-convention package).
- 2026-11-08: Migrations run at startup with retry (30×2s); EF Core's own migration lock covers two processes.
- 2026-11-08: Cloud DB mode: use direct (non-pooled) connection; PgBouncer transaction mode can break session features.

## Gotchas
- `NpgsqlConnectionStringBuilder.ContainsKey` is always true; inspect raw text (see AI mistakes log).
- EF Core 10: check constraints go in `ToTable(t => t.HasCheckConstraint(...))`.
- `dotnet ef` needs `DesignTimeDbContextFactory` because the app throws without a connection string.
- Run `dotnet tool restore` in `backend/` before `dotnet ef`. Add migration: `dotnet ef migrations add <Name> --project src/Admission.Api --output-dir Data/Migrations`.
- Compute `expires_at` in SQL (`now() + make_interval(secs => @d)`), not in C#.
- Sweeper must be disable-able via config for the lazy-expiry test.

## Test notes
- Unit tests (no Docker): `ConnectionStringHelperTests`, `AppSettingsTests`.
- Integration/concurrency tests: Testcontainers Postgres, shared fixture, truncate between tests.
- Two-instance test: two `WebApplicationFactory` on the same connection string.
