# AGENTS.md — Instructions for AI agents

Applies to any AI agent (Claude Code, subagents, others) working in this repo.

## 1. Read first, in order
1. `CLAUDE.md` — quick project context and commands
2. `docs/PRD.md` — what to build (API contract is frozen)
3. `docs/ARCHITECTURE.md` — how it is built
4. `docs/RULES.md` — non-negotiable rules
5. `docs/DESIGN.md` — before touching any UI
6. `docs/PHASES.md` — current status; pick the next unchecked item
7. `docs/MEMORY.md` and the `MEMORY.md` of the area you work in (`backend/MEMORY.md` or `frontend/MEMORY.md`)

## 2. Working loop
1. Choose the next unchecked item in `docs/PHASES.md`. Do not skip ahead.
2. Restate the task in one line; identify touched files.
3. Implement the smallest change that satisfies it.
4. Add or update tests (mandatory for concurrency, confirm, cancel, expiry).
5. Build and run tests. Report failures verbatim; never claim success without running.
6. Update `docs/PHASES.md` checkbox and the relevant `MEMORY.md` (`docs/MEMORY.md`, `backend/MEMORY.md` or `frontend/MEMORY.md`).
7. Do NOT commit. Suggest a Conventional Commit message (see `docs/RULES.md` §4); the user runs every commit.

## 3. Boundaries
- Never change the API contract. Extra fields only.
- Never add stored seat counters or in-memory locks.
- Never introduce UI patterns banned in `docs/DESIGN.md` (gradients, badges, filler text, extra colors).
- Never commit the assignment PDF, secrets, or build output.
- Do not add dependencies without a reason noted in the relevant `MEMORY.md`.
- Never run `git commit`, `git branch -m` or `git push`; the user makes all commits.
- Ask the user before: pushing, force operations, deleting files, rewriting history, publishing anywhere.

## 4. Roles (when delegating to subagents)
| Role | Scope | Must read |
|---|---|---|
| backend | `backend/` | RULES §1–2, ARCHITECTURE §3–5, backend/MEMORY |
| frontend | `frontend/` | RULES §3, DESIGN, ARCHITECTURE §6, frontend/MEMORY |
| tests | `backend/tests`, frontend tests | RULES §5, ARCHITECTURE §8 |
| devops | Dockerfiles, compose | RULES §6, ARCHITECTURE §7 |
| docs | `docs/` and root docs | RULES §7 |
Subagents get a self-contained prompt: goal, files, constraints, expected output. Parallelize only independent work (e.g., frontend vs backend after the contract is fixed).

## 5. Quality gates before saying "done"
- [ ] Builds with no warnings treated as errors
- [ ] Relevant tests pass (and concurrency test still passes)
- [ ] No 500 reachable from invalid input in what you touched
- [ ] UI matches `docs/DESIGN.md`; all states handled (loading, empty, error)
- [ ] Memory + phases updated; commit message suggested to the user

## 6. AI usage log (feeds `AI_USAGE.md`)
Whenever an AI-generated output was wrong, misleading, or needed correction, append a line to `docs/MEMORY.md` under "AI mistakes log": *what was wrong → how it was caught → fix*. Only real events; never invent entries. At release, summarize them in `AI_USAGE.md`.

## 7. Communication
Be concise. State what changed, what was verified, what is left. Flag assumptions explicitly so they can go to `ASSUMPTIONS.md`.
