using ModernApp.Domain.Common;

namespace ModernApp.Domain.WorkoutPlans;

public class WorkoutPlan : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public int DurationWeeks { get; private set; }
    public int TrainingFrequencyDaysPerWeek { get; private set; }
    public Guid? NextPhasePlanId { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation property
    public IReadOnlyList<Workout> Workouts { get; private set; } = new List<Workout>();

    private WorkoutPlan() { }

    public static WorkoutPlan Create(
        string name,
        Difficulty difficulty,
        int durationWeeks,
        int trainingFrequencyDaysPerWeek,
        string? description = null,
        Guid? nextPhasePlanId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (name.Length > 255)
            throw new ArgumentException("Name must not exceed 255 characters", nameof(name));
        if (durationWeeks < 1 || durationWeeks > 52)
            throw new ArgumentException("DurationWeeks must be between 1 and 52", nameof(durationWeeks));
        if (trainingFrequencyDaysPerWeek < 2 || trainingFrequencyDaysPerWeek > 5)
            throw new ArgumentException("TrainingFrequencyDaysPerWeek must be between 2 and 5", nameof(trainingFrequencyDaysPerWeek));

        return new WorkoutPlan
        {
            Name = name,
            Description = description,
            Difficulty = difficulty,
            DurationWeeks = durationWeeks,
            TrainingFrequencyDaysPerWeek = trainingFrequencyDaysPerWeek,
            NextPhasePlanId = nextPhasePlanId,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static WorkoutPlan Reconstitute(
        Guid id,
        string name,
        string? description,
        Difficulty difficulty,
        int durationWeeks,
        int trainingFrequencyDaysPerWeek,
        Guid? nextPhasePlanId,
        bool isPublished,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new WorkoutPlan
        {
            Id = id,
            Name = name,
            Description = description,
            Difficulty = difficulty,
            DurationWeeks = durationWeeks,
            TrainingFrequencyDaysPerWeek = trainingFrequencyDaysPerWeek,
            NextPhasePlanId = nextPhasePlanId,
            IsPublished = isPublished,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Publish()
    {
        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNextPhase(Guid? nextPlanId)
    {
        NextPhasePlanId = nextPlanId;
        UpdatedAt = DateTime.UtcNow;
    }
}
