# MEMORY (root)

Project-wide memory. Area-specific notes: `backend/MEMORY.md`, `frontend/MEMORY.md`. Keep entries short and dated. Newest first within each section.

## Status
- Phase: 0 and backend foundation done (config, errors, EF model + `Initial` migration, /health, Dockerfile, compose). Verified inside Docker by the user. Phase 1 (8 endpoints + sweeper) done 2026-11-09. Phases 0-3 done (backend complete, containerized, persistence verified). Phase 4 (frontend foundation) done 2026-11-09. Phase 5 (frontend features) code done 2026-11-09; browser walkthrough of U1-U5 pending. Phase 6 done 2026-11-09 (frontend container, compose up verified by the user). Phase 7 (frontend tests) done 2026-11-09. Next: Phase 8 docs and release.
- The user makes all git commits (see Claude memory). Branch is still `master` until the user renames it.
- Last updated: 2026-11-09. Deadline: 2026-11-11 23:59:59.

## Decisions
| Date | Decision | Why |
|---|---|---|
| 2026-11-08 | Counts derived from `holds`, row lock on class | Single source of truth, multi-process safe |
| 2026-11-08 | Lazy expiry + background sweeper | Seat frees exactly at expiry with no API call |
| 2026-11-08 | DB clock for all expiry logic (`clock_timestamp()`, not `now()`, since 2026-11-09) | Avoid process clock skew; `now()` is stale after waiting on a lock |
| 2026-11-08 | Confirm expired → 409; cancel confirmed → 409; not owner → 403 | Consistent semantics; document in ASSUMPTIONS |
| 2026-11-09 | Frontend is Next.js 16 (App Router, standalone), not Vite; `BACKEND_URL` injected by root layout at request time | User choice; still React; one image, any backend URL; drops nginx + entrypoint script |
| 2026-11-08 | Tailwind + Radix + lucide, no heavy UI kit | Full control for the minimal DESIGN.md look |

## Assumptions to carry into ASSUMPTIONS.md
- Student ID must be exactly 17 digits.
- A student may re-apply after CANCELLED or EXPIRED.
- Confirmed admissions do not expire and cannot be cancelled.
- Class names are not required to be unique.
- `GET /holds/{id}` is readable without the header (spec lists none).

## Gotchas
- Windows CRLF breaks shell scripts in Linux containers → `.gitattributes` `eol=lf`.
- `NEXT_PUBLIC_*` is inlined at build time → read `BACKEND_URL` at request time in the root layout (`await connection()`), never via `NEXT_PUBLIC_*`.
- Testcontainers requires Docker running on the host.
- Assignment PDF is confidential: never commit it.

## AI mistakes log (feeds AI_USAGE.md)
Format: what was wrong → how it was caught → fix. Real events only.
- 2026-11-08: AI wrote `ConnectionStringHelper` using `NpgsqlConnectionStringBuilder.ContainsKey("SSL Mode")` to detect an explicit SSL choice. The typed builder reports every keyword as present, so SSL would never have been forced for Neon/Supabase. → Caught by a unit test (4 failures) → Now inspects the raw string with `DbConnectionStringBuilder`.
- 2026-11-08: AI used `HasCheckConstraint` on the entity builder; EF Core 10 marks it obsolete, and warnings-as-errors broke the build → compiler message → moved to `ToTable(t => t.HasCheckConstraint(...))`.
- 2026-11-08: Test project resolved EF Core 10.0.4 while the API used 10.0.14 (Design package is private), causing MSB3277 warnings → seen in build output → pinned `Microsoft.EntityFrameworkCore` and `.Relational` 10.0.14 in the API project.

- 2026-11-09: AI interpolated a SQL fragment constant (EffectiveStatusSql) into SqlQuery<T>($"""..."""); EF bound it as a parameter, so status came back as the SQL text. -> Caught by curl smoke test -> switched to SqlQueryRaw with {{Const}}.

- 2026-11-09: AI assumed minimal-API JSON binding was strict about types; `totalSeats: "5"` was accepted as 201. -> Caught by the validation test matrix -> `JsonNumberHandling.Strict`.

- 2026-11-09: AI edited UTF-8 docs with PowerShell `Get-Content`/`WriteAllText` (ANSI read), turning `→ × ≤ —` into mojibake (double-encoded after a second edit); one corrupted version was committed. → Noticed as garbled text while editing `MEMORY.md` → repaired by reversing the encoding; doc edits now use Python with explicit UTF-8.

## Open questions
- None.
