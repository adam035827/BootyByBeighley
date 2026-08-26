# Phase 1 Implementation Specification: Domain, Database, CQRS & API

**Status**: Ready for Backend Implementer  
**Target**: Complete MVP domain model, database schema, CQRS handlers, and Minimal API endpoints  
**Scope**: Replaces TodoItems scaffold with fitness coaching domain  

---

## Overview

This spec defines the complete Phase 1 backend implementation for Booty by Beighley. It includes:
1. Domain entity definitions with aggregates and value objects
2. EF Core DbContext configuration and relationships
3. Database migration setup
4. CQRS command and query handlers
5. Minimal API endpoints with auth/authorization
6. Azure Blob Storage video upload integration

All work follows Clean Architecture boundaries and CQRS patterns defined in [Backend Implementer Agent](../.github/agents/backend-implementer.agent.md).

---

## 1. Domain Layer (`ModernApp.Domain`)

### 1.1 Core Entities

#### `User` (Aggregate Root)
Represents both Coach and Student accounts. Carries role and subscription status.

```
Properties:
- Id: Guid (PK)
- Email: string (unique index)
- Role: UserRole enum (Coach | Student)
- FirstName: string
- LastName: string
- CreatedAt: DateTime (UTC)
- SubscriptionStatus: SubscriptionStatus enum (Active | Suspended | Cancelled)
- IsCoach: bool (derived from Role)
- ProfilePhotoUrl: string? (for coach bio; optional)
- Bio: string? (for coach bio; optional)
- SocialLinks: string? (JSON or normalized structure; optional)

Validations:
- Email must be valid email format
- FirstName, LastName required and non-empty
- SubscriptionStatus defaults to Active

Notes:
- Designed for future multi-coach scaling (Coach role is DB flag, not hardcoded)
- v1: One user with Coach role exists; all others are Students
- Subscription fields added for v2 Stripe gating (default to Active in v1)
- Optional health data fields (heartRateAvgBpm, heartRateMaxBpm, caloriesBurned) added to
  support v2 smartwatch integration without migration
```

#### `WorkoutPlan` (Aggregate Root)
A structured multi-week program. Each phase is a separate plan linked to the next.

```
Properties:
- Id: Guid (PK)
- Name: string (required)
- Description: string
- Difficulty: Difficulty enum (Beginner | Intermediate | Advanced)
- DurationWeeks: int (required, 1-52)
- TrainingFrequencyDaysPerWeek: int (2-5)
- NextPhasePlanId: Guid? (FK to WorkoutPlan, nullable; optional link to progression)
- IsPublished: bool (draft vs active)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)
- Workouts: IReadOnlyList<Workout> (ordered list; navigation)

Validations:
- Name: required, max 255 chars
- DurationWeeks: 1-52
- TrainingFrequencyDaysPerWeek: 2-5
- NextPhasePlanId: if set, must reference a valid WorkoutPlan

Notes:
- FK to Coach/User? No — plans are created by THE coach. Implicit link via IsPublished flag.
  Future multi-coach version can add explicit coach FK.
- Workouts are ordered within the plan by a position/order field (see Workout entity)
- Plans are immutable once published (coach must create new version to edit)
- No explicit phase naming — phase structure is implicit in the linked chain
```

#### `Workout` (Entity)
A single training session within a plan.

```
Properties:
- Id: Guid (PK)
- WorkoutPlanId: Guid (FK to WorkoutPlan)
- Name: string (e.g., "Leg Day")
- Description: string?
- EstimatedDurationMinutes: int (e.g., 45)
- ScheduledDayOfWeek: DayOfWeek enum? OR Position: int (ordered within plan)
- Order: int (position within plan for display order)
- CreatedAt: DateTime (UTC)
- Movements: IReadOnlyList<WorkoutMovement> (ordered list)

Validations:
- Name: required, max 255 chars
- EstimatedDurationMinutes: 1-600
- Order: unique within WorkoutPlanId

Notes:
- DayOfWeek: Schedule is flexible; each student chooses their training days at enrollment.
  Workouts are not bound to specific days; students map them to their chosen days.
- Movements are ordered by position within the workout.
- Workouts can be reused across multiple plans (future version; for now, unique per plan)
```

#### `Movement` (Aggregate Root)
An individual exercise/movement definition with prescribed sets/reps and demo video.

```
Properties:
- Id: Guid (PK)
- Name: string (e.g., "Back Squat")
- Description: string?
- DefaultSets: int
- DefaultReps: int
- DefaultRestSeconds: int?
- VideoUrl: string? (Azure Blob Storage URL)
- VideoCaption: string? (full transcript/dub text shown as subtitles)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)

Validations:
- Name: required, max 255 chars
- DefaultSets: 1-20
- DefaultReps: 1-100
- DefaultRestSeconds: 0-300 (optional)
- VideoUrl: valid URI format (optional)
- VideoCaption: max 5000 chars (optional)

Notes:
- Movement library is reusable across workouts (coach defines once, use many times)
- When coach replaces video, all students see updated version immediately (no per-student versioning)
- Caption is mandatory when video is uploaded (voiceover coaching text)
- VideoUrl is stored as string; full signed URL generation happens in Infrastructure layer
```

#### `WorkoutMovement` (Value Object / Composite)
Link between Workout and Movement with prescribed intensity.

```
Properties:
- Id: Guid (PK)
- WorkoutId: Guid (FK to Workout)
- MovementId: Guid (FK to Movement)
- Order: int (position within workout)
- PrescribedSets: int (can differ from Movement.DefaultSets)
- PrescribedReps: int (can differ from Movement.DefaultReps)
- PrescribedRestSeconds: int? (can differ from Movement.DefaultRestSeconds)

Validations:
- Order: unique within WorkoutId
- PrescribedSets, PrescribedReps, RestSeconds: same ranges as Movement defaults

Notes:
- Acts as junction with customization; not a pure junction table
- Coach defines the exact sets/reps/rest for this workout, not defaulting to movement
```

#### `Questionnaire` (Aggregate Root)
Template for onboarding questions. Coach can update; students re-answer to progress.

