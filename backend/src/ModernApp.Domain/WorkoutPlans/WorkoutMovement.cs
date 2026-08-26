using ModernApp.Domain.Common;

namespace ModernApp.Domain.WorkoutPlans;

public class WorkoutMovement : Entity
{
    public Guid WorkoutId { get; private set; }
    public Guid MovementId { get; private set; }
    public int Order { get; private set; }
    public int PrescribedSets { get; private set; }
    public int PrescribedReps { get; private set; }
    public int? PrescribedRestSeconds { get; private set; }

    private WorkoutMovement() { }

    public static WorkoutMovement Create(
        Guid workoutId,
        Guid movementId,
        int order,
        int prescribedSets,
        int prescribedReps,
        int? prescribedRestSeconds = null)
    {
        if (prescribedSets < 1 || prescribedSets > 20)
            throw new ArgumentException("PrescribedSets must be between 1 and 20", nameof(prescribedSets));
        if (prescribedReps < 1 || prescribedReps > 100)
            throw new ArgumentException("PrescribedReps must be between 1 and 100", nameof(prescribedReps));
        if (prescribedRestSeconds.HasValue && (prescribedRestSeconds < 0 || prescribedRestSeconds > 300))
            throw new ArgumentException("PrescribedRestSeconds must be between 0 and 300", nameof(prescribedRestSeconds));
        if (order < 0)
            throw new ArgumentException("Order must be non-negative", nameof(order));

        return new WorkoutMovement
        {
            WorkoutId = workoutId,
            MovementId = movementId,
            Order = order,
            PrescribedSets = prescribedSets,
            PrescribedReps = prescribedReps,
            PrescribedRestSeconds = prescribedRestSeconds
        };
    }

    public static WorkoutMovement Reconstitute(
        Guid id,
        Guid workoutId,
        Guid movementId,
        int order,
        int prescribedSets,
        int prescribedReps,
        int? prescribedRestSeconds)
    {
        return new WorkoutMovement
        {
            Id = id,
            WorkoutId = workoutId,
            MovementId = movementId,
            Order = order,
            PrescribedSets = prescribedSets,
            PrescribedReps = prescribedReps,
            PrescribedRestSeconds = prescribedRestSeconds
        };
    }
}
