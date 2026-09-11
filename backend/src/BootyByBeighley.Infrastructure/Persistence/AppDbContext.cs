using Microsoft.EntityFrameworkCore;
using BootyByBeighley.Domain.MatchingRules;
using BootyByBeighley.Domain.Movements;
using BootyByBeighley.Domain.PersonalRecords;
using BootyByBeighley.Domain.PlanEnrollments;
using BootyByBeighley.Domain.Questionnaires;
using BootyByBeighley.Domain.TodoItems;
using BootyByBeighley.Domain.Users;
using BootyByBeighley.Domain.WorkoutLogs;
using BootyByBeighley.Domain.WorkoutPlans;
using BootyByBeighley.Infrastructure.Persistence.Configurations;

namespace BootyByBeighley.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutMovement> WorkoutMovements => Set<WorkoutMovement>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<Questionnaire> Questionnaires => Set<Questionnaire>();
    public DbSet<QuestionnaireResponse> QuestionnaireResponses => Set<QuestionnaireResponse>();
    public DbSet<MatchingRule> MatchingRules => Set<MatchingRule>();
    public DbSet<PlanEnrollment> PlanEnrollments => Set<PlanEnrollment>();
    public DbSet<WorkoutLogEntry> WorkoutLogEntries => Set<WorkoutLogEntry>();
    public DbSet<WorkoutLogSetEntry> WorkoutLogSetEntries => Set<WorkoutLogSetEntry>();
    public DbSet<PersonalRecord> PersonalRecords => Set<PersonalRecord>();
    public DbSet<WorkoutFeedback> WorkoutFeedback => Set<WorkoutFeedback>();
    public DbSet<MissedWorkout> MissedWorkouts => Set<MissedWorkout>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutPlanConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutMovementConfiguration());
        modelBuilder.ApplyConfiguration(new MovementConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionnaireConfiguration());
        modelBuilder.ApplyConfiguration(new QuestionnaireResponseConfiguration());
        modelBuilder.ApplyConfiguration(new MatchingRuleConfiguration());
        modelBuilder.ApplyConfiguration(new PlanEnrollmentConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutLogEntryConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutLogSetEntryConfiguration());
        modelBuilder.ApplyConfiguration(new PersonalRecordConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutFeedbackConfiguration());
        modelBuilder.ApplyConfiguration(new MissedWorkoutConfiguration());
    }
}
