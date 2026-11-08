# PRD — School Admission System

## 1. Purpose
Online, first-come-first-served admission for a private school. Each shift of each class is a separate "class" with a fixed seat count. Thousands of parents apply at once. An application **holds** a seat for a limited time; paying the fee **confirms** it; otherwise the seat returns automatically.

## 2. Users
| User | Goal |
|---|---|
| School admin | Create classes with a seat count; watch seat usage |
| Parent / student | Apply for a seat, pay the fee in time, cancel, see all own applications |

No login. Identity = 17-digit birth registration number, entered at the top of the UI and sent as `X-Student-Id`.

## 3. Scope
**In:** 8 API endpoints, React UI, PostgreSQL persistence, Docker compose, backend tests (incl. concurrency), docs.
**Out:** real payments, authentication, email/SMS, admin editing/deleting classes, i18n, pagination.
**Bonus (in plan):** multi-process safety on one DB; frontend tests.

## 4. Domain
- **Class**: `id, name, totalSeats`.
- **Hold (application)**: `id, classId, studentId, status, expiresAt`.
- **Status**: `HELD → CONFIRMED | CANCELLED | EXPIRED`. Terminal states never change.
- **Active** = `CONFIRMED`, or `HELD` with `expiresAt > now`.
- `heldSeats` = active HELD, `confirmedSeats` = CONFIRMED, `availableSeats = total − held − confirmed`.

## 5. Functional requirements
### Backend (mandatory)
| # | Requirement |
|---|---|
| B1 | `held + confirmed ≤ totalSeats` always, under any concurrency |
| B2 | Hold lasts `HOLD_DURATION_SECONDS` (default 600); expiry frees the seat with no API call |
| B3 | One active/confirmed application per student per class |
| B4 | Only the owner's unexpired `HELD` application can be confirmed |
| B5 | Repeated confirm is idempotent; `confirmedSeats` rises once |
| B6 | Data survives `docker compose down` → `up` |
| B7 | Invalid input → 4xx, never 500 |

### UI (mandatory)
| # | Requirement |
|---|---|
| U1 | Everything doable in the UI |
| U2 | Expiry shows `EXPIRED` live, no reload; Pay disabled |
| U3 | Seat counts refresh every few seconds |
| U4 | Applications persist across refresh/browser restart |
| U5 | Every error shown in plain language (full, expired, invalid, server down) |

### Screens
Create class (admin) · Class list (counts + Apply) · Application (status, countdown, Pay, Cancel) · My applications.

## 6. API contract (must not change)
Base `http://localhost:8080`, JSON, header `X-Student-Id`.

| Method & path | Success | Notes |
|---|---|---|
| `POST /classes` | 201 `{id,name,totalSeats}` | |
| `GET /classes` | 200 `[{id,name,totalSeats,heldSeats,confirmedSeats,availableSeats}]` | |
| `GET /classes/{id}` | 200 same shape | |
| `POST /classes/{id}/holds` | 201 `{holdId,classId,studentId,status,expiresAt}` | 409 when full |
| `GET /holds/{id}` | 200 `{holdId,classId,status,expiresAt}` | |
| `POST /holds/{id}/confirm` | 200 `{holdId,status}` | |
| `DELETE /holds/{id}` | 204 | |
| `GET /me/holds` | 200 `[{holdId,classId,className,status,expiresAt}]` | |

Extra response fields are allowed; none may be removed or renamed. `expiresAt` is UTC ISO-8601 with `Z`.

### Error semantics (our assumptions)
`400` invalid input/header · `403` not the owner · `404` unknown id · `409` full / duplicate application / expired / bad state. Body: `{ "code": "...", "message": "..." }`.

## 7. Non-functional
- Correct under ≥300 parallel requests; two backend processes on one DB.
- Only Docker needed: `docker compose up` → frontend `http://localhost:3000`, API `http://localhost:8080`.
- Config via env: `ConnectionStrings__Default`, `HOLD_DURATION_SECONDS`, frontend `BACKEND_URL`.

## 8. Acceptance / deliverables checklist
- [ ] 8 endpoints match contract
- [ ] Concurrency test proves no oversell
- [ ] ≥3 backend tests (target ~10)
- [ ] `docker compose up` from a clean clone works; data persists after down/up
- [ ] UI meets U1–U5
- [ ] `README.md`, `ASSUMPTIONS.md`, `AI_USAGE.md`
- [ ] Incremental git history, public GitHub repo, submitted before 11 Nov 2026 23:59:59
- [ ] Assignment PDF is NOT in the repo