```
Properties:
- Id: Guid (PK)
- Question: string (required)
- QuestionNumber: int (display order)
- AnswerOptions: string (JSON array of option strings)
- IsActive: bool (coach can archive old versions)
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)

Notes:
- Fixed-answer questions only; coach configures options
- No free-text responses (enables automated matching)
- Questionnaire is global (not per-student)
- Students answer after signup; can re-answer to change plan
- Matching rules evaluate questionnaire responses (see MatchingRule)
```

#### `QuestionnaireResponse` (Entity)
A student's answer to a questionnaire question at a point in time.

```
Properties:
- Id: Guid (PK)
- UserId: Guid (FK to User; student)
- QuestionnaireId: Guid (FK to Questionnaire)
- SelectedAnswer: string (one of Questionnaire.AnswerOptions)
- AnsweredAt: DateTime (UTC)

Validations:
- UserId: must be a Student (Role = Student)
- SelectedAnswer: must be in Questionnaire.AnswerOptions

Notes:
- Each response is timestamped; student can answer same question multiple times
- Latest response per question is used for matching
- Full history retained for audit/analytics
```

#### `MatchingRule` (Aggregate Root)
Coach-defined rule that evaluates questionnaire responses to assign plan + phase.

```
Properties:
- Id: Guid (PK)
- Priority: int (lower = higher priority; evaluated in order)
- RuleDefinition: string (JSON condition; TBD format for phase 1)
- TargetWorkoutPlanId: Guid (FK to WorkoutPlan; destination on match)
- IsActive: bool
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime (UTC)

Notes:
- v1: Coach manually reviews questionnaire responses and assigns plans via API
- Matching rule engine deferred to v2 (for now, rules stored but not evaluated)
- RuleDefinition format TBD; store as JSON for flexibility
- Example rule: "level = Beginner AND goal = Tone up AND daysPerWeek = 3"
```

#### `PlanEnrollment` (Aggregate Root)
Association of Student to WorkoutPlan with progress tracking.

```
Properties:
- Id: Guid (PK)
- UserId: Guid (FK to User; student) + unique index (UserId, WorkoutPlanId)
- WorkoutPlanId: Guid (FK to WorkoutPlan)
- EnrolledAt: DateTime (UTC)
- StartDate: DateTime (UTC; when student begins the plan cycle)
- PlannedEndDate: DateTime (UTC; calculated as StartDate + DurationWeeks)
- SelectedTrainingDays: string (JSON array of DayOfWeek enums; e.g. ["Monday", "Wednesday", "Friday"])
- Status: PlanEnrollmentStatus enum (Active | Completed | Archived)
- CompletedAt: DateTime? (when student finishes the plan)

Validations:
- SelectedTrainingDays: length must match WorkoutPlan.TrainingFrequencyDaysPerWeek
- StartDate, PlannedEndDate: valid date range
- Status progression: Active → Completed/Archived

Notes:
- Student can be enrolled in multiple plans simultaneously
- SelectedTrainingDays determined by student at enrollment; can change if plan allows
- Plans auto-renew 2 weeks before end if student takes no action (v1 MVP doesn't enforce this yet)
- 2-week warning prompt shown 2 weeks before PlannedEndDate (client-side logic)
```

#### `WorkoutLogEntry` (Aggregate Root)
Student's record of completing a workout session with actual performance.

```
Properties:
- Id: Guid (PK)
- UserId: Guid (FK to User; student)
- WorkoutId: Guid (FK to Workout)
- PlanEnrollmentId: Guid (FK to PlanEnrollment; for context)
- CompletedAt: DateTime (UTC)
- Notes: string? (private note from student; max 1000 chars)
- LoggedSets: List<WorkoutLogSetEntry> (actual performance per movement)
- Status: WorkoutStatus enum (Completed | Missed)
- MissedReason: string? (if Status = Missed)

Validations:
- CompletedAt: must be in the past
- If Status = Completed, LoggedSets must not be empty
- If Status = Missed, MissedReason optional but encouraged

Notes:
- Created when student manually marks workout complete (not auto-tracked)
- All log entries retained permanently; never deleted when student changes plans
- LoggedSets is a collection of per-movement performance details (see below)
```

#### `WorkoutLogSetEntry` (Value Object)
Details of a student's actual performance on a single movement within a log entry.

```
Properties:
- Id: Guid (PK)
- WorkoutLogEntryId: Guid (FK to WorkoutLogEntry)
- WorkoutMovementId: Guid (FK to WorkoutMovement)
- SetNumber: int (1-based; which set)
- RepsCompleted: int
- WeightUsed: decimal? (kg; null for bodyweight)
- DurationSeconds: int? (for timed movements)
- NotesPerMovement: string? (optional; logged at set level)

Validations:
- SetNumber: 1-20
- RepsCompleted: 0-1000
- WeightUsed: 0-500 (in kg)
- DurationSeconds: 0-3600

Notes:
- Each movement in a workout can have multiple sets logged
- Student can log fewer/more sets than prescribed
- WeightUsed is optional for bodyweight movements
- This is where PR detection happens: if WeightUsed > previous max for this student + movement, flag new PR
```

#### `PersonalRecord` (Aggregate Root)
The heaviest weight ever logged for a specific movement by a specific student.

```
Properties:
- Id: Guid (PK)
- UserId: Guid (FK to User; student)
- MovementId: Guid (FK to Movement)
- MaxWeightKg: decimal
- LoggedAt: DateTime (UTC; when PR was set)
- WorkoutLogSetEntryId: Guid? (FK to WorkoutLogSetEntry; source log entry)

Validations:
- MaxWeightKg: > 0

Notes:
- Auto-detected when WorkoutLogSetEntry is saved
- PR is per movement, per student, global across all plans
- When new PR detected, celebration screen shown to student (client logic)
- Coach can upload branded PR image (stored separately; TBD)
```

#### `WorkoutFeedback` (Entity)
Student feedback on a completed workout, visible to coach.

```
Properties:
- Id: Guid (PK)
- WorkoutLogEntryId: Guid (FK to WorkoutLogEntry; unique)
- UserId: Guid (FK to User; student)
- DifficultyRating: int (1-5 scale)
- Comment: string? (optional; max 500 chars)
- SubmittedAt: DateTime (UTC)

Validations:
- DifficultyRating: 1-5
- Comment: max 500 chars
- WorkoutLogEntryId: unique (one feedback per log entry)

Notes:
- Only created if student explicitly submits feedback
- Visible to coach; supports content refinement
```

