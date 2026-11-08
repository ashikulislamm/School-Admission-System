# MEMORY — backend

Notes for `backend/`. Read `ARCHITECTURE.md` §3–5 and `RULES.md` §1–2 first. Keep entries short and dated.

## Status
- Not started.

## Fixed facts
- .NET 8 minimal API, EF Core + Npgsql, listens on `http://+:8080`.
- Env: `ConnectionStrings__Default`, `HOLD_DURATION_SECONDS` (600), `CORS_ORIGINS`.
- IDs: `c_`/`h_` + 12 url-safe chars.
- Error body: `{ code, message }`; codes: `CLASS_FULL`, `ALREADY_APPLIED`, `HOLD_EXPIRED`, `HOLD_NOT_ACTIVE`, `NOT_OWNER`, `NOT_FOUND`, `INVALID_INPUT`, `INVALID_STUDENT_ID`.

## Algorithms (summary)
- Apply: lock class row → expire this student's lapsed holds → duplicate check → count active → insert with `now() + duration`.
- Confirm: lock hold row → owner? → CONFIRMED no-op / HELD&unexpired → CONFIRMED / else 409.
- Cancel: lock hold row → owner → HELD→CANCELLED; CANCELLED/EXPIRED no-op 204; CONFIRMED 409.
- Lock order: class then hold.
- Effective status: `HELD` with `expires_at <= now()` is reported `EXPIRED`.

## Decisions
_Add dated entries here._

## Gotchas
- Compute `expires_at` in SQL (`now() + make_interval(secs => @d)`), not in C#.
- Catch Npgsql `PostgresException` `23505` → 409, `22xxx`/`23xxx` other → 4xx where input-caused.
- Migrations under `pg_advisory_lock` so two processes don't race at startup.
- Sweeper must be disable-able via config for the lazy-expiry test.
- JSON binding errors (`BadHttpRequestException`, `JsonException`) must map to 400, not 500.

## Test notes
- Testcontainers Postgres fixture shared per collection; truncate tables between tests.
- Concurrency test: 300 parallel distinct student IDs, 120 seats, assert exactly 120×201.
- Two-instance test: two `WebApplicationFactory` on the same connection string.

## Open items
_None._
