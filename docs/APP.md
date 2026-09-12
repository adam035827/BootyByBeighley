# Booty by Beighley — App Definition

This document is the **product source of truth** for the Booty by Beighley application.
All features, agent work, and architectural decisions must align with this definition.

---

## Overview

**Booty by Beighley** is a fitness instructing platform built around a single coach and her students.
The coach creates and manages workout content through an admin interface; students consume that content through a **mobile-first experience** optimized for iOS and Android.

The app will be distributed via the **Apple App Store and Google Play Store** as its primary delivery channels. The Angular frontend is wrapped with **Capacitor** for native app store submission. All UI decisions prioritize the **mobile experience first**; desktop is a secondary concern.

---

## Branding

### Current Theme: White & Vibrant Red

The app uses a **white background with vibrant red accents** as the primary brand colors.

| Element | Color | Token |
|---|---|---|
| App name | Booty by Beighley | — |
| Primary color | Vibrant Red `#ff1744` | `--color-primary` |
| Background | White `#ffffff` | `--color-background` |
| Surface | Light Gray `#f5f5f5` | `--color-surface` |
| Text | Dark `#212121` | `--color-text-primary` |
| Logo | TBD | Use a text wordmark as fallback |

### Design Tokens & Theming

All colors, typography, spacing, and layout are defined as **CSS custom properties** in `frontend/app/src/styles/_theme.scss` and `frontend/app/src/styles/_tokens.scss`.

**To change the theme:**
- Edit the theme variables in `_theme.scss`
- All component colors update automatically
- See [THEMING.md](THEMING.md) for full documentation on creating and managing themes

---

## User Roles

### Coach (Admin)
- Single account — there is one coach.
- Full admin access to the backend CMS-style interface.
- Can create, edit, publish, and archive workout plans and individual workouts.
- Defines the **questionnaire matching rules** that determine which plan and phase a student receives.
- Can upload movement demonstration videos via a dedicated **video upload page** in the admin.
- Can view **individual student workout logs** — sets, reps, weight, notes, and completion history per student.
- Can manage student accounts (deactivate).
- Can view student progress and engagement.

### Student
- The primary end-user audience.
- **Self-registers** — no coach invite required.
- Account defaults to **active** status; a subscription/payment gate will be added in a future phase (see Payments below).
- On first login, completes an **onboarding questionnaire** (see below) — answers determine which workout plan(s) are matched to them.
- Can be enrolled in **multiple plans simultaneously** (e.g. a strength plan and a conditioning plan at the same time).
- Logs workout completions, sets, reps, and weight for each movement.
- Can **leave personal notes** on any completed workout session.
- Can **submit feedback** on a workout (difficulty rating + optional comment visible to the coach).
- Can re-take the questionnaire at any time to change focus or progress to the next phase.
- All historical workout data is retained when changing or adding plans.
- Can watch demonstration videos for each movement within a workout.
- Has no access to admin/content-creation screens.

---

## Onboarding Questionnaire

New students complete a questionnaire after registration. Their answers drive which workout plan is matched to them. The questionnaire is designed by the coach and can be updated over time.

**Captured data points (initial set — subject to expansion):**

> All questionnaire questions use **fixed answer options** (no free text) so that matching can be fully automated.

| Question | Answer options (indicative — coach configures) | Purpose |
|---|---|---|
| Current fitness/experience level | Beginner / Intermediate / Advanced | Match plan difficulty |
| Primary goal | Tone up / Lose weight / Build strength / Improve endurance | Match plan outcome focus |
| How many days per week can you train? | 2 / 3 / 4 / 5 | Match plan frequency |
| Any injuries or limitations? | None / Upper body / Lower body / Back / Multiple | Flag for coach review |
| Equipment available | No equipment / Resistance bands / Dumbbells / Full gym | Filter appropriate movements |

**Plan phases:**
- Each workout plan has one or more **phases** (e.g. Phase 1, Phase 2, Phase 3).
- The coach publishes new phases on a rolling basis throughout the year.
- A student progresses through phases sequentially within a plan.

**Matching logic:**
- Matching is **rule-based** — the coach defines explicit rules in the admin (e.g. "if goal = Tone up AND level = Beginner AND days = 3 → assign Plan A Phase 1").
- Rules are evaluated in priority order; the first matching rule wins.
- The best-matching active plan and phase are presented to the student.
- If a student re-takes the questionnaire and their answers match a plan they are already on, they are advanced to the **next phase** of that plan rather than restarting.
- If the answers point to a different plan, the student is enrolled in the new plan (previous plan history is retained).
- If no rule matches, the coach is notified to review and can manually enroll the student.
- Students can opt into a new plan or phase at any time by re-taking the questionnaire.

