using BootyByBeighley.Domain.Common;

namespace BootyByBeighley.Domain.WorkoutLogs;

public class WorkoutFeedback : Entity
{
    public Guid WorkoutLogEntryId { get; private set; }
    public Guid UserId { get; private set; }
    public int DifficultyRating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private WorkoutFeedback() { }

    public static WorkoutFeedback Create(
        Guid workoutLogEntryId,
        Guid userId,
        int difficultyRating,
        string? comment = null)
    {
        if (difficultyRating < 1 || difficultyRating > 5)
            throw new ArgumentException("DifficultyRating must be between 1 and 5", nameof(difficultyRating));
        if (comment != null && comment.Length > 500)
            throw new ArgumentException("Comment must not exceed 500 characters", nameof(comment));

        return new WorkoutFeedback
        {
            WorkoutLogEntryId = workoutLogEntryId,
            UserId = userId,
            DifficultyRating = difficultyRating,
            Comment = comment,
            SubmittedAt = DateTime.UtcNow
        };
    }

    public static WorkoutFeedback Reconstitute(
        Guid id,
        Guid workoutLogEntryId,
        Guid userId,
        int difficultyRating,
        string? comment,
        DateTime submittedAt)
    {
        return new WorkoutFeedback
        {
            Id = id,
            WorkoutLogEntryId = workoutLogEntryId,
            UserId = userId,
            DifficultyRating = difficultyRating,
            Comment = comment,
            SubmittedAt = submittedAt
        };
    }
}
