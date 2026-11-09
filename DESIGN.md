# DESIGN SYSTEM

A real application UI: sleek, modern, clean, professional. It looks like a product dashboard, not a landing page.

## 1. Principles
1. **Quiet.** Neutral surfaces, one accent, generous whitespace.
2. **Dense but calm.** Show data, not decoration. Tables and rows over cards-in-cards.
3. **Only necessary text.** No taglines, hero copy, helper paragraphs, or filler. Labels are 1–3 words.
4. **State is obvious.** Status is conveyed by plain colored text and clear controls, not decoration.
5. **Consistent.** Everything comes from tokens below. Nothing is ad hoc.

## 2. Hard bans
- No gradients (backgrounds, buttons, text, borders)
- No badges, pills, chips, or tag-style labels
- No emoji, illustrations, hero sections, decorative icons
- No glassmorphism, blur, glow, neon
- No heavy or colored shadows; no animated flourishes
- No more than one accent color; no multi-color rainbow charts
- No placeholder lorem, marketing copy, or exclamation marks
- No tooltips for essential information

## 3. Color tokens (CSS variables)
Light:
| Token | Value | Use |
|---|---|---|
| `--bg` | `#F1F5F9` | App background (soft slate) |
| `--surface` | `#FFFFFF` | Panels, tables, inputs |
| `--border` | `#E5E7EB` | Dividers, outlines |
| `--border-strong` | `#D1D5DB` | Input outlines, hover |
| `--text` | `#111827` | Primary text |
| `--text-muted` | `#6B7280` | Secondary text, labels |
| `--accent` | `#2563EB` | Primary action, links, focus ring |
| `--accent-hover` | `#1D4ED8` | Primary hover |
| `--success` | `#15803D` | CONFIRMED text |
| `--warning` | `#B45309` | HELD text, low-time |
| `--danger` | `#B91C1C` | Errors, destructive text |
| `--neutral` | `#6B7280` | EXPIRED / CANCELLED text |
| `--row-hover` | `#E8EDF4` | Table row hover |

Light theme only (decision 2026-11-09: no dark mode; `color-scheme: light`).

Text contrast ≥ 4.5:1 on its surface.

## 4. Typography
- Font: `Inter`, fallback `system-ui, -apple-system, "Segoe UI", sans-serif`. Tabular numbers (`font-variant-numeric: tabular-nums`) for all counts and timers.
| Role | Size / line | Weight |
|---|---|---|
| Page title | 20 / 28 | 600 |
| Section title | 16 / 24 | 600 |
| Body | 14 / 20 | 400 |
| Label / table header | 12 / 16 | 500, `--text-muted` |
| Numeric emphasis | 14 / 20 | 600, tabular |
- No uppercase transforms except status values (see §7). No letter-spacing tweaks.

## 5. Spacing, radius, elevation
- 4px base grid: `4 8 12 16 24 32 48`.
- Page max width 1040px, centered; gutters 24px (16px on mobile).
- Radius: `6px` for controls and panels. No fully round shapes except focus rings.
- Elevation: none by default (1px border). Overlays (dialog, toast) only: `0 8px 24px rgba(0,0,0,0.08)`.
- Dividers: 1px `--border`.

## 6. Layout
```
┌──────────────────────────────────────────────────────────────┐
│ Admissions      Classes  Applications  Admin     [ Student ID ]│  56px top bar, 1px bottom border
├──────────────────────────────────────────────────────────────┤
│ Page title                                      [Primary action]│
│ ─────────────────────────────────────────────────────────────│
│ content (table / form / detail)                                │
└──────────────────────────────────────────────────────────────┘
```
- Top bar: app name (left), nav links, student-ID input (right). Active nav: `--text` + 2px `--accent` bottom border; inactive `--text-muted`.
- Content is a single column. Mobile: nav scrolls horizontally; tables become stacked rows (label left, value right).

## 7. Components
### Button
- Height 36px, padding 0 14px, radius 6px, 14px / 500.
- **Primary:** `--accent` fill, white text. **Secondary:** `--surface` fill, 1px `--border-strong`, `--text`. **Destructive:** secondary style with `--danger` text.
- Hover: darker fill or `--row-hover`. Disabled: 50% opacity, `not-allowed`, no hover. Loading: label replaced by "…ing" text, width fixed; no spinner animation required (a 14px simple spinner is allowed).
- One primary button per view.

### Input
- Height 36px, `--surface`, 1px `--border-strong`, radius 6px. Focus: 2px `--accent` ring, offset 1px. Error: `--danger` border + one line of `--danger` text below. Label above, 12px muted.
- Student-ID field: monospace-free, tabular numerals, `inputmode="numeric"`, max 17 digits, inline validity text only when invalid.