---

## Core Concepts

### Workout Plan
A structured, multi-week program created by the coach. Each plan is a **distinct object** with its own goals, difficulty, and content — phases are separate plan objects linked in a progression sequence.
- Has a name, description, difficulty level, and duration (in weeks).
- Linked to a **next phase plan** (optional) so the system knows what follows when a student completes it.
- Specifies a **training frequency** (days per week — 2 through 5) so the schedule matches the student's availability.
- When a student enrolls, they **select which specific days of the week** they will train (e.g. Mon / Wed / Fri). This drives the home screen schedule and enables future push notifications on those days.
- Students can **change their selected training days** at any time, provided the new selection has the same number of days as the plan requires. If they want a different frequency, they must switch to a plan that matches.
- Contains an ordered list of **Workouts** mapped to day slots within the week.
- Can be in draft (coach-only) or active (visible to matched students) state.
- A student can be enrolled in multiple plans simultaneously.

**Plan completion behavior:**
- **2 weeks before a plan ends**, the student receives a prompt to re-take the questionnaire and choose what comes next.
- If the student does not act, the **current plan auto-renews** (restarts from week 1) at the end date.
- The student can change their plan at any time regardless of where they are in the cycle.

### Workout
A single training session within a plan.
- Has a name, description, and estimated duration.
- Contains an ordered list of **Movements** with prescribed sets, reps, and rest periods.
- Can be reused across multiple plans.

### Workout Log Entry
A student's record of completing a workout.
- Created when a student **manually marks a workout as complete**.
- Stores the date completed and the student's **actual performance per movement**: sets performed, reps per set, and weight used.
- Students may adjust sets from the prescribed amount — the log records what they actually did, not just what was prescribed.
- Students can attach a **personal note** to the log entry (private, visible only to the student).
- Students can submit **workout feedback** (difficulty rating 1–5 + optional comment) that is visible to the coach.
- All log entries are retained permanently; history is never deleted when a student changes plans.

### Missed Workout
- Students can explicitly **mark a workout as missed**.
- A missed workout is recorded in the log with a `Missed` status and an optional reason.
- Missed workouts do not auto-reschedule in v1 — the student simply continues from the next scheduled workout.
- The coach can see missed workouts per student in the individual log view.
- A catch-up / rescheduling policy will be defined in a future iteration.

### Personal Record (PR)
- A PR is the **heaviest weight ever logged for a specific movement** by a student, across all time and all plans.
- A new PR is automatically detected the moment a student saves a log entry with a weight exceeding their previous best for that movement.
- When a new PR is detected, a **PR celebration screen** is shown to the student in-app.
- The coach can upload a single **branded congratulatory image** that appears on the celebration screen for every PR event. This image is managed in the admin and applies globally.
- PRs are tracked per movement, not per workout or plan.

### Offline Workout Download *(future feature)*
- Students will be able to download a workout to their device for use without a network connection (e.g. at the gym).
- Downloaded workouts include all movement data and video references; video streaming offline is TBD.
- This feature is deferred to a later release and should not block v1 architecture decisions.

### Movement
An individual movement or drill (referred to in the UI as a "movement", not "exercise").
- Has a name, description, and **demonstration video** stored in **Azure Blob Storage** (URL reference stored in PostgreSQL).
- Defined with prescribed sets, reps, duration, or distance.
- Lives in a reusable movement library — add once, use across many workouts.
- Students can tap/click a movement to watch the demonstration video before or during a workout.
- When the coach replaces a video on a movement, **all students see the updated video immediately** — there is no per-student video versioning.

**Movement video requirements:**
- Each video has **dubbed audio** — a voiceover coaching track recorded by the coach.
- The full text of the dub is stored as a **caption/transcript** alongside the video and displayed on screen while the video plays (styled as subtitles).
- The app **must not interrupt the student's background audio** (Spotify, Apple Music, podcasts, etc.) when a video plays.
  - This is controlled by a **user setting: "Allow background audio during videos"**, defaulted to **ON**.
  - When ON: the video plays muted and caption text is the primary coaching delivery method.
  - When OFF: the dubbed coaching audio plays and background audio is interrupted as normal.
- The caption text must always be visible regardless of the audio setting.

**Video storage architecture:**
- Videos uploaded via the coach admin interface are stored in **Azure Blob Storage** (existing Azure account).
- Blob URLs are stored in **PostgreSQL** on the `Movement` table via EF Core.
- No maximum file size or duration is enforced by the app — the coach is responsible for keeping uploads reasonable.
- Azure CDN will be layered on top in a future phase for global delivery performance and reduced egress cost.

## Coach Home Screen

The coach's home screen is tabbed with three sections:

