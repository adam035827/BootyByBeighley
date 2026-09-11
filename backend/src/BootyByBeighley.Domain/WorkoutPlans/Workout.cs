using BootyByBeighley.Domain.Common;

namespace BootyByBeighley.Domain.WorkoutPlans;

public class Workout : Entity
{
    public Guid WorkoutPlanId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public int Order { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public IReadOnlyList<WorkoutMovement> Movements { get; private set; } = new List<WorkoutMovement>();

    private Workout() { }

    public static Workout Create(
        Guid workoutPlanId,
        string name,
        int order,
        int estimatedDurationMinutes,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (name.Length > 255)
            throw new ArgumentException("Name must not exceed 255 characters", nameof(name));
        if (estimatedDurationMinutes < 1 || estimatedDurationMinutes > 600)
            throw new ArgumentException("EstimatedDurationMinutes must be between 1 and 600", nameof(estimatedDurationMinutes));
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        return new Workout
        {
            WorkoutPlanId = workoutPlanId,
            Name = name,
            Description = description,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            Order = order,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Workout Reconstitute(
        Guid id,
        Guid workoutPlanId,
        string name,
        string? description,
        int estimatedDurationMinutes,
        int order,
        DateTime createdAt)
    {
        return new Workout
        {
            Id = id,
            WorkoutPlanId = workoutPlanId,
            Name = name,
            Description = description,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            Order = order,
            CreatedAt = createdAt
        };
    }
}
