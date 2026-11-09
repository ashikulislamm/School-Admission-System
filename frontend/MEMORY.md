# MEMORY — frontend

Notes for `frontend/`. Read `DESIGN.md`, `ARCHITECTURE.md` §6 and `RULES.md` §3 first. Keep entries short and dated.

## Status
- Initialized 2026-11-09: Next.js 16.4.5 + React 19.3 + Tailwind 4 scaffold, `output: "standalone"`, builds clean. Phase 4 done 2026-11-09: deps, tokens, API client, error copy, runtime config, UI primitives, app shell. Next: Phase 5 features.

## Fixed facts
- Next.js 16 (App Router, standalone), React 19, TypeScript strict, Tailwind 4, Radix primitives, lucide-react, TanStack Query. Routing is Next file routing (no React Router).
- Served by `node server.js` on port 3000 (host 3000). No nginx.
- Backend URL: root layout (server component) does `await connection()` and renders `<script>window.__CONFIG__ = {BACKEND_URL}</script>` from `process.env.BACKEND_URL`; fallback `http://localhost:8080`.
- This Next version differs from older releases: read `node_modules/next/dist/docs/` before using Next APIs (`frontend/AGENTS.md` says the same).
- All pages are `"use client"`; Next is only routing/build/server, the UI is SPA-like.
- Student ID stored in `localStorage` key `studentId`; must match `^\d{17}$`; sent as `X-Student-Id`.
- Polling: classes 3s, hold detail 3s, my holds 5s; paused when tab hidden.

## Layout (`src/`)
- `app/` `layout.tsx` (async server layout: `connection()` + `window.__CONFIG__` script, TopBar, Providers), `providers.tsx` (QueryClient; retries only network/5xx), route stubs `page.tsx`, `applications/`, `admin/` (title only until Phase 5).
- `api/` `client.ts` (`ApiError`, `apiFetch`, sends `X-Student-Id`, feeds clock offset), `types.ts`, `admission.ts` (one function per endpoint).
- `lib/` `config.ts`, `clock.ts` (`serverNow()`), `errors.ts` (`messageFor`, `isUnreachable`), `student-id.ts`, `cn.ts`. `hooks/useStudentId.ts` (`useSyncExternalStore`, hydration-safe).
- `components/` `TopBar`, `StudentIdField`, `PageHeader`; `ui/` `Button`, `Input`, `Alert`. Still to add in Phase 5: Table, StatusText, Countdown, Dialog, toast.

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
- 2026-11-09: Switched from Vite to Next.js (user choice). `cacheComponents` and `partialPrefetching` (scaffold defaults) turned off: with them on, hidden routes stay mounted via `<Activity>` and `dynamic` segment config is removed, both unwanted here.
- 2026-11-09: Dynamic route `/applications/[holdId]` works because we run a Node server (static export would need `generateStaticParams`).

## Gotchas
- Tokens: `globals.css` resets Tailwind's default colors/radii/shadows/type (`--color-*: initial` etc.), so only DESIGN tokens exist. Utilities: `bg-bg bg-surface border-line border-line-strong text-fg text-muted bg-accent text-success text-warning text-danger text-neutral hover:bg-row-hover rounded-control shadow-overlay text-title text-section text-body text-label`.
- Student ID field: the typed value is the identity. Only a full 17 digits is stored; a partial value clears storage so no request goes out under a stale ID.
- The `Date` header is truncated to 1s; `clock.ts` adds 500ms to centre the error.
- `await connection()` in the root layout makes every route dynamic on purpose (runtime env).
- A shell heredoc with apostrophes can silently abort the whole command; use the Write tool for source files.
- `NEXT_PUBLIC_*` is build-time; never use it for the backend URL.
- `output: "standalone"` on Windows needs symlink rights for a local `next build`; Docker (Linux) builds are fine. Standalone does not copy `public/` or `.next/static`: the Dockerfile must copy both.
- Hooks, state, `localStorage` need `"use client"`; guard browser-only reads (`window`) against SSR.
- Use `visibilitychange`-aware polling (`refetchIntervalInBackground: false`).
- Timers: clean up intervals on unmount; use one shared ticking hook for countdowns.
- Tests use MSW; use fake timers for countdown tests.

## Open items
_None._