1. **Students** — full roster with each student's active plans, last workout date, and a flag for any pending feedback or unread PR achievements.
2. **Content** — content management dashboard: plans, workouts, movement library, matching rules, and video uploads.
3. **Activity Feed** — a chronological feed of recent student completions, feedback submissions, missed workouts, and new PRs across all students.

## Coach Profile

The coach has a public-facing profile page visible to all students inside the app.
- Contains a **photo**, **bio/about text**, and optional **social links**.
- Managed by the coach via the admin interface.
- Displayed on a dedicated "About" or "Your Coach" screen in the student-facing app.

---

## Student Home Screen

The student's home screen has three sections, with **Today's Workout as the primary focus**:

1. **Today's Workout** (hero section) — the scheduled workout(s) for today based on the days the student selected at enrollment. **If multiple enrolled plans have a workout on the same day, all are shown.** One-tap to start each. If nothing is scheduled today, shows the next upcoming workout.
2. **Weekly Calendar** — a compact week view showing the student's chosen training days, which are complete, and which were missed.
3. **My Plans** — a list of all plans the student is currently enrolled in, with progress indicators.

**Timezone**: All scheduling uses the **student's device timezone**. Dates are stored in UTC and converted to local time on the client.

---

## Authentication

- **Provider**: **Azure AD B2C**
  - Issues standard JWTs consumed by the existing JWT Bearer middleware in the backend.
  - Free tier covers up to 50,000 monthly active users.
  - Built-in support for Google and Apple as identity providers — social login can be enabled via configuration when required, with no backend code changes.
  - Handles self-service registration, login, password reset, and token refresh out of the box.
- **Roles**: A custom claim (`role`) in the JWT distinguishes `Coach` from `Student`. The `Coach` role is a **database flag** — not hardcoded to a specific email — so additional coaches can be granted access in the future without code changes.
- **Social login** (Google + Apple): planned for a future release. Note — Apple App Store requires **Sign in with Apple** to be offered if any third-party social login is available; Google and Apple must therefore be added together.
- **v1**: Email/password login only via Azure AD B2C local accounts.

---

## High-Level Feature Areas

> Details for each area will be expanded in `docs/FEATURES.md` as they are designed and built.

| Area | Summary | Status |
|---|---|---|
| Authentication (Azure AD B2C) | Email/password via Azure AD B2C; JWT role claims; social login ready for v2 | Planned |
| Self-registration | Students create their own accounts via Azure AD B2C | Planned |
| Onboarding questionnaire | Fixed-option questions; answers drive automated plan matching | Planned |
| Questionnaire matching rules | Coach defines explicit rules in admin; first match wins | Planned |
| Plan completion & auto-renewal | 2-week warning prompt; auto-renews if student takes no action; changeable at any time | Planned |
| Student day selection | Student picks specific training days at enrollment; drives schedule and future notifications | Planned |
| Movement library | Coach-managed reusable movements with Azure Blob-hosted demo videos | Planned |
| Video upload (coach) | Dedicated upload page in admin; stores to Azure Blob Storage | Planned |
| Coach admin — content management | Create/edit/publish plans, workouts, movements, matching rules | Planned |
| Coach admin — student log view | View individual student workout logs, feedback, missed workouts | Planned |
| Student — home screen | Today's workout (hero) + weekly calendar + my plans | Planned |
| Student — multi-plan enrollment | Students enrolled in multiple plans simultaneously | Planned |
| Student — workout execution | Step through workout, watch captioned videos, log sets/reps/weight, adjust sets | Planned |
| Workout notes | Students attach private notes to completed workout log entries | Planned |
| Workout feedback | Students rate difficulty (1–5) and leave a comment visible to the coach | Planned |
| Missed workout tracking | Students mark workouts missed; logged with optional reason; no auto-reschedule in v1 | Planned |
| Student — progress tracking | Full history, streaks, and PRs (heaviest weight per movement) | Planned |
| PR celebration | Coach uploads a branded image shown to the student when a new PR is hit | Planned |
| Coach profile | Coach photo, bio, and social links; visible to students on a dedicated screen | Planned |
| Coach home screen | Tabbed: Students roster / Content management / Activity feed | Planned |
| Workout download (offline) | Students download a workout for offline gym use | Planned (later) |
| Background audio setting | Per-student setting (default ON) to keep Spotify/Apple Music playing during videos | Planned |
| Smartwatch health sync | Pull heart rate + calories from HealthKit / Health Connect at workout completion | Planned (v2) |
| Social login (Google + Apple) | Added together (App Store requirement); Azure AD B2C identity providers | Planned (v2) |
| Payments | Stripe integration; student subscription gating access | Planned (v2) |
| Notifications | Push/email reminders for scheduled workouts | Out of scope (v1) |
| Mobile app distribution | Capacitor wrapper for iOS App Store and Google Play Store | Planned |
| Azure CDN for video delivery | CDN layer over Blob Storage for performance and egress cost | Planned (v2) |
| Branding & theming | Apply final brand tokens once brand guide is delivered | Placeholder |

