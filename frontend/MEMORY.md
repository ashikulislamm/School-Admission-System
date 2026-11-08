# MEMORY — frontend

Notes for `frontend/`. Read `DESIGN.md`, `ARCHITECTURE.md` §6 and `RULES.md` §3 first. Keep entries short and dated.

## Status
- Not started.

## Fixed facts
- React 18, Vite, TypeScript strict, Tailwind, Radix primitives, lucide-react, TanStack Query, React Router.
- Served by nginx on container port 80 → host `3000`.
- Backend URL: `window.__CONFIG__.BACKEND_URL` (written by `docker/entrypoint.sh`), fallback `http://localhost:8080`.
- Student ID stored in `localStorage` key `studentId`; must match `^\d{17}$`; sent as `X-Student-Id`.
- Polling: classes 3s, hold detail 3s, my holds 5s; paused when tab hidden.

## UI rules (summary of DESIGN.md)
- No gradients, no badges/pills, no filler text, one accent `#2563EB`, 6px radius, 1px borders, no shadows except overlays.
- Status is plain uppercase colored text: HELD warning, CONFIRMED success, EXPIRED/CANCELLED neutral.
- One primary button per view. Verb-first labels. Errors are one plain sentence in an Alert.
- Shared primitives only from `src/components/ui/`.

## Behaviors to get right
- Countdown from server `expiresAt` + clock offset (response `Date` header). At 0: show EXPIRED, disable Pay, invalidate queries.
- Refetch keeps previous data (`placeholderData: keepPreviousData`) → no flicker.
- Server is the authority: still handle 409/403 after optimistic UI gating.
- Map `ApiError.code` → copy in `lib/errors.ts`; network error → "Can't reach the server. Retrying."
- Root error boundary: never a blank page.

## Decisions
_Add dated entries here._

## Gotchas
- Vite env is build-time; don't use `import.meta.env` for backend URL.
- Entry script must have LF endings (Windows).
- Use `visibilitychange`-aware polling (`refetchIntervalInBackground: false`).
- Timers: clean up intervals on unmount; use one shared ticking hook for countdowns.
- Tests use MSW; use fake timers for countdown tests.

## Open items
_None._