#### `MissedWorkout` (Entity)
Log entry for a workout the student explicitly marked as missed.

```
Properties:
- Id: Guid (PK)
- WorkoutLogEntryId: Guid (FK to WorkoutLogEntry; when Status = Missed)
- UserId: Guid (FK to User; student)
- WorkoutId: Guid (FK to Workout)
- MissedDate: DateTime (UTC)
- MissedReason: string? (optional; "Too busy", "Injured", "Travel", etc.)
- Notes: string? (optional context)

Validations:
- MissedDate: in the past
- MissedReason: optional but encouraged

Notes:
- Created alongside WorkoutLogEntry with Status = Missed
- No auto-rescheduling in v1; coach sees missed count per student
- Future: catch-up policy will reschedule missed workouts
```

---

### 1.2 Value Objects

#### `UserRole` (Enum)
```csharp
public enum UserRole
{
    Coach = 1,
    Student = 2
}
```

#### `SubscriptionStatus` (Enum)
```csharp
public enum SubscriptionStatus
{
    Active = 1,
    Suspended = 2,
    Cancelled = 3
}
```

#### `Difficulty` (Enum)
```csharp
public enum Difficulty
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3
}
```

#### `PlanEnrollmentStatus` (Enum)
```csharp
public enum PlanEnrollmentStatus
{
    Active = 1,
    Completed = 2,
    Archived = 3
}
```

#### `WorkoutStatus` (Enum)
```csharp
public enum WorkoutStatus
{
    Completed = 1,
    Missed = 2
}
```

---

## 2. Infrastructure Layer (`ModernApp.Infrastructure`)

### 2.1 EF Core DbContext

Create `ModernApp.Infrastructure/Persistence/AppDbContext.cs` with:

```
DbSets:
- DbSet<User> Users
- DbSet<WorkoutPlan> WorkoutPlans
- DbSet<Workout> Workouts
- DbSet<WorkoutMovement> WorkoutMovements
- DbSet<Movement> Movements
- DbSet<Questionnaire> Questionnaires
- DbSet<QuestionnaireResponse> QuestionnaireResponses
- DbSet<MatchingRule> MatchingRules
- DbSet<PlanEnrollment> PlanEnrollments
- DbSet<WorkoutLogEntry> WorkoutLogEntries
- DbSet<WorkoutLogSetEntry> WorkoutLogSetEntries
- DbSet<PersonalRecord> PersonalRecords
- DbSet<WorkoutFeedback> WorkoutFeedback
- DbSet<MissedWorkout> MissedWorkouts

Key Configuration:
- Entity relationships (FKs, cascading deletes)
- Unique indexes: User.Email, PlanEnrollment (UserId, WorkoutPlanId)
- Ordering: Workout.Order, WorkoutMovement.Order, WorkoutLogSetEntry.SetNumber
- Timestamp indexes: CreatedAt, UpdatedAt, CompletedAt, AnsweredAt, LoggedAt
```

### 2.2 Repository Pattern (Optional but Recommended)

If using repositories, define in `ModernApp.Infrastructure/Repositories/`:
- `IUserRepository` / `UserRepository`
- `IWorkoutPlanRepository` / `WorkoutPlanRepository`
- `IPlanEnrollmentRepository` / `PlanEnrollmentRepository`
- `IWorkoutLogRepository` / `WorkoutLogRepository`

Or: Query directly via DbContext in handlers (CQRS queries naturally map to EF queries).

### 2.3 Azure Blob Storage Client

Create `ModernApp.Infrastructure/Storage/IBlobStorageService.cs` interface:
```csharp
public interface IBlobStorageService
{
    Task<string> UploadVideoAsync(string containerName, string blobName, 
        Stream fileStream, CancellationToken ct);
    Task<Uri> GetSignedUrlAsync(string containerName, string blobName, 
        int expirationMinutes = 60, CancellationToken ct = default);
    Task DeleteAsync(string containerName, string blobName, CancellationToken ct);
}
```

Implement in `ModernApp.Infrastructure/Storage/AzureBlobStorageService.cs`:
- Use `Azure.Storage.Blobs.BlobContainerClient` from `Azure.Storage.Blobs` NuGet
- Generate signed URLs with SAS token (read-only for student views)
- Handle upload errors gracefully (ProblemDetails response)

### 2.4 Dependency Injection

Update `ModernApp.Infrastructure/DependencyInjection.cs`:
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    // EF Core + PostgreSQL
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<IPlanEnrollmentRepository, PlanEnrollmentRepository>();
    // ... etc

    // Azure Blob Storage
    var blobConnectionString = configuration.GetConnectionString("AzureBlobStorage");
    services.AddSingleton(new BlobContainerClient(
        new Uri(configuration["Azure:BlobStorage:ContainerUri"]),
        new DefaultAzureCredential()));
    services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

    return services;
}
```

### 2.5 EF Core Migration

Create initial migration:
```bash
cd backend/src/ModernApp.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../ModernApp.Api
```

Update connection string in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=booty_by_beighley_dev;User Id=postgres;Password=..."
  },
  "Azure": {
    "BlobStorage": {
      "ContainerUri": "https://<storageaccount>.blob.core.windows.net/videos"
    }
  }
}
```

---

## 3. Application Layer (`ModernApp.Application`)

### 3.1 Feature: User Registration (Student)

**Feature Folder**: `ModernApp.Application/Features/Users/`

**Files**:

#### `RegisterStudentCommand.cs`
```csharp
namespace ModernApp.Application.Features.Users;

public record RegisterStudentCommand(
    [EmailAddress] string Email,
    [StringLength(100)] string FirstName,
    [StringLength(100)] string LastName
) : ICommand<UserDto>;

public sealed class RegisterStudentCommandHandler(IUserRepository userRepository)
    : ICommandHandler<RegisterStudentCommand, UserDto>
{
    public async Task<UserDto> ExecuteAsync(RegisterStudentCommand command, CancellationToken ct)
    {
        // Check if user already exists
        var existing = await userRepository.GetByEmailAsync(command.Email, ct);
        if (existing != null)
            throw new InvalidOperationException($"User with email {command.Email} already exists");

        var user = new User
        {
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Role = UserRole.Student,
            SubscriptionStatus = SubscriptionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, ct);
        return new UserDto(user.Id, user.Email, user.FirstName, user.LastName, 
            user.Role.ToString(), user.SubscriptionStatus.ToString());
    }
}

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string SubscriptionStatus
);
```

