using ModernApp.Domain.Common;

namespace ModernApp.Domain.WorkoutLogs;

public class WorkoutLogSetEntry : Entity
{
    public Guid WorkoutLogEntryId { get; private set; }
    public Guid WorkoutMovementId { get; private set; }
    public int SetNumber { get; private set; }
    public int RepsCompleted { get; private set; }
    public decimal? WeightUsed { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string? NotesPerMovement { get; private set; }

    private WorkoutLogSetEntry() { }

    public static WorkoutLogSetEntry Create(
        Guid workoutLogEntryId,
        Guid workoutMovementId,
        int setNumber,
        int repsCompleted,
        decimal? weightUsed = null,
        int? durationSeconds = null,
        string? notesPerMovement = null)
    {
        if (setNumber < 1 || setNumber > 20)
            throw new ArgumentException("SetNumber must be between 1 and 20", nameof(setNumber));
        if (repsCompleted < 0 || repsCompleted > 1000)
            throw new ArgumentException("RepsCompleted must be between 0 and 1000", nameof(repsCompleted));
        if (weightUsed.HasValue && (weightUsed < 0 || weightUsed > 500))
            throw new ArgumentException("WeightUsed must be between 0 and 500 kg", nameof(weightUsed));
        if (durationSeconds.HasValue && (durationSeconds < 0 || durationSeconds > 3600))
            throw new ArgumentException("DurationSeconds must be between 0 and 3600", nameof(durationSeconds));

        return new WorkoutLogSetEntry
        {
            WorkoutLogEntryId = workoutLogEntryId,
            WorkoutMovementId = workoutMovementId,
            SetNumber = setNumber,
            RepsCompleted = repsCompleted,
            WeightUsed = weightUsed,
            DurationSeconds = durationSeconds,
            NotesPerMovement = notesPerMovement
        };
    }

    public static WorkoutLogSetEntry Reconstitute(
        Guid id,
        Guid workoutLogEntryId,
        Guid workoutMovementId,
        int setNumber,
        int repsCompleted,
        decimal? weightUsed,
        int? durationSeconds,
        string? notesPerMovement)
    {
        return new WorkoutLogSetEntry
        {
            Id = id,
            WorkoutLogEntryId = workoutLogEntryId,
            WorkoutMovementId = workoutMovementId,
            SetNumber = setNumber,
            RepsCompleted = repsCompleted,
            WeightUsed = weightUsed,
            DurationSeconds = durationSeconds,
            NotesPerMovement = notesPerMovement
        };
    }
}
