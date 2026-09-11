using BootyByBeighley.Domain.Common;

namespace BootyByBeighley.Domain.MatchingRules;

public class MatchingRule : Entity
{
    public int Priority { get; private set; }
    public string RuleDefinition { get; private set; } = string.Empty; // JSON
    public Guid TargetWorkoutPlanId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private MatchingRule() { }

    public static MatchingRule Create(
        int priority,
        string ruleDefinition,
        Guid targetWorkoutPlanId)
    {
        if (priority < 0)
            throw new ArgumentException("Priority must be non-negative", nameof(priority));
        if (string.IsNullOrWhiteSpace(ruleDefinition))
            throw new ArgumentException("RuleDefinition is required", nameof(ruleDefinition));

        return new MatchingRule
        {
            Priority = priority,
            RuleDefinition = ruleDefinition,
            TargetWorkoutPlanId = targetWorkoutPlanId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static MatchingRule Reconstitute(
        Guid id,
        int priority,
        string ruleDefinition,
        Guid targetWorkoutPlanId,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt)
    {
        return new MatchingRule
        {
            Id = id,
            Priority = priority,
            RuleDefinition = ruleDefinition,
            TargetWorkoutPlanId = targetWorkoutPlanId,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