#### `GetStudentQuery.cs`
```csharp
namespace ModernApp.Application.Features.Users;

public record GetStudentQuery(Guid StudentId) : IQuery<StudentDetailsDto?>;

public sealed class GetStudentQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetStudentQuery, StudentDetailsDto?>
{
    public async Task<StudentDetailsDto?> ExecuteAsync(GetStudentQuery query, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(query.StudentId, ct);
        if (user == null || user.Role != UserRole.Student)
            return null;

        // Fetch enrollments, last workout log date, etc.
        return new StudentDetailsDto(user.Id, user.Email, user.FirstName, user.LastName,
            user.CreatedAt);
    }
}

public record StudentDetailsDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime EnrolledAt
);
```

### 3.2 Feature: Questionnaire & Responses

**Feature Folder**: `ModernApp.Application/Features/Questionnaire/`

#### `SubmitQuestionnaireResponsesCommand.cs`
```csharp
namespace ModernApp.Application.Features.Questionnaire;

public record SubmitQuestionnaireResponsesCommand(
    Guid StudentId,
    List<QuestionnaireAnswerInput> Answers
) : ICommand<QuestionnaireResponseDto>;

public record QuestionnaireAnswerInput(
    Guid QuestionnaireId,
    string SelectedAnswer
);

public sealed class SubmitQuestionnaireResponsesCommandHandler
    : ICommandHandler<SubmitQuestionnaireResponsesCommand, QuestionnaireResponseDto>
{
    public async Task<QuestionnaireResponseDto> ExecuteAsync(
        SubmitQuestionnaireResponsesCommand command, CancellationToken ct)
    {
        // Store responses as timestamped entries
        // Return summary for client
        // (In v2, evaluate matching rules to auto-assign plan)
        throw new NotImplementedException("Questionnaire submission — deferred to v2");
    }
}
```

### 3.3 Feature: Workout Plan Management (Coach)

**Feature Folder**: `ModernApp.Application/Features/WorkoutPlans/`

#### `CreateWorkoutPlanCommand.cs`
```csharp
namespace ModernApp.Application.Features.WorkoutPlans;

public record CreateWorkoutPlanCommand(
    [StringLength(255)] string Name,
    string Description,
    Difficulty Difficulty,
    [Range(1, 52)] int DurationWeeks,
    [Range(2, 5)] int TrainingFrequencyDaysPerWeek
) : ICommand<WorkoutPlanDto>;

public sealed class CreateWorkoutPlanCommandHandler(IWorkoutPlanRepository planRepository)
    : ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto>
{
    public async Task<WorkoutPlanDto> ExecuteAsync(
        CreateWorkoutPlanCommand command, CancellationToken ct)
    {
        var plan = new WorkoutPlan
        {
            Name = command.Name,
            Description = command.Description,
            Difficulty = command.Difficulty,
            DurationWeeks = command.DurationWeeks,
            TrainingFrequencyDaysPerWeek = command.TrainingFrequencyDaysPerWeek,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        await planRepository.AddAsync(plan, ct);
        return new WorkoutPlanDto(plan.Id, plan.Name, plan.Difficulty.ToString(), 
            plan.IsPublished);
    }
}

public record WorkoutPlanDto(Guid Id, string Name, string Difficulty, bool IsPublished);
```

#### `PublishWorkoutPlanCommand.cs`
```csharp
namespace ModernApp.Application.Features.WorkoutPlans;

public record PublishWorkoutPlanCommand(Guid PlanId) : ICommand<bool>;

public sealed class PublishWorkoutPlanCommandHandler(IWorkoutPlanRepository planRepository)
    : ICommandHandler<PublishWorkoutPlanCommand, bool>
{
    public async Task<bool> ExecuteAsync(PublishWorkoutPlanCommand command, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdAsync(command.PlanId, ct);
        if (plan == null)
            throw new InvalidOperationException($"Plan {command.PlanId} not found");

        plan.IsPublished = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await planRepository.UpdateAsync(plan, ct);
        return true;
    }
}
```

#### `GetWorkoutPlanQuery.cs`
```csharp
namespace ModernApp.Application.Features.WorkoutPlans;

public record GetWorkoutPlanQuery(Guid PlanId) : IQuery<WorkoutPlanDetailDto?>;

public sealed class GetWorkoutPlanQueryHandler(IWorkoutPlanRepository planRepository)
    : IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDetailDto?>
{
    public async Task<WorkoutPlanDetailDto?> ExecuteAsync(GetWorkoutPlanQuery query, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdWithWorkoutsAsync(query.PlanId, ct);
        if (plan == null)
            return null;

        var workouts = plan.Workouts
            .OrderBy(w => w.Order)
            .Select(w => new WorkoutSummaryDto(w.Id, w.Name, w.Order))
            .ToList();

        return new WorkoutPlanDetailDto(plan.Id, plan.Name, plan.Difficulty.ToString(),
            plan.DurationWeeks, plan.IsPublished, workouts);
    }
}

public record WorkoutPlanDetailDto(
    Guid Id, string Name, string Difficulty, int DurationWeeks, 
    bool IsPublished, List<WorkoutSummaryDto> Workouts
);

public record WorkoutSummaryDto(Guid Id, string Name, int Order);
```

### 3.4 Feature: Movements (Coach)

**Feature Folder**: `ModernApp.Application/Features/Movements/`

#### `CreateMovementCommand.cs`
```csharp
namespace ModernApp.Application.Features.Movements;

public record CreateMovementCommand(
    [StringLength(255)] string Name,
    string Description,
    int DefaultSets,
    int DefaultReps,
    int? DefaultRestSeconds
) : ICommand<MovementDto>;

public sealed class CreateMovementCommandHandler(IMovementRepository movementRepository)
    : ICommandHandler<CreateMovementCommand, MovementDto>
{
    public async Task<MovementDto> ExecuteAsync(CreateMovementCommand command, CancellationToken ct)
    {
        var movement = new Movement
        {
            Name = command.Name,
            Description = command.Description,
            DefaultSets = command.DefaultSets,
            DefaultReps = command.DefaultReps,
            DefaultRestSeconds = command.DefaultRestSeconds,
            CreatedAt = DateTime.UtcNow
        };

        await movementRepository.AddAsync(movement, ct);
        return new MovementDto(movement.Id, movement.Name, movement.DefaultSets, 
            movement.DefaultReps, null);
    }
}

public record MovementDto(Guid Id, string Name, int DefaultSets, 
    int DefaultReps, string? VideoUrl);
```

