# Features

This document catalogs the features built in **Booty by Beighley**, organized by completion status.
For the full product definition and all planned features see `docs/APP.md`.

**Legend:**
- ✅ **Complete** — Feature fully implemented and tested
- 🟡 **Partial** — Core logic implemented, some aspects incomplete
- 🚧 **In Progress** — Skeleton/foundation only; significant work remaining
- 📋 **Planned** — Not yet started; roadmap feature from APP.md

---

## ✅ Complete Features

### Domain Features

### Workout Plans & Phases
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern
- **Description**: Coaches create multi-week workout programs. Each plan has linked phases, difficulty level, training frequency, and duration. Students select training days at enrollment. Plans can be draft or active; students can enroll in multiple plans simultaneously.
- **Key files**:
  - `src/BootyByBeighley.Domain/WorkoutPlans/WorkoutPlan.cs`
  - `src/BootyByBeighley.Application/Features/WorkoutPlans/` (commands & queries)
  - `src/BootyByBeighley.Infrastructure/Repositories/WorkoutPlanRepository.cs`
  - `src/BootyByBeighley.Api/Features/CoachEndpoints.cs` (MapWorkoutPlanEndpoint)

---

### Movements Library
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern and Azure Blob Storage integration
- **Description**: Reusable movement definitions with prescribed sets/reps/duration. Each movement stores a demonstration video URL in Azure Blob Storage. Movements are reused across multiple workouts and plans. Captions/transcripts are stored alongside video URLs for accessibility.
- **Key files**:
  - `src/BootyByBeighley.Domain/Movements/Movement.cs`
  - `src/BootyByBeighley.Application/Features/Movements/` (commands & queries)
  - `src/BootyByBeighley.Infrastructure/Repositories/MovementRepository.cs`
  - `src/BootyByBeighley.Infrastructure/Storage/IBlobStorageService.cs` (Azure Blob Storage abstraction)
  - `src/BootyByBeighley.Api/Features/CoachEndpoints.cs` (MapMovementEndpoints)

---

### Workouts
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern
- **Description**: Individual training sessions within a plan. Each workout has a name, description, estimated duration, and an ordered list of movements with prescribed sets, reps, and rest periods. Workouts can be reused across multiple plans.
- **Key files**:
  - `src/BootyByBeighley.Domain/WorkoutPlans/Workout.cs` (nested entity)
  - `src/BootyByBeighley.Infrastructure/Repositories/WorkoutRepository.cs`

---

### Workout Logging & Student Performance
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern
- **Description**: Students manually log workout completions with actual performance: sets performed, reps per set, weight used. Each log entry can include a personal note (private, student-only) and a feedback submission (difficulty 1–5 + optional comment visible to coach). Students can mark workouts as missed with optional reason. All log entries are retained permanently.
- **Key files**:
  - `src/BootyByBeighley.Domain/WorkoutLogs/WorkoutLog.cs`
  - `src/BootyByBeighley.Application/Features/WorkoutLogging/` (commands & queries)
  - `src/BootyByBeighley.Infrastructure/Repositories/WorkoutLogRepository.cs`
  - `src/BootyByBeighley.Api/Features/StudentEndpoints.cs` (logging endpoints)

---

### Personal Records (PRs)
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern and automatic detection
- **Description**: Tracks the heaviest weight ever logged for each movement per student across all time and plans. New PRs are automatically detected when a log entry saves a weight exceeding the previous best. PR data enables coach visibility and future in-app celebration screens.
- **Key files**:
  - `src/BootyByBeighley.Domain/PersonalRecords/PersonalRecord.cs`
  - `src/BootyByBeighley.Application/Features/PersonalRecords/` (commands & queries)
  - `src/BootyByBeighley.Infrastructure/Repositories/PersonalRecordRepository.cs`

---

### Plan Enrollment & Student Matching
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern and matching rule engine
- **Description**: Manages student enrollment in workout plans. Students can be enrolled in multiple plans simultaneously. Tracks enrollment status (active, paused, completed). Includes a rule-based matching system where coaches define explicit rules (e.g. "if goal = Tone up AND level = Beginner AND days = 3 → assign Plan A Phase 1"). Rules are evaluated in priority order; the first matching rule wins. If a student re-takes the questionnaire, they may progress to the next phase or switch plans entirely.
- **Key files**:
  - `src/BootyByBeighley.Domain/PlanEnrollments/PlanEnrollment.cs`
  - `src/BootyByBeighley.Domain/MatchingRules/MatchingRule.cs`
  - `src/BootyByBeighley.Application/Features/PlanEnrollment/` (commands & queries)
  - `src/BootyByBeighley.Infrastructure/Repositories/PlanEnrollmentRepository.cs`

