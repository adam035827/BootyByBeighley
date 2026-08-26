using ModernApp.Domain.Common;

namespace ModernApp.Domain.WorkoutLogs;

public class MissedWorkout : Entity
{
    public Guid WorkoutLogEntryId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WorkoutId { get; private set; }
    public DateTime MissedDate { get; private set; }
    public string? MissedReason { get; private set; }
    public string? Notes { get; private set; }

    private MissedWorkout() { }

    public static MissedWorkout Create(
        Guid workoutLogEntryId,
        Guid userId,
        Guid workoutId,
        string? missedReason = null,
        string? notes = null)
    {
        return new MissedWorkout
        {
            WorkoutLogEntryId = workoutLogEntryId,
            UserId = userId,
            WorkoutId = workoutId,
            MissedDate = DateTime.UtcNow,
            MissedReason = missedReason,
            Notes = notes
        };
    }

    public static MissedWorkout Reconstitute(
        Guid id,
        Guid workoutLogEntryId,
        Guid userId,
        Guid workoutId,
        DateTime missedDate,
        string? missedReason,
        string? notes)
    {
        return new MissedWorkout
        {
            Id = id,
            WorkoutLogEntryId = workoutLogEntryId,
            UserId = userId,
            WorkoutId = workoutId,
            MissedDate = missedDate,
            MissedReason = missedReason,
            Notes = notes
        };
    }
}
