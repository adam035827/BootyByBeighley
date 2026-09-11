using BootyByBeighley.Domain.Common;

namespace BootyByBeighley.Domain.PersonalRecords;

public class PersonalRecord : Entity
{
    public Guid UserId { get; private set; }
    public Guid MovementId { get; private set; }
    public decimal MaxWeightKg { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public Guid? WorkoutLogSetEntryId { get; private set; }

    private PersonalRecord() { }

    public static PersonalRecord Create(
        Guid userId,
        Guid movementId,
        decimal maxWeightKg,
        Guid? workoutLogSetEntryId = null)
    {
        if (maxWeightKg <= 0)
            throw new ArgumentException("MaxWeightKg must be positive", nameof(maxWeightKg));

        return new PersonalRecord
        {
            UserId = userId,
            MovementId = movementId,
            MaxWeightKg = maxWeightKg,
            LoggedAt = DateTime.UtcNow,
            WorkoutLogSetEntryId = workoutLogSetEntryId
        };
    }

    public static PersonalRecord Reconstitute(
        Guid id,
        Guid userId,
        Guid movementId,
        decimal maxWeightKg,
        DateTime loggedAt,
        Guid? workoutLogSetEntryId)
    {
        return new PersonalRecord
        {
            Id = id,
            UserId = userId,
            MovementId = movementId,
            MaxWeightKg = maxWeightKg,
            LoggedAt = loggedAt,
            WorkoutLogSetEntryId = workoutLogSetEntryId
        };
    }

    public void UpdateMaxWeight(decimal newMaxWeight, Guid? workoutLogSetEntryId = null)
    {
        if (newMaxWeight <= MaxWeightKg)
            throw new ArgumentException("New weight must exceed current maximum", nameof(newMaxWeight));

        MaxWeightKg = newMaxWeight;
        LoggedAt = DateTime.UtcNow;
        WorkoutLogSetEntryId = workoutLogSetEntryId;
    }
}