#### `UploadMovementVideoCommand.cs`
```csharp
namespace ModernApp.Application.Features.Movements;

public record UploadMovementVideoCommand(
    Guid MovementId,
    [StringLength(5000)] string Caption,
    Stream FileStream,
    string FileName
) : ICommand<MovementVideoUploadDto>;

public sealed class UploadMovementVideoCommandHandler(
    IMovementRepository movementRepository,
    IBlobStorageService blobStorageService)
    : ICommandHandler<UploadMovementVideoCommand, MovementVideoUploadDto>
{
    public async Task<MovementVideoUploadDto> ExecuteAsync(
        UploadMovementVideoCommand command, CancellationToken ct)
    {
        var movement = await movementRepository.GetByIdAsync(command.MovementId, ct);
        if (movement == null)
            throw new InvalidOperationException($"Movement {command.MovementId} not found");

        // Upload to Azure Blob Storage
        var blobName = $"{command.MovementId}/{command.FileName}";
        var videoUrl = await blobStorageService.UploadVideoAsync(
            "movement-videos", blobName, command.FileStream, ct);

        // Update movement record
        movement.VideoUrl = videoUrl;
        movement.VideoCaption = command.Caption;
        movement.UpdatedAt = DateTime.UtcNow;
        await movementRepository.UpdateAsync(movement, ct);

        return new MovementVideoUploadDto(movement.Id, videoUrl, command.Caption);
    }
}

public record MovementVideoUploadDto(Guid MovementId, string VideoUrl, string Caption);
```

### 3.5 Feature: Plan Enrollment (Coach)

**Feature Folder**: `ModernApp.Application/Features/PlanEnrollment/`

#### `EnrollStudentToPlanCommand.cs`
```csharp
namespace ModernApp.Application.Features.PlanEnrollment;

public record EnrollStudentToPlanCommand(
    Guid StudentId,
    Guid WorkoutPlanId,
    List<DayOfWeek> SelectedTrainingDays
) : ICommand<PlanEnrollmentDto>;

public sealed class EnrollStudentToPlanCommandHandler(
    IPlanEnrollmentRepository enrollmentRepository,
    IWorkoutPlanRepository planRepository)
    : ICommandHandler<EnrollStudentToPlanCommand, PlanEnrollmentDto>
{
    public async Task<PlanEnrollmentDto> ExecuteAsync(
        EnrollStudentToPlanCommand command, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdAsync(command.WorkoutPlanId, ct);
        if (plan == null)
            throw new InvalidOperationException($"Plan {command.WorkoutPlanId} not found");

        if (command.SelectedTrainingDays.Count != plan.TrainingFrequencyDaysPerWeek)
            throw new InvalidOperationException(
                $"Plan requires {plan.TrainingFrequencyDaysPerWeek} training days");

        var enrollment = new PlanEnrollment
        {
            UserId = command.StudentId,
            WorkoutPlanId = command.WorkoutPlanId,
            EnrolledAt = DateTime.UtcNow,
            StartDate = DateTime.UtcNow,
            PlannedEndDate = DateTime.UtcNow.AddDays(plan.DurationWeeks * 7),
            SelectedTrainingDays = JsonSerializer.Serialize(command.SelectedTrainingDays),
            Status = PlanEnrollmentStatus.Active
        };

        await enrollmentRepository.AddAsync(enrollment, ct);
        return new PlanEnrollmentDto(enrollment.Id, command.StudentId, 
            command.WorkoutPlanId, PlanEnrollmentStatus.Active.ToString());
    }
}

public record PlanEnrollmentDto(Guid Id, Guid StudentId, Guid PlanId, string Status);
```

#### `GetStudentPlansQuery.cs`
```csharp
namespace ModernApp.Application.Features.PlanEnrollment;

public record GetStudentPlansQuery(Guid StudentId) : IQuery<List<StudentPlanDto>>;

public sealed class GetStudentPlansQueryHandler(IPlanEnrollmentRepository enrollmentRepository)
    : IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>>
{
    public async Task<List<StudentPlanDto>> ExecuteAsync(
        GetStudentPlansQuery query, CancellationToken ct)
    {
        var enrollments = await enrollmentRepository
            .GetByStudentIdAsync(query.StudentId, ct);

        return enrollments
            .Where(e => e.Status == PlanEnrollmentStatus.Active)
            .Select(e => new StudentPlanDto(e.Id, e.WorkoutPlanId, e.PlannedEndDate))
            .ToList();
    }
}

public record StudentPlanDto(Guid EnrollmentId, Guid PlanId, DateTime PlannedEndDate);
```

### 3.6 Feature: Workout Logging (Student)

**Feature Folder**: `ModernApp.Application/Features/WorkoutLogging/`

#### `LogWorkoutCompletionCommand.cs`
```csharp
namespace ModernApp.Application.Features.WorkoutLogging;

public record LogWorkoutCompletionCommand(
    Guid StudentId,
    Guid WorkoutId,
    Guid PlanEnrollmentId,
    List<LoggedSetInput> LoggedSets,
    string? Notes
) : ICommand<WorkoutLogEntryDto>;

public record LoggedSetInput(
    Guid WorkoutMovementId,
    int SetNumber,
    int RepsCompleted,
    decimal? WeightUsed,
    int? DurationSeconds
);

public sealed class LogWorkoutCompletionCommandHandler(
    IWorkoutLogRepository logRepository,
    IPersonalRecordService prService)
    : ICommandHandler<LogWorkoutCompletionCommand, WorkoutLogEntryDto>
{
    public async Task<WorkoutLogEntryDto> ExecuteAsync(
        LogWorkoutCompletionCommand command, CancellationToken ct)
    {
        var logEntry = new WorkoutLogEntry
        {
            UserId = command.StudentId,
            WorkoutId = command.WorkoutId,
            PlanEnrollmentId = command.PlanEnrollmentId,
            CompletedAt = DateTime.UtcNow,
            Notes = command.Notes,
            Status = WorkoutStatus.Completed,
            LoggedSets = command.LoggedSets
                .Select(s => new WorkoutLogSetEntry
                {
                    WorkoutMovementId = s.WorkoutMovementId,
                    SetNumber = s.SetNumber,
                    RepsCompleted = s.RepsCompleted,
                    WeightUsed = s.WeightUsed,
                    DurationSeconds = s.DurationSeconds
                })
                .ToList()
        };

        await logRepository.AddAsync(logEntry, ct);

        // Check for new PRs
        var prDetections = await prService.DetectNewPRsAsync(
            command.StudentId, logEntry.LoggedSets, ct);

        return new WorkoutLogEntryDto(logEntry.Id, logEntry.CompletedAt, 
            logEntry.Status.ToString(), prDetections);
    }
}

public record WorkoutLogEntryDto(
    Guid Id,
    DateTime CompletedAt,
    string Status,
    List<PersonalRecordDetectionDto> NewPRs
);

public record PersonalRecordDetectionDto(Guid MovementId, decimal Weight);
```