---

### Questionnaire System
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern
- **Description**: Students complete a questionnaire after registration to determine their workout plan match. Questionnaire questions are coach-defined with fixed answer options (no free text) for full automation. Captures fitness level, primary goal, training frequency, injuries/limitations, and available equipment. Student responses are stored and can be re-taken at any time to change focus or progress through phases.
- **Key files**:
  - `src/BootyByBeighley.Domain/Questionnaires/Questionnaire.cs`
  - `src/BootyByBeighley.Domain/Questionnaires/QuestionnaireResponse.cs`
  - `src/BootyByBeighley.Application/Features/Questionnaires/` (if present)

---

### User Management
- **Status**: ✅ Complete
- **Layer**: backend (Domain + Application + Infrastructure + API)
- **Pattern**: Domain-driven design with repository pattern
- **Description**: Manages coach and student accounts. Supports user roles (Coach, Student) with different permissions and dashboard views. Students self-register and can be deactivated by the coach. Coach is a single account with full admin access. User profiles include metadata for future expansion (coach public profile, student preferences, etc.).
- **Key files**:
  - `src/BootyByBeighley.Domain/Users/User.cs`
  - `src/BootyByBeighley.Domain/UserRole.cs`
  - `src/BootyByBeighley.Application/Features/Users/` (commands & queries)
  - `src/BootyByBeighley.Infrastructure/Repositories/UserRepository.cs`
  - `src/BootyByBeighley.Api/Features/AuthEndpoints.cs` (registration, profile management)

---

## Architecture & Patterns

### Clean Architecture layer scaffold
- **Status**: ✅ Complete
- **Layer**: backend
- **Pattern**: Clean Architecture (Domain / Application / Infrastructure / API)
- **Description**: Four-project solution with enforced dependency direction. Domain has no external dependencies. Application depends only on Domain. Infrastructure implements Application interfaces. API wires everything together.
- **Key files**:
  - `src/BootyByBeighley.Domain/Common/Entity.cs`
  - `src/BootyByBeighley.Domain/Common/ValueObject.cs`
  - `src/BootyByBeighley.Application/Common/Interfaces/ICqrs.cs`
  - `src/BootyByBeighley.Application/DependencyInjection.cs`
  - `src/BootyByBeighley.Infrastructure/DependencyInjection.cs`
  - `src/BootyByBeighley.Api/Program.cs`

---

### CQRS interfaces
- **Status**: ✅ Complete
- **Layer**: backend — Application
- **Pattern**: CQRS via pure DI (no mediator library)
- **Description**: Marker interfaces `ICommand<T>`, `IQuery<T>`, `ICommandHandler<TCommand, TResponse>`, `IQueryHandler<TQuery, TResponse>` enforce Command/Query separation. Handlers are injected directly into Minimal API endpoints — no `ISender` or dispatch bus required. FluentValidation is not used — validation is done via .NET 10 native data annotations.
- **Key files**:
  - `src/BootyByBeighley.Application/Common/Interfaces/ICqrs.cs`

---

### PostgreSQL + EF Core infrastructure base
- **Status**: ✅ Complete
- **Layer**: backend — Infrastructure
- **Pattern**: Repository pattern over EF Core with Npgsql
- **Description**: `AppDbContext` provides the EF Core entry point for all database access. All entity configurations use Fluent API in `Infrastructure/Persistence/Configurations/`. Repository implementations live in `Infrastructure/Repositories/`. Schema is managed via EF Core migrations. The Cosmos DB infrastructure from the original template is no longer used by any active repository or DI registration; see `docs/INSTRUCTIONS.md` for the remaining unused template files pending removal.
- **Key files**:
  - `src/BootyByBeighley.Infrastructure/Persistence/AppDbContext.cs`
  - `src/BootyByBeighley.Infrastructure/DependencyInjection.cs`

---

