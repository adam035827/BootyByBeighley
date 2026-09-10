# Future Features for Agentic Engineering

This document identifies three well-scoped features for the LaunchCode agentic engineering course. These features have not yet been completed and provide opportunities for an agentic system to work across the backend, frontend, database, authorization, generated API client, and automated tests.

## 1. Student Workout Logging

Allow students to record completed workouts and review their workout history.

### Scope

- Add API endpoints to start, complete, and retrieve workout logs.
- Record movements, sets, repetitions, weight, completion time, and optional notes.
- Validate that repetitions and weight are non-negative and that completed workouts contain at least one set.
- Build a mobile-friendly Angular workout logging form and workout history view.
- Regenerate the NSwag TypeScript client after adding the endpoints and DTOs.
- Add command and query handler tests, Angular component tests, and a Playwright workflow.

### Acceptance Criteria

- An authenticated student can create and complete a workout log.
- A student can only view or modify their own workout logs.
- Invalid workout data returns structured ProblemDetails validation errors.
- A completed workout appears in the student's workout history.
- A completed workout appears in the coach activity feed.
- Loading, empty, error, and successful states are represented in the UI.

### Course Value

This feature is a focused vertical slice through every architecture layer. It also creates the workout data required by the existing coach activity feed and future progress features.

## 2. Coach Workout Plan Assignment

Allow coaches to create workout plans and assign them to students.

### Scope

- Add CRUD endpoints for workout plans and their exercises.
- Support ordered exercises with prescribed sets, repetitions, and optional load.
- Add an endpoint that assigns a plan to a student through a plan enrollment.
- Enforce coach-only authorization for plan management and assignment.
- Build Angular plan list, plan editor, and student assignment views.
- Regenerate the NSwag TypeScript client.
- Add tests for authorization, duplicate enrollment, missing students, and plan changes.

### Acceptance Criteria

- Only a coach can create, edit, delete, or assign workout plans.
- A coach can add, remove, edit, and reorder exercises in a plan.
- Assigning a plan creates an active enrollment for the selected student.
- Duplicate active assignments of the same plan are rejected.
- Students can view their active assigned plan but cannot modify it.
- Deleting or changing a plan with active enrollments follows an explicitly documented business rule.
- Unauthorized requests return an appropriate `401` or `403` response.

### Course Value

This feature exercises richer domain behavior, database relationships, and role-based authorization while remaining bounded. The existing `WorkoutPlans` and `PlanEnrollments` domain areas provide a natural starting point.

## 3. Personal Record Tracking and Progress Dashboard

Automatically identify personal records from completed workouts and display a student's progress over time.

### Scope

- Detect personal records when a workout is completed.
- Support records such as heaviest weight, estimated one-repetition maximum, and highest repetitions at a given weight.
- Add API queries for movement history and current personal records.
- Build a student progress dashboard with movement selection, summary metrics, and a progress chart.
- Allow coaches to view progress for their assigned students.
- Make record processing idempotent so repeated completion requests do not create duplicate records.
- Add calculation tests, query handler tests, Angular component tests, and an end-to-end test.

### Acceptance Criteria

- Completing a workout updates a personal record only when performance improves.
- Reprocessing the same workout does not create duplicate records.
- Record calculations are covered by unit tests, including boundary cases.
- Students can only see their own progress.
- Coaches can only view progress for students they are authorized to manage.
- The dashboard supports loading, empty, error, and populated states.
- Users can select a movement and see its performance history over time.

### Course Value

This feature introduces nontrivial and highly testable business logic rather than primarily CRUD behavior. The existing `PersonalRecords`, `Movements`, and `WorkoutLogs` domain areas make it a natural extension of the application.

## Recommended Implementation Order

1. Student Workout Logging
2. Coach Workout Plan Assignment
3. Personal Record Tracking and Progress Dashboard

Workout logging establishes the source data first. Plan assignment then connects prescribed programming to students. Personal record tracking can finally derive progress analytics from completed workout data.

Each feature can be divided among the project's orchestrator, backend implementer, frontend implementer, test writer, architecture reviewer, code reviewer, and documentation agent. This makes the work suitable for demonstrating planning, delegation, implementation, validation, review, and iteration in an agentic engineering workflow.

## 4. CI/CD Automated Testing Strategy (Module 4 Requirement)

Before code changes are accepted by the agentic system, automated checks must validate correctness across all layers.

### Existing Automated Checks

**1. Backend Build Validation**
- **Command:** `cd backend/src/ModernApp.Api && dotnet build`
- **What passes tell you:** C# code compiles without errors; NuGet dependencies resolve; domain models and API contracts are valid; Entity Framework migrations are correct
- **Current status:** Passing (18 warnings for known vulnerable dependencies, not blocking)

**2. Frontend Build Validation**
- **Command:** `cd frontend/app && npm run build`
- **What passes tell you:** TypeScript compiles without errors; Angular AOT compilation succeeds; no bundle bloat or regressions; generated NSwag client interfaces match usage
- **Current status:** Passing with no warnings

**3. Unit Tests (Angular + Vitest)**
- **Command:** `cd frontend/app && npm test`
- **What passes tell you:** Component logic works correctly; Signal-driven state management handles state transitions; RxJS subscription cancellation prevents race conditions; error handling and retry logic function as designed
- **Current status:** 7/7 tests passing for activity-feed component; test suite covers initial load, range switching, empty state, error with retry, and race-condition regressions

**4. End-to-End Tests (Playwright)**
- **Command:** `cd frontend/app && npx playwright test` (from `frontend/app` with backend running)
- **What passes tell you:** Full integration works end-to-end; frontend API routing is correct; filters and refresh buttons trigger correct backend calls; data flows through all layers without errors
- **Current status:** Passing test in `frontend/app/e2e/activity-feed.spec.ts` validates filter switching, refresh endpoint calls, and data rendering

### Recommended GitHub Actions Workflow

Create `.github/workflows/ci.yml` to run on all pull requests:

1. **Backend Validation** (always)
   - Restore and build .NET project
   - Report any compilation errors

2. **Frontend Validation** (always)
   - Install dependencies
   - Run TypeScript type check
   - Build Angular application
   - Run unit tests with Vitest
   - Report coverage changes

3. **Optional: Database Migrations** (when schema files change)
   - Verify migrations apply cleanly to a fresh database

4. **Optional: E2E Tests** (on-demand or nightly)
   - Start backend
   - Run Playwright tests
   - Longer running; may not run on every PR but should run before merge

### Acceptance Criteria for CI/CD Implementation

- All pull requests must pass backend build before merge
- All pull requests must pass frontend build and unit tests before merge
- The workflow reports which check failed and why (not generic "build failed")
- Developers can run the same checks locally before pushing (reproducible locally)
- The workflow completes in under 5 minutes for common PRs
- Flaky tests are identified and fixed or made deterministic

### Course Value

This demonstrates that the agentic system can verify code quality before accepting changes. It establishes a foundation for future modules where agents submit PRs and automated checks gate acceptance.