#### `SubmitWorkoutFeedbackCommand.cs`
```csharp
namespace ModernApp.Application.Features.WorkoutLogging;

public record SubmitWorkoutFeedbackCommand(
    Guid StudentId,
    Guid WorkoutLogEntryId,
    [Range(1, 5)] int DifficultyRating,
    string? Comment
) : ICommand<WorkoutFeedbackDto>;

public sealed class SubmitWorkoutFeedbackCommandHandler(
    IWorkoutFeedbackRepository feedbackRepository)
    : ICommandHandler<SubmitWorkoutFeedbackCommand, WorkoutFeedbackDto>
{
    public async Task<WorkoutFeedbackDto> ExecuteAsync(
        SubmitWorkoutFeedbackCommand command, CancellationToken ct)
    {
        var feedback = new WorkoutFeedback
        {
            WorkoutLogEntryId = command.WorkoutLogEntryId,
            UserId = command.StudentId,
            DifficultyRating = command.DifficultyRating,
            Comment = command.Comment,
            SubmittedAt = DateTime.UtcNow
        };

        await feedbackRepository.AddAsync(feedback, ct);
        return new WorkoutFeedbackDto(feedback.Id, command.DifficultyRating);
    }
}

public record WorkoutFeedbackDto(Guid Id, int DifficultyRating);
```

#### `GetStudentProgressQuery.cs`
```csharp
namespace ModernApp.Application.Features.WorkoutLogging;

public record GetStudentProgressQuery(Guid StudentId) : IQuery<StudentProgressDto>;

public sealed class GetStudentProgressQueryHandler(
    IWorkoutLogRepository logRepository,
    IPersonalRecordRepository prRepository)
    : IQueryHandler<GetStudentProgressQuery, StudentProgressDto>
{
    public async Task<StudentProgressDto> ExecuteAsync(
        GetStudentProgressQuery query, CancellationToken ct)
    {
        var completedCount = await logRepository.CountCompletedByStudentAsync(query.StudentId, ct);
        var missedCount = await logRepository.CountMissedByStudentAsync(query.StudentId, ct);
        var personalRecords = await prRepository.GetByStudentAsync(query.StudentId, ct);

        return new StudentProgressDto(completedCount, missedCount, personalRecords.Count);
    }
}

public record StudentProgressDto(int WorkoutsCompleted, int WorkoutsMissed, int PersonalRecords);
```

### 3.7 Dependency Injection

Update `ModernApp.Application/DependencyInjection.cs`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Application.Features.Users;
using ModernApp.Application.Features.WorkoutPlans;
using ModernApp.Application.Features.Movements;
using ModernApp.Application.Features.PlanEnrollment;
using ModernApp.Application.Features.WorkoutLogging;

namespace ModernApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // User commands
        services.AddScoped<ICommandHandler<RegisterStudentCommand, UserDto>, 
            RegisterStudentCommandHandler>();

        // User queries
        services.AddScoped<IQueryHandler<GetStudentQuery, StudentDetailsDto?>, 
            GetStudentQueryHandler>();

        // Workout Plan commands
        services.AddScoped<ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto>,
            CreateWorkoutPlanCommandHandler>();
        services.AddScoped<ICommandHandler<PublishWorkoutPlanCommand, bool>,
            PublishWorkoutPlanCommandHandler>();

        // Workout Plan queries
        services.AddScoped<IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDetailDto?>,
            GetWorkoutPlanQueryHandler>();

        // Movement commands
        services.AddScoped<ICommandHandler<CreateMovementCommand, MovementDto>,
            CreateMovementCommandHandler>();
        services.AddScoped<ICommandHandler<UploadMovementVideoCommand, MovementVideoUploadDto>,
            UploadMovementVideoCommandHandler>();

        // Plan Enrollment commands
        services.AddScoped<ICommandHandler<EnrollStudentToPlanCommand, PlanEnrollmentDto>,
            EnrollStudentToPlanCommandHandler>();

        // Plan Enrollment queries
        services.AddScoped<IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>>,
            GetStudentPlansQueryHandler>();

        // Workout Logging commands
        services.AddScoped<ICommandHandler<LogWorkoutCompletionCommand, WorkoutLogEntryDto>,
            LogWorkoutCompletionCommandHandler>();
        services.AddScoped<ICommandHandler<SubmitWorkoutFeedbackCommand, WorkoutFeedbackDto>,
            SubmitWorkoutFeedbackCommandHandler>();

        // Workout Logging queries
        services.AddScoped<IQueryHandler<GetStudentProgressQuery, StudentProgressDto>,
            GetStudentProgressQueryHandler>();

        return services;
    }
}
```

---

## 4. API Layer (`ModernApp.Api`)

### 4.1 Endpoint Design

All endpoints follow:
- `[Authorize]` for protected routes
- `[Authorize(Roles = "Coach")]` for coach-only routes
- Return `TypedResults` for type safety
- Use `.WithParameterValidation()` for request validation
- Use `.WithTags("FeatureName")` for OpenAPI grouping

### 4.2 Coach Endpoints (Admin)

**Feature**: Workout Plans

```csharp
// POST /api/v1/workout-plans
app.MapPost("/api/v1/workout-plans", async (
    CreateWorkoutPlanCommand cmd,
    ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Created($"/api/v1/workout-plans/{result.Id}", result);
})
.WithName("CreateWorkoutPlan")
.WithOpenApi()
.WithParameterValidation()
.WithTags("WorkoutPlans")
.Produces<WorkoutPlanDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

