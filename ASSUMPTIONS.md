# ASSUMPTIONS

Where the spec is not explicit, this is what I assumed and why.

## Identity and input
| Assumption | Why |
|---|---|
| `X-Student-Id` must be exactly 17 digits, otherwise `400`. | The brief says it is a 17-digit birth registration number. |
| `X-Student-Id` is required for apply, confirm, cancel and `/me/holds`. Class endpoints and `GET /holds/{id}` do not need it. | The brief lists the header only on those endpoints. |
| Class name is trimmed and must be 1–100 characters. Names need not be unique. | Shift names can legitimately repeat across years, and the brief has no uniqueness rule. |
| `totalSeats` must be an integer from 1 to 100000. | Zero or negative seats make no sense. The upper bound stops absurd input. |
| Malformed JSON or a wrong field type returns `400`. An unknown or malformed id returns `404`. | Rule 7: invalid input is always 4xx, never 500. |

## Hold lifecycle
| Assumption | Why |
|---|---|
| A seat counts as held only while `status = HELD` and `expiresAt` is in the future. | The seat must return automatically at expiry, with no API call. |
| Expiry is decided with the database clock, not the application clock. | Two backend processes with different clocks must agree. |
| A student may apply again after a `CANCELLED` or `EXPIRED` application in the same class. | Rule 3 forbids only active or confirmed duplicates. |
| A duplicate active application returns `409`. | It is a state conflict, not bad input. |
| Confirming an expired or cancelled hold returns `409`. | The request is valid but the hold is in the wrong state. |
| Confirming or cancelling another student's hold returns `403`. | Rule 4 says only the owner can confirm, so it is "forbidden", not "not found". |
| A repeated confirm on an already `CONFIRMED` hold returns `200` and changes nothing. | Rule 5: confirm is idempotent. |
| Cancelling a `HELD` hold gives `CANCELLED` (`204`). Cancelling an already `CANCELLED` or `EXPIRED` hold returns `204` and changes nothing. | Idempotent delete is friendlier to retries. |
| A `CONFIRMED` admission cannot be cancelled (`409`). | The brief defines cancel for applications, not paid admissions. Refunds are out of scope. |
| Confirmed admissions never expire. | Expiry applies only to unpaid holds. |
| `GET /me/holds` returns every application of the student, newest first, including expired and cancelled ones. | The brief says "all applications and the status of each". |
| `GET /holds/{id}` reports a lapsed `HELD` hold as `EXPIRED` even if the background job has not yet saved that status. | The UI must never show a stale `HELD`. |

## Seat counts
| Assumption | Why |
|---|---|
| `availableSeats = totalSeats − heldSeats − confirmedSeats`. Only unexpired holds count as held. | Matches the example numbers and rule 1. |
| Counts are computed from the holds table, not stored in a counter column. | One source of truth, so counters cannot drift. |

## API shape
| Assumption | Why |
|---|---|
| IDs look like `c_` or `h_` plus 12 random URL-safe characters. | The brief says the ID format is my choice. |
| Timestamps are UTC ISO-8601 ending in `Z`. | Matches the examples. |
| Errors return `{ "code": "...", "message": "..." }`. | A stable code for the UI and a readable message for people. |
| CORS allows the frontend origin from config. | The browser calls the backend on a different port. |

## Platform
| Assumption | Why |
|---|---|
| The backend targets .NET 10 (the brief allows ".NET 8 or newer"). | It is the installed LTS SDK, and it avoids needing a second runtime. |
| The frontend is served at `http://localhost:3000`. | The brief gives no frontend port. |
| The backend URL is injected into the frontend at container start, not at build time. | The brief requires it to be configurable by environment variable. |

## UI
| Assumption | Why |
|---|---|
| No login. The student ID entered at the top is saved in the browser's `localStorage`. | Rule 11: applications must survive a refresh or a browser restart. |
| Admin screens are open to anyone. | The brief says no login is required. |
| The countdown uses the server's `expiresAt` and corrects for the difference between the browser clock and the server clock. | A wrong laptop clock must not show the wrong time left. |
| The UI disables "Apply" and "Pay" when they cannot succeed, but still handles server `409` and `403` replies. | The server is the authority. |

## How `held + confirmed ≤ totalSeats` is kept with several backend processes (bonus)
1. Every apply runs in a database transaction that first locks the class row with `SELECT ... FOR UPDATE`. PostgreSQL makes a second process, on any port, wait for that lock.
2. Inside the lock the code counts active holds and inserts only if the count is below `totalSeats`. Nothing is decided from memory.
3. The backend keeps no in-memory counters, locks or caches that affect correctness, so any number of processes behave the same.
4. A partial unique index on `(class_id, student_id)` for active statuses blocks duplicates even if application code has a bug.
5. Expiry compares against the database `now()`. The background sweeper only tidies statuses and is safe to run in every process.
6. A test starts two app instances on one database, sends parallel requests to both, and checks the cap is never exceeded.
