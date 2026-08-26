using ModernApp.Domain.Common;

namespace ModernApp.Domain.WorkoutLogs;

public class WorkoutLogEntry : Entity
{
    public Guid UserId { get; private set; }
    public Guid WorkoutId { get; private set; }
    public Guid PlanEnrollmentId { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public string? Notes { get; private set; }
    public WorkoutStatus Status { get; private set; }
    public string? MissedReason { get; private set; }

    // Navigation property
    public IReadOnlyList<WorkoutLogSetEntry> LoggedSets { get; private set; } = new List<WorkoutLogSetEntry>();

    private WorkoutLogEntry() { }

    public static WorkoutLogEntry CreateCompleted(
        Guid userId,
        Guid workoutId,
        Guid planEnrollmentId,
        List<WorkoutLogSetEntry> loggedSets,
        string? notes = null)
    {
        if (loggedSets == null || loggedSets.Count == 0)
            throw new ArgumentException("LoggedSets cannot be empty for completed workout", nameof(loggedSets));

        return new WorkoutLogEntry
        {
            UserId = userId,
            WorkoutId = workoutId,
            PlanEnrollmentId = planEnrollmentId,
            CompletedAt = DateTime.UtcNow,
            Notes = notes,
            Status = WorkoutStatus.Completed,
            MissedReason = null,
            LoggedSets = loggedSets.AsReadOnly()
        };
    }

    public static WorkoutLogEntry CreateMissed(
        Guid userId,
        Guid workoutId,
        Guid planEnrollmentId,
        string? missedReason = null,
        string? notes = null)
    {
        return new WorkoutLogEntry
        {
            UserId = userId,
            WorkoutId = workoutId,
            PlanEnrollmentId = planEnrollmentId,
            CompletedAt = DateTime.UtcNow,
            Notes = notes,
            Status = WorkoutStatus.Missed,
            MissedReason = missedReason,
            LoggedSets = new List<WorkoutLogSetEntry>()
        };
    }

    public static WorkoutLogEntry Reconstitute(
        Guid id,
        Guid userId,
        Guid workoutId,
        Guid planEnrollmentId,
        DateTime completedAt,
        string? notes,
        WorkoutStatus status,
        string? missedReason)
    {
        return new WorkoutLogEntry
        {
            Id = id,
            UserId = userId,
            WorkoutId = workoutId,
            PlanEnrollmentId = planEnrollmentId,
            CompletedAt = completedAt,
            Notes = notes,
            Status = status,
            MissedReason = missedReason
        };
    }
}