// GET /api/v1/workout-plans/{planId}
app.MapGet("/api/v1/workout-plans/{planId:guid}", async (
    Guid planId,
    IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDetailDto?> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    var result = await handler.ExecuteAsync(
        new GetWorkoutPlanQuery(planId), http.RequestAborted);
    return result == null 
        ? Results.NotFound() 
        : Results.Ok(result);
})
.WithName("GetWorkoutPlan")
.WithOpenApi()
.WithTags("WorkoutPlans")
.Produces<WorkoutPlanDetailDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

// PUT /api/v1/workout-plans/{planId}/publish
app.MapPut("/api/v1/workout-plans/{planId:guid}/publish", async (
    Guid planId,
    ICommandHandler<PublishWorkoutPlanCommand, bool> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    await handler.ExecuteAsync(new PublishWorkoutPlanCommand(planId), http.RequestAborted);
    return Results.NoContent();
})
.WithName("PublishWorkoutPlan")
.WithOpenApi()
.WithTags("WorkoutPlans")
.ProducesProblem(StatusCodes.Status404NotFound);
```

**Feature**: Movements

```csharp
// POST /api/v1/movements
app.MapPost("/api/v1/movements", async (
    CreateMovementCommand cmd,
    ICommandHandler<CreateMovementCommand, MovementDto> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Created($"/api/v1/movements/{result.Id}", result);
})
.WithName("CreateMovement")
.WithOpenApi()
.WithParameterValidation()
.WithTags("Movements")
.Produces<MovementDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

// POST /api/v1/movements/{movementId}/video
app.MapPost("/api/v1/movements/{movementId:guid}/video", async (
    Guid movementId,
    IFormFile videoFile,
    [FromForm] string caption,
    IBlobStorageService blobStorage,
    ICommandHandler<UploadMovementVideoCommand, MovementVideoUploadDto> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    using var stream = videoFile.OpenReadStream();
    var cmd = new UploadMovementVideoCommand(
        movementId, caption, stream, videoFile.FileName);
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Ok(result);
})
.WithName("UploadMovementVideo")
.WithOpenApi()
.WithTags("Movements")
.Produces<MovementVideoUploadDto>()
.ProducesProblem(StatusCodes.Status400BadRequest);
```

**Feature**: Plan Enrollment (Coach assigns plans to students)

```csharp
// POST /api/v1/plan-enrollments
app.MapPost("/api/v1/plan-enrollments", async (
    EnrollStudentToPlanCommand cmd,
    ICommandHandler<EnrollStudentToPlanCommand, PlanEnrollmentDto> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Created($"/api/v1/plan-enrollments/{result.Id}", result);
})
.WithName("EnrollStudentToPlan")
.WithOpenApi()
.WithParameterValidation()
.WithTags("PlanEnrollment")
.Produces<PlanEnrollmentDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

// GET /api/v1/students/{studentId}/plan-enrollments
app.MapGet("/api/v1/students/{studentId:guid}/plan-enrollments", async (
    Guid studentId,
    IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>> handler,
    HttpContext http) =>
{
    [Authorize(Roles = "Coach")]
    var result = await handler.ExecuteAsync(
        new GetStudentPlansQuery(studentId), http.RequestAborted);
    return Results.Ok(result);
})
.WithName("GetStudentPlans")
.WithOpenApi()
.WithTags("PlanEnrollment")
.Produces<List<StudentPlanDto>>();
```

### 4.3 Student Endpoints (Self-service)

**Feature**: User Registration

```csharp
// POST /api/v1/auth/register
app.MapPost("/api/v1/auth/register", async (
    RegisterStudentCommand cmd,
    ICommandHandler<RegisterStudentCommand, UserDto> handler,
    HttpContext http) =>
{
    // No auth required for registration
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Created($"/api/v1/students/{result.Id}", result);
})
.WithName("RegisterStudent")
.WithOpenApi()
.WithParameterValidation()
.WithTags("Auth")
.Produces<UserDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

// GET /api/v1/students/{studentId}
app.MapGet("/api/v1/students/{studentId:guid}", async (
    Guid studentId,
    IQueryHandler<GetStudentQuery, StudentDetailsDto?> handler,
    HttpContext http) =>
{
    [Authorize]
    var result = await handler.ExecuteAsync(
        new GetStudentQuery(studentId), http.RequestAborted);
    return result == null 
        ? Results.NotFound() 
        : Results.Ok(result);
})
.WithName("GetStudentDetails")
.WithOpenApi()
.WithTags("Students")
.Produces<StudentDetailsDto>()
.ProducesProblem(StatusCodes.Status404NotFound);
```

**Feature**: Plan Enrollment (Student views assigned plans)

```csharp
// GET /api/v1/my/plans
app.MapGet("/api/v1/my/plans", async (
    HttpContext http,
    IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>> handler) =>
{
    [Authorize]
    var studentId = Guid.Parse(http.User.FindFirst("sub").Value);
    var result = await handler.ExecuteAsync(
        new GetStudentPlansQuery(studentId), http.RequestAborted);
    return Results.Ok(result);
})
.WithName("GetMyPlans")
.WithOpenApi()
.WithTags("MyPlans")
.Produces<List<StudentPlanDto>>();
```

**Feature**: Workout Logging

```csharp
// POST /api/v1/workouts/{workoutId}/log
app.MapPost("/api/v1/workouts/{workoutId:guid}/log", async (
    Guid workoutId,
    LogWorkoutCompletionCommand cmd,
    ICommandHandler<LogWorkoutCompletionCommand, WorkoutLogEntryDto> handler,
    HttpContext http) =>
{
    [Authorize]
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Created($"/api/v1/workout-logs/{result.Id}", result);
})
.WithName("LogWorkoutCompletion")
.WithOpenApi()
.WithParameterValidation()
.WithTags("WorkoutLogging")
.Produces<WorkoutLogEntryDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

