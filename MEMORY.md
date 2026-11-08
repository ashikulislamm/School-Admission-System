# MEMORY (root)

Project-wide memory. Area-specific notes: `backend/MEMORY.md`, `frontend/MEMORY.md`. Keep entries short and dated. Newest first within each section.

## Status
- Phase: 0 and backend foundation done (config, errors, EF model + `Initial` migration, /health, Dockerfile, compose). Verified inside Docker by the user. Next: Phase 1 endpoints.
- The user makes all git commits (see Claude memory). Branch is still `master` until the user renames it.
- Today: 2026-11-08. Deadline: 2026-11-11 23:59:59.

## Decisions
| Date | Decision | Why |
|---|---|---|
| 2026-11-08 | Counts derived from `holds`, row lock on class | Single source of truth, multi-process safe |
| 2026-11-08 | Lazy expiry + background sweeper | Seat frees exactly at expiry with no API call |
| 2026-11-08 | DB clock for all expiry logic | Avoid process clock skew |
| 2026-11-08 | Confirm expired → 409; cancel confirmed → 409; not owner → 403 | Consistent semantics; document in ASSUMPTIONS |
| 2026-11-08 | Frontend at :3000, runtime `config.js` for `BACKEND_URL` | Env-configurable without rebuild |
| 2026-11-08 | Tailwind + Radix + lucide, no heavy UI kit | Full control for the minimal DESIGN.md look |

## Assumptions to carry into ASSUMPTIONS.md
- Student ID must be exactly 17 digits.
- A student may re-apply after CANCELLED or EXPIRED.
- Confirmed admissions do not expire and cannot be cancelled.
- Class names are not required to be unique.
- `GET /holds/{id}` is readable without the header (spec lists none).

## Gotchas
- Windows CRLF breaks shell scripts in Linux containers → `.gitattributes` `eol=lf`.
- Vite env is build-time → use runtime `config.js` via nginx entrypoint.
- Testcontainers requires Docker running on the host.
- Assignment PDF is confidential: never commit it.

## AI mistakes log (feeds AI_USAGE.md)
Format: what was wrong → how it was caught → fix. Real events only.
- 2026-11-08: AI wrote `ConnectionStringHelper` using `NpgsqlConnectionStringBuilder.ContainsKey("SSL Mode")` to detect an explicit SSL choice. The typed builder reports every keyword as present, so SSL would never have been forced for Neon/Supabase. → Caught by a unit test (4 failures) → Now inspects the raw string with `DbConnectionStringBuilder`.
- 2026-11-08: AI used `HasCheckConstraint` on the entity builder; EF Core 10 marks it obsolete, and warnings-as-errors broke the build → compiler message → moved to `ToTable(t => t.HasCheckConstraint(...))`.
- 2026-11-08: Test project resolved EF Core 10.0.4 while the API used 10.0.14 (Design package is private), causing MSB3277 warnings → seen in build output → pinned `Microsoft.EntityFrameworkCore` and `.Relational` 10.0.14 in the API project.

## Open questions
- None.