### Azure AD B2C authentication
- **Status**: 🚧 In Progress (Skeleton only)
- **Layer**: backend — API
- **Pattern**: Stateless token-based auth via Azure AD B2C
- **Description**: JWT Bearer middleware is registered but NOT configured with Azure AD B2C issuer, authority, or audience. The frontend auth interceptor reads from `sessionStorage` with a TODO comment noting that proper token service integration is needed. Coach and Student endpoints have commented-out role-based authorization awaiting auth completion.
- **Key files**:
  - `src/BootyByBeighley.Api/Program.cs`
  - `src/app/core/interceptors/auth.interceptor.ts` (TODO comment)
  - `src/BootyByBeighley.Api/Features/CoachEndpoints.cs` (TODO comments for role-based auth)

---

### ProblemDetails error handling
- **Status**: ✅ Complete
- **Layer**: backend — API
- **Pattern**: RFC 9457 ProblemDetails
- **Description**: All API errors return structured ProblemDetails responses. The built-in .NET 10 exception handler middleware is enabled. Custom exception mappers can be added via `IExceptionHandler` implementations.
- **Key files**:
  - `src/BootyByBeighley.Api/Program.cs`
  - `src/BootyByBeighley.Api/Middleware/ExceptionHandlerExtensions.cs`

---

### NSwag API client generation
- **Status**: ✅ Complete
- **Layer**: cross-cutting (backend → frontend)
- **Pattern**: Code generation from OpenAPI spec
- **Description**: The backend exposes an OpenAPI document via `app.MapOpenApi()`. NSwag reads that spec and generates strongly typed TypeScript client classes and DTO interfaces into `frontend/app/src/app/core/api/`. Feature code imports those generated clients — no handwritten `HttpClient` services or DTO interfaces for backend data.
- **Key files**:
  - `nswag.json` — NSwag configuration (input: OpenAPI spec URL, output: `core/api/`)
  - `src/BootyByBeighley.Api/Program.cs` — `app.MapOpenApi()` exposes the spec
  - `src/app/core/api/` — generated output (do not edit manually)
- **Regeneration**: `nswag run nswag.json` from the repo root whenever a backend DTO or endpoint changes

---

## Frontend

### Angular 21 standalone app scaffold
- **Status**: ✅ Complete
- **Layer**: frontend
- **Pattern**: Standalone components, no NgModules
- **Description**: Angular 21 application with standalone components, SCSS, routing, and `HttpClient` wired with the auth interceptor. `OnPush` change detection is the default for all components.
- **Key files**:
  - `src/app/app.config.ts`
  - `src/app/app.routes.ts`

---

### SCSS design system
- **Status**: ✅ Complete
- **Layer**: frontend
- **Pattern**: Design tokens as CSS custom properties + BEM naming
- **Description**: All design values (colors, spacing, typography, shadows, z-index, transitions) are defined as CSS custom properties in `_tokens.scss`. Components consume tokens via `var(--token-name)` — no raw values in component SCSS.
- **Key files**:
  - `src/styles/_tokens.scss`
  - `src/styles/_reset.scss`
  - `src/styles/_typography.scss`
  - `src/styles.scss`

---

### Auth interceptor
- **Status**: ✅ Complete
- **Layer**: frontend — core
- **Pattern**: `HttpInterceptorFn`
- **Description**: Functional HTTP interceptor that attaches the JWT access token from `sessionStorage` to all outgoing API requests. Registered in `app.config.ts` via `provideHttpClient(withInterceptors([...]))`.
- **Key files**:
  - `src/app/core/interceptors/auth.interceptor.ts`
  - `src/app/app.config.ts`

---

### ProblemDetails error service
- **Status**: ✅ Complete
- **Layer**: frontend — core
- **Pattern**: Injectable service
- **Description**: Parses `ProblemDetails` responses from the backend and converts them into user-friendly messages and field-level error maps. Used in feature services and components to display error state.
- **Key files**:
  - `src/app/core/services/error.service.ts`
  - `src/app/core/models/problem-details.model.ts`

---

### Feature-based folder structure
- **Status**: ✅ Complete
- **Layer**: frontend
- **Pattern**: Feature modules with lazy loading
- **Description**: The `features/` directory holds one subfolder per domain feature, each with its own component, service, routes, and SCSS. Features are lazy-loaded via `loadComponent` / `loadChildren` to keep the initial bundle small.
- **Key files**:
  - `src/app/features/` (placeholder — features added per fork)
  - `src/app/shared/index.ts`

---

### Coach activity feed
- **Status**: 🟡 Partial (Frontend complete, backend incomplete)
- **Layer**: frontend — coach dashboard / backend
- **Pattern**: Signal-driven generated API client integration
- **Description**: Frontend component is fully implemented with Signal state, loading/error handling, and 7/30-day filtering. However, the backend `GetRecentActivity` endpoint exists but is not protected by role-based authorization (commented out in CoachEndpoints.cs pending auth system completion).
- **Key files**:
  - `src/app/features/coach-dashboard/pages/activity-feed/activity-feed.component.ts` (✅ complete)
  - `src/app/features/coach-dashboard/pages/activity-feed/activity-feed.component.html` (✅ complete)
  - `src/BootyByBeighley.Api/Features/CoachEndpoints.cs` (🚧 role-based auth commented out)

---

## 🟡 Partially Complete Features

### Azure AD B2C Authentication
- **Status**: 🚧 In Progress (Skeleton only)
- **Layer**: backend — API / frontend — core
- **Pattern**: Stateless token-based auth via Azure AD B2C
- **Description**: JWT Bearer middleware is registered but NOT configured with Azure AD B2C issuer, authority, or audience. The frontend auth interceptor reads from `sessionStorage` with a TODO comment noting that proper token service integration is needed. Coach and Student endpoints have commented-out role-based authorization awaiting auth completion. **Before production use**, you must:
  1. Configure Azure AD B2C in `Program.cs` with proper issuer, authority, and audience
  2. Implement token acquisition in the frontend (login/logout flows)
  3. Uncomment and wire up role-based authorization in all protected endpoints
  4. Replace `sessionStorage` token access with a proper token service
- **Key files**:
  - `src/BootyByBeighley.Api/Program.cs`
  - `src/app/core/interceptors/auth.interceptor.ts` (TODO comment)
  - `src/BootyByBeighley.Api/Features/CoachEndpoints.cs` (TODO comments for role-based auth)
- **See**: `docs/INSTRUCTIONS.md` for required secrets

---

## 📋 Planned Features (from APP.md)

### Self-Registration & User Signup
- **Status**: 📋 Planned
- **Layer**: backend + frontend
- **Description**: Students create their own accounts via Azure AD B2C registration flow. Frontend signup page guides new users through account creation and initial questionnaire.
- **Depends on**: Azure AD B2C authentication setup (currently 🚧 in progress)

---

### Coach Admin Interface — Content Management
- **Status**: 📋 Planned
- **Layer**: frontend — coach-dashboard
- **Description**: Dedicated UI for the coach to create, edit, and publish workout plans, workouts, movements, and matching rules. Includes draft/active state management.
- **Components needed**:
  - Plan builder (name, phases, difficulty, frequency, duration)
  - Workout builder (add movements, set reps/sets/rest)
  - Movement editor (name, description, video URL, captions)
  - Matching rules editor (rule builder with AND/OR logic)

---

### Coach Admin Interface — Video Upload
- **Status**: 📋 Planned
- **Layer**: backend + frontend
- **Description**: Dedicated upload page in the coach admin for movement demonstration videos. Videos are stored in Azure Blob Storage. Coach defines caption text (shown as subtitles during playback). Handles video encoding/transcoding setup (TBD).
- **Key backend entity**: `Movement` already has `VideoUrl` and `CaptionText` fields ready

---

### Coach Admin Interface — Student Log View
- **Status**: 📋 Planned
- **Layer**: frontend — coach-dashboard
- **Description**: Detailed per-student view showing workout history, logged sets/reps/weights, personal notes, feedback submissions, and missed workouts. Enables coach to track student progress.

---

### Coach Profile Management & Display
- **Status**: 📋 Planned
- **Layer**: backend + frontend
- **Description**: Coach profile page (visible to all students in-app) with photo, bio/about text, and optional social links. Coach edits this via admin interface; students view it on a dedicated "About Coach" screen.

---

### Student Home Screen
- **Status**: 📋 Planned
- **Layer**: frontend
- **Description**: Primary student interface with three sections: (1) Today's Workout (hero), (2) Weekly Calendar (week view with completed/missed status), (3) My Plans (current enrollments with progress). Built with Signals for reactive state management.
- **Related features**: Plan completion & auto-renewal, Student day selection, Student progress tracking

---

### Student Workout Execution & Video Playback
- **Status**: 📋 Planned
- **Layer**: frontend
- **Description**: Step-through UI for executing a workout. Students see each movement, can watch demonstration video (with captions, respecting background audio setting), log actual sets/reps/weight, and adjust prescribed sets on the fly.
- **Related features**: Movement library, Background audio setting, Workout logging

---

