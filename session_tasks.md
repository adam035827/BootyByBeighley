# Parallel Agent Session Tasks

## Session A

**Branch name:** `agent/docs-postgresql-alignment`

**Worktree directory:** `C:\BootyByBeighley-worktrees\docs-postgresql-alignment`

**Task:** Correct stale architecture documentation that still describes Cosmos DB as the active persistence implementation. Update it to match the current PostgreSQL, EF Core, and Npgsql implementation. Preserve any historical template context only when it is clearly labeled as historical.

**Files or folders the agent may write to:**

- `docs/FEATURES.md`
- `docs/INSTRUCTIONS.md`
- `docs/APP.md`

**Files or folders the agent may read but not write to:**

- `backend/src/BootyByBeighley.Infrastructure/`
- `backend/src/BootyByBeighley.Application/`
- `backend/src/BootyByBeighley.Api/`
- `backend/src/BootyByBeighley.Domain/`
- `backend/BootyByBeighley.sln`
- `README.md`
- `copilot-instructions.md`

**Commands the agent may run:**

```powershell
rg -n "Cosmos|PostgreSQL|EF Core|Npgsql" docs backend/src

git diff --check -- docs/FEATURES.md docs/INSTRUCTIONS.md docs/APP.md
```

The agent may use read-only file inspection commands such as `rg`, `Get-Content`, and `git diff`. It must not run database migrations or modify application code.

**Definition of done:**

- Active architecture descriptions identify PostgreSQL through EF Core and Npgsql as the persistence implementation.
- Stale instructions for Cosmos-specific repositories, documents, mappings, and dependency injection are removed or explicitly labeled as historical template information.
- Documentation references paths and types that exist in the current repository.
- The three allowed documentation files contain no contradictory claims about the active database.
- `git diff --check` passes for the allowed files.
- No files outside the allowed write scope are modified.

## Session B

**Branch name:** `agent/students-component-tests`

**Worktree directory:** `C:\BootyByBeighley-worktrees\students-component-tests`

**Task:** Add focused Angular unit tests for `StudentsComponent`. Cover its current observable behavior without refactoring or changing the component implementation.

**Files or folders the agent may write to:**

- `frontend/app/src/app/features/coach-dashboard/pages/students/students.component.spec.ts`

**Files or folders the agent may read but not write to:**

- `frontend/app/src/app/features/coach-dashboard/pages/students/students.component.ts`
- `frontend/app/src/app/features/coach-dashboard/pages/students/students.component.html`
- `frontend/app/src/app/features/coach-dashboard/pages/students/students.component.scss`
- `frontend/app/src/app/features/coach-dashboard/pages/activity-feed/activity-feed.component.spec.ts`
- `frontend/app/src/app/core/`
- `frontend/app/package.json`
- `frontend/app/angular.json`
- `frontend/app/tsconfig*.json`

**Commands the agent may run:**

```powershell
npm ci
npm test -- --watch=false --include=src/app/features/coach-dashboard/pages/students/students.component.spec.ts
```

Run npm commands from `frontend/app`. The agent may use read-only inspection commands and may rerun only the focused Students component test while iterating.

**Definition of done:**

- A new `students.component.spec.ts` exists in the Students page folder.
- Tests verify the request to `/api/v1/coach/students`.
- Tests cover successful roster rendering, the empty state, and the error state.
- Tests cover `getLastWorkoutDisplay` for no date and at least one relative-date case.
- Tests use the repository's existing Angular and Vitest testing patterns.
- The focused test command passes.
- No production source, generated API client, styling, or documentation files are modified.

## Session Review

After both sessions finish, compare each worktree against its scope contract:

```powershell
git -C C:\BootyByBeighley-worktrees\docs-postgresql-alignment status --short
git -C C:\BootyByBeighley-worktrees\students-component-tests status --short
```

Review each diff independently before merging either branch. Do not merge one session merely because the other succeeded.
