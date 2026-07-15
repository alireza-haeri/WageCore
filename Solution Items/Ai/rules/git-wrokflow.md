# Git Workflow

Solo project — no PR-review gate, no team-coordination ceremony. These rules exist purely so history stays readable and useful to future-you.

## Commit Messages

Use Conventional Commits:

```
{type}({scope}): {short summary, imperative mood}

{optional longer body — why, not what}
```

Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `build`.
Scope: the feature or layer touched — `feat(payroll): add proration for partial months`, `fix(attendance): correct overtime threshold comparison`, `refactor(application): extract validation behavior`.

- Subject line ≤ 72 chars, imperative mood ("add", not "added"/"adds").
- Body explains **why** a change was made when it's not obvious from the diff — especially for payroll-rule changes, where the "why" is often "labor law changed" or "fixed a rounding bug affecting X."

## Branching

- `main` is always in a working state.
- Feature/fix work happens on short-lived branches: `feat/{short-description}`, `fix/{short-description}`.
- Merge back to `main` when the feature works and tests pass — no need for a formal PR process, but do a self-review of the diff before merging (`git diff main`).
- Delete the branch after merging.

## Commit Size

- Prefer small, focused commits over large ones — each commit should represent one coherent change (one Command added, one bug fixed), not a mix of unrelated changes.
- It's fine to commit work-in-progress locally, but squash/clean up the history before merging into `main` so the log stays meaningful.

## Tags / Releases

- Tag meaningful milestones (e.g. `v0.1.0` for first working payroll calculation end-to-end) using semantic versioning, once the project reaches a point where "releases" are meaningful.

## What NOT to Commit

- `bin/`, `obj/`, and other build artifacts (ensure `.gitignore` covers these).
- Secrets/connection strings — use `appsettings.Development.json` (gitignored) or user-secrets, never commit real credentials.