---

## What This App Is Not (initial scope)

- Not a marketplace — there is one coach, not multiple trainers in v1. The `Coach` role is a DB flag so additional coaches can be added in the future.
- Not a social platform — no student-to-student interaction in v1.
- Not a live-streaming app — all content is pre-recorded or text/image based.
- Not a subscription billing platform in v1 — Stripe integration is planned for v2; all accounts are active by default until then.
- Notifications (push/email) are explicitly out of scope for v1 — no reminder system will be built initially.

---

## Smartwatch Integration (v2)

**Scope**: Read-only health data sync — no companion watch app required.

- When a student completes a workout, the app pulls **heart rate** and **calories burned** data from the device's health platform and attaches it to the `WorkoutLogEntry`.
- **iOS**: [Apple HealthKit](https://developer.apple.com/health-fitness/) via a Capacitor HealthKit plugin.
- **Android**: [Google Health Connect](https://developer.android.com/health-and-fitness/guides/health-connect) via a Capacitor Health Connect plugin.
- Both APIs require explicit user permission grants — the app must request authorization on first use.
- Data is **pulled at workout completion**, not streamed in real time in v1 of this feature.
- The `WorkoutLogEntry` domain entity must include optional fields for `heartRateAvgBpm`, `heartRateMaxBpm`, and `caloriesBurned` from day one so they can be populated without a schema migration in v2.
- No native watchOS or Wear OS companion app is required for this scope; the phone app handles all HealthKit / Health Connect calls.

**What this is not (in this scope)**:
- No workout control from the watch face.
- No real-time heart rate display during the workout in v1.
- No GPS/distance tracking (not relevant to this workout style).

---

## Payments (v2)

- **Provider**: Stripe (or equivalent) — decision to be confirmed before v2 build begins.
- Students will require an active subscription to access plan content.
- The `User` domain entity must carry a subscription status field from day one so the gate can be enforced without a schema migration in v2.
- For v1 all users default to `SubscriptionStatus.Active`.

---

## Mobile Strategy

- The Angular frontend is wrapped with **Capacitor** for native iOS and Android app store distribution.
- All UI is designed mobile-first; desktop is supported but secondary.
- Capacitor gives access to native device APIs (audio session control for the background audio feature, push notifications in v2, etc.).
- The web PWA build is maintained alongside the Capacitor build for potential web access.

---

## Technical Alignment

This app is built on the **Booty by Beighley** already in this repo.
The template's original Cosmos DB infrastructure layer has been **replaced with PostgreSQL + EF Core** — the Domain and Application layers were unaffected by this change.

### Database
- **Azure Database for PostgreSQL — Flexible Server**
- Accessed via **EF Core** with the **Npgsql** provider
- The existing Azure PostgreSQL server (used by another app) will host this app's database until dedicated server capacity is needed
- All relational entities (plans, enrollments, log entries, PRs, etc.) are mapped as EF Core entities with proper foreign keys and indexes
- Migrations managed with `dotnet ef migrations`

### Layer mapping

| Concept | Layer |
|---|---|
| `WorkoutPlan`, `Workout`, `Movement`, `Questionnaire`, `QuestionnaireResponse`, `MatchingRule`, `PlanEnrollment`, `WorkoutLogEntry`, `WorkoutFeedback`, `MissedWorkout`, `PersonalRecord`, `User` | `BootyByBeighley.Domain` |
| EF Core `DbContext`, repositories, PostgreSQL configuration | `BootyByBeighley.Infrastructure` |
| Azure Blob Storage client (video upload/retrieval) | `BootyByBeighley.Infrastructure` |
| Azure AD B2C JWT Bearer validation | `BootyByBeighley.Api` |
| CQRS handlers for each feature | `BootyByBeighley.Application` |
| Minimal API endpoints | `BootyByBeighley.Api` |
| Angular feature modules | `frontend/app/src/app/features/` |

### Infrastructure migration note
`DependencyInjection.cs` registers the EF Core `AppDbContext` (via `UseNpgsql`) and no longer registers a `CosmosClient`. A handful of unused template files from the original Cosmos DB implementation (`CosmosDocument`, `TodoItemDocument`, `TodoItemMappings`) remain in `BootyByBeighley.Infrastructure` pending removal — none of them are referenced by the active repositories or DI registration.

The app name `BootyByBeighley` used throughout the codebase is a **placeholder** inherited from the template and will be renamed.