// POST /api/v1/workout-logs/{logId}/feedback
app.MapPost("/api/v1/workout-logs/{logId:guid}/feedback", async (
    Guid logId,
    SubmitWorkoutFeedbackCommand cmd,
    ICommandHandler<SubmitWorkoutFeedbackCommand, WorkoutFeedbackDto> handler,
    HttpContext http) =>
{
    [Authorize]
    var result = await handler.ExecuteAsync(cmd, http.RequestAborted);
    return Results.Created($"/api/v1/workout-feedback/{result.Id}", result);
})
.WithName("SubmitWorkoutFeedback")
.WithOpenApi()
.WithParameterValidation()
.WithTags("WorkoutLogging")
.Produces<WorkoutFeedbackDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

// GET /api/v1/my/progress
app.MapGet("/api/v1/my/progress", async (
    HttpContext http,
    IQueryHandler<GetStudentProgressQuery, StudentProgressDto> handler) =>
{
    [Authorize]
    var studentId = Guid.Parse(http.User.FindFirst("sub").Value);
    var result = await handler.ExecuteAsync(
        new GetStudentProgressQuery(studentId), http.RequestAborted);
    return Results.Ok(result);
})
.WithName("GetMyProgress")
.WithOpenApi()
.WithTags("Progress")
.Produces<StudentProgressDto>();
```

### 4.4 Shared Endpoints

**Feature**: Get Exercise Video (signed URL)

```csharp
// GET /api/v1/movements/{movementId}/video-url
app.MapGet("/api/v1/movements/{movementId:guid}/video-url", async (
    Guid movementId,
    IBlobStorageService blobStorage,
    IMovementRepository movementRepository,
    HttpContext http) =>
{
    [Authorize]
    var movement = await movementRepository.GetByIdAsync(movementId, http.RequestAborted);
    if (movement?.VideoUrl == null)
        return Results.NotFound("No video available for this movement");

    // Extract blob name and generate signed URL
    var uri = new Uri(movement.VideoUrl);
    var blobName = uri.AbsolutePath.TrimStart('/').Split('/')[1]; // Extraction logic TBD
    var signedUrl = await blobStorage.GetSignedUrlAsync(
        "movement-videos", blobName, 60, http.RequestAborted);

    return Results.Ok(new { videoUrl = signedUrl.ToString() });
})
.WithName("GetMovieVideoUrl")
.WithOpenApi()
.WithTags("Movements")
.Produces<VideoUrlDto>();

public record VideoUrlDto(string VideoUrl);
```

### 4.5 Update Program.cs

```csharp
using ModernApp.Api.Features;
using ModernApp.Application;
using ModernApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseStatusCodePages();

// Map all feature endpoints
app.MapCoachEndpoints();
app.MapStudentEndpoints();
app.MapSharedEndpoints();

app.Run();
```

Create extension methods in `ModernApp.Api/Features/` folder for each feature group.

---

## 5. Testing Strategy

### 5.1 Unit Tests (Application Layer)

Create tests in `ModernApp.Application.Tests/` for:
- CQRS commands and queries
- Domain entity validations
- Business logic (PR detection, enrollment validation, etc.)

Example test:

```csharp
[TestClass]
public class RegisterStudentCommandTests
{
    [TestMethod]
    public async Task RegisterStudentCommand_WithValidData_CreatesStudent()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var handler = new RegisterStudentCommandHandler(mockUserRepository.Object);
        var cmd = new RegisterStudentCommand("john@example.com", "John", "Doe");

        // Act
        var result = await handler.ExecuteAsync(cmd, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("john@example.com", result.Email);
    }
}
```

### 5.2 Integration Tests (Infrastructure + API)

Create tests in a new `ModernApp.Integration.Tests/` project for:
- EF Core DbContext and migrations
- Azure Blob Storage upload/retrieval
- Full API endpoint flows

---

## 6. Documentation Updates

After implementation, update `docs/FEATURES.md` with:

**Section: Core Domain**
- Entity definitions (User, WorkoutPlan, Workout, Movement, etc.)
- Relationships and constraints
- Enums (UserRole, Difficulty, etc.)

**Section: API Endpoints**
- Coach management endpoints
- Student self-service endpoints
- Shared endpoints
- Auth/authorization model
- Example request/response payloads

**Section: Video Upload & Storage**
- Azure Blob Storage integration
- Signed URL generation for student access
- Video caption/transcript requirements

**Section: Questionnaire & Matching (v1 Deferred)**
- Current placeholder implementation
- v2 roadmap for automated matching rules

---

## 7. Migration & Deployment

### 7.1 EF Core Migration

```bash
# Create initial migration
cd backend/src/ModernApp.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../ModernApp.Api

# Apply to dev database
dotnet ef database update --startup-project ../ModernApp.Api
```

### 7.2 Local Development

1. Set `appsettings.Development.json` connection strings and Azure credentials
2. Run migrations
3. Start API: `dotnet run` from `ModernApp.Api` folder
4. Open `https://localhost:5001/openapi/v1.json` to verify OpenAPI spec
5. Use NSwag to generate frontend client types

---

## 8. Scope Boundaries (Out of Scope for Phase 1)

- ❌ Frontend UI implementation (Phase 2)
- ❌ Capacitor mobile app setup (Phase 3)
- ❌ Questionnaire automated matching rules (v2)
- ❌ Plan auto-renewal logic (v2)
- ❌ Notifications / push reminders (out of scope per APP.md)
- ❌ Smartwatch health data integration (v2)
- ❌ Stripe payment gating (v2)
- ❌ Multi-coach authorization (roadmap, currently single coach assumed)

---

## Next Steps

1. **Implement Domain Layer** — Define all 9 entities with proper aggregates, value objects, and validation rules
2. **Set up EF Core** — Create DbContext, configure relationships, add indexes, create migration
3. **Implement Application Layer** — CQRS handlers for all coach and student operations
4. **Expose API Endpoints** — Minimal API mappings with auth/authorization
5. **Integrate Azure Blob Storage** — Video upload and signed URL generation
6. **Unit Tests** — Cover CQRS handlers and domain logic
7. **Update Documentation** — FEATURES.md with domain model and API reference

---

**Backend Implementer**: Once you receive this spec, begin with the Domain Layer. All entity definitions are provided above. Implement them in `ModernApp.Domain`, then proceed to Infrastructure, Application, and API layers in order.

**Questions or clarifications?** Refer to [Backend Implementer Agent](../.github/agents/backend-implementer.agent.md) and [APP.md](./APP.md) for architectural guidance and product context.