### Table (class list, applications)
- Header row 12px muted, 40px tall, bottom border. Body rows 48px, bottom border, hover `--row-hover`. No zebra stripes, no outer card shadow.
- Numeric columns right-aligned, tabular.
- Row actions right-aligned: at most two buttons.

### Status text (replaces badges)
Plain text, 12px, 600, uppercase, colored by token, preceded by nothing:
`HELD` `--warning` · `CONFIRMED` `--success` · `EXPIRED` `--neutral` · `CANCELLED` `--neutral`.
Never inside a box, border or filled background.

### Seat availability
Numbers in columns: Total, Held, Confirmed, Available. A thin 4px progress bar (single color `--accent`, track `--border`) may appear under the class name. Full class: Available shown in `--danger`, Apply disabled.

### Countdown
`mm:ss`, tabular, 14px / 600; `--text` normally, `--warning` under 60s. At 0 the status switches to EXPIRED and Pay is disabled; no layout shift.

### Error / feedback
- Inline `Alert`: full width, 1px `--danger` border, `--surface` fill, `--danger` text, 12px 16px padding, role="alert". One sentence, plain language, plus a Retry button when retry makes sense.
- Success feedback: short toast (bottom-right, 3s), e.g. "Seat held". No celebratory UI.
- Server unreachable: persistent top-of-content alert "Can't reach the server. Retrying." that clears itself on recovery.
- Never a blank page; root error boundary shows an Alert and a Reload button.

### Dialog
Only for destructive confirm (Cancel application). 400px, 1px border, overlay `rgba(0,0,0,0.4)`, title + one line + two buttons.

### Empty state
One line of muted text and, if relevant, one button. No illustration.

## 8. Screens
**Classes** — Table: Name · Total · Held · Confirmed · Available · [Apply]. Polls every 3s without flicker (keep previous data, no skeleton on refetch).
**Application detail** — Two-column definition list: Class, Status, Expires (countdown), Student. Actions: **Pay admission fee** (primary), **Cancel** (secondary destructive). After confirm: Pay hidden, status CONFIRMED.
**My applications** — Table: Class · Status · Expires · [Open]. Newest first.
**Admin** — Single form: Class name, Total seats, [Create class]. After success, inline reset + toast.

## 9. Copy rules
- Sentence case. No periods in labels or buttons. Errors are one short sentence.
- Buttons: verb first — "Apply", "Pay admission fee", "Cancel application", "Create class".
- Error copy examples:
  - full: "All seats in this class are taken."
  - expired: "This application has expired."
  - duplicate: "You already have an application for this class."
  - invalid ID: "Enter a 17-digit birth registration number."
  - invalid input: "Check the highlighted fields."
  - server down: "Can't reach the server. Retrying."
  - unknown: "Something went wrong. Try again."
- Never show technical terms (HTTP codes, stack traces, "undefined").

## 10. Motion
Only 120ms opacity/color transitions on hover/focus and dialog fade. Respect `prefers-reduced-motion`. No skeleton shimmer; use static muted placeholders.

## 11. Accessibility
- Visible focus ring everywhere; logical tab order; Esc closes dialogs.
- Color is never the only status signal: the status word is always present.
- `aria-live="polite"` for countdown state changes and toasts; `role="alert"` for errors.
- Touch targets ≥ 36px (44px on mobile primary actions).

## 12. Implementation
- Tailwind CSS with tokens mapped to the CSS variables above; Radix primitives (Dialog, Toast) for behavior; lucide-react only for functional icons (e.g., close), sized 16px, stroke 1.75.
- Shared primitives in `frontend/src/components/ui/` (Button, Input, Table, Alert, Dialog, StatusText, Countdown). Features compose these; no one-off styled elements.
- A PR/commit that adds a color, radius, shadow, or font not listed here is rejected.

## 13. Polish revision (2026-11-09, user request)
Applied on top of the rules above; hard bans (gradients, badges, emoji, one accent) still hold.
- Radius: controls 8px, cards/dialogs/toasts 12px. Progress bar fill and track are fully rounded.
- Elevation: cards, table panels, top bar and buttons use `--shadow-card` (hairline, neutral). Overlays use a slightly larger neutral shadow.
- Tables and forms sit inside white cards on the soft `--bg`; table header row uses `--bg`; body rows 64px.
- Page title 24/32; brand name in `--accent`; inputs 40px high.
- Empty and loading states are centered inside a card.