### Student Progress Tracking
- **Status**: 📋 Planned
- **Layer**: frontend
- **Description**: Dashboard showing full workout history, completion streaks, total workouts logged, and personal records across all plans. Visual progress indicators and filters by plan/date range.

---

### PR Celebration Screen
- **Status**: 📋 Planned
- **Layer**: frontend
- **Description**: In-app celebration screen shown to student when a new personal record is detected. Displays branded congratulatory image (uploaded by coach, applied globally to all students). Triggered automatically after workout log save.

---

### Plan Completion & Auto-Renewal Logic
- **Status**: 📋 Planned
- **Layer**: backend
- **Description**: **2 weeks before a plan ends**, prompt student to re-take questionnaire and choose what comes next. If student does not act, the plan **auto-renews** (restarts from week 1) at the end date. Student can change plan at any time regardless of progress.
- **Backend work**: Add scheduled job or cron trigger to send renewal prompts

---

### Student Day Selection Management
- **Status**: 📋 Planned
- **Layer**: backend + frontend
- **Description**: Allow students to select/change their preferred training days at enrollment or anytime after. Must maintain same frequency as the plan (e.g., a 3-day/week plan requires exactly 3 days selected). Selection drives home screen schedule and enables future push notifications.
- **Related entities**: `PlanEnrollment` already has `SelectedTrainingDays` field ready

---

### Background Audio Setting
- **Status**: 📋 Planned
- **Layer**: frontend
- **Description**: Per-student setting (default ON) to allow background audio (Spotify, Apple Music, podcasts) to continue during video playback. When ON: video plays muted and captions are primary; when OFF: dubbed coaching audio plays and background audio is interrupted.
- **Capacitor integration**: Requires audio session control on iOS/Android

---

### Offline Workout Download
- **Status**: 📋 Planned
- **Layer**: frontend + Capacitor
- **Description**: Allow students to download a workout for offline gym use. Downloaded package includes all movement data and video references. Video streaming offline is TBD in separate design.
- **Note**: Deferred feature; should not block v1 architecture

---

### Smartwatch Health Sync (v2)
- **Status**: 📋 Planned (v2 release)
- **Layer**: frontend + Capacitor
- **Description**: When a student completes a workout, app pulls heart rate and calories burned from HealthKit (iOS) or Health Connect (Android). Attached to workout log entry. No real-time streaming; read-only data pull at completion only. No watch companion app required.
- **Domain readiness**: `WorkoutLog` entity already has optional fields for `HeartRateAvgBpm`, `HeartRateMaxBpm`, `CaloriesBurned`

---

### Social Login — Google & Apple (v2)
- **Status**: 📋 Planned (v2 release)
- **Layer**: backend — Azure AD B2C config
- **Description**: Enable social login via Google and Apple identity providers. Configured in Azure AD B2C without backend code changes. Added together (Apple App Store requires Sign in with Apple if any third-party login is available).
- **Note**: Requires full Azure AD B2C setup first (currently 🚧 in progress)

---

### Payments & Subscription Gating (v2)
- **Status**: 📋 Planned (v2 release)
- **Layer**: backend + frontend
- **Description**: Stripe integration for student subscriptions. Students require active subscription to access plan content. Enforced at endpoint level via role/status check.
- **Domain readiness**: `User` entity already has `SubscriptionStatus` field; all users default to `Active` in v1

---

### Mobile App Distribution (Capacitor)
- **Status**: 📋 Planned
- **Layer**: Capacitor wrapper + build pipeline
- **Description**: Wrap Angular frontend with Capacitor for iOS App Store and Google Play Store distribution. Enables access to native device APIs (audio session, push notifications in v2, HealthKit/Health Connect in v2).

---

### Azure CDN for Video Delivery (v2)
- **Status**: 📋 Planned (v2 release)
- **Layer**: Infrastructure
- **Description**: Layer Azure CDN over Blob Storage for global video delivery performance and reduced egress costs.

---

### Push Notifications (Out of Scope for v1)
- **Status**: 📋 Out of Scope
- **Description**: Explicitly deferred. Reminder system for scheduled workouts will not be built in v1. Push/email notification infrastructure can be added in v2 without schema changes.

---

## 🚧 In Progress / Blockers

### ⚠️ Authentication System
The authentication infrastructure is scaffolded but **NOT fully implemented**. This is a blocker for:
- Self-registration
- Role-based authorization (Coach/Student endpoints are commented out)
- Social login
- All frontend auth flows

**Unblock path**: Complete Azure AD B2C configuration in `Program.cs` and frontend login flow implementation
