using ModernApp.Domain.Common;

namespace ModernApp.Domain.PlanEnrollments;

public class PlanEnrollment : Entity
{
    public Guid UserId { get; private set; }
    public Guid WorkoutPlanId { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime PlannedEndDate { get; private set; }
    public string SelectedTrainingDays { get; private set; } = string.Empty; // JSON array of DayOfWeek
    public PlanEnrollmentStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private PlanEnrollment() { }

    public static PlanEnrollment Create(
        Guid userId,
        Guid workoutPlanId,
        List<DayOfWeek> selectedTrainingDays,
        int durationWeeks,
        DateTime? startDate = null)
    {
        if (selectedTrainingDays == null || selectedTrainingDays.Count == 0)
            throw new ArgumentException("SelectedTrainingDays cannot be empty", nameof(selectedTrainingDays));
        if (durationWeeks < 1)
            throw new ArgumentException("DurationWeeks must be positive", nameof(durationWeeks));

        var enrollmentDate = startDate ?? DateTime.UtcNow;
        if (enrollmentDate.Kind != DateTimeKind.Utc)
            throw new ArgumentException("StartDate must be UTC", nameof(startDate));

        var selectedDaysJson = System.Text.Json.JsonSerializer.Serialize(selectedTrainingDays);

        return new PlanEnrollment
        {
            UserId = userId,
            WorkoutPlanId = workoutPlanId,
            EnrolledAt = enrollmentDate,
            StartDate = enrollmentDate,
            PlannedEndDate = enrollmentDate.AddDays(durationWeeks * 7),
            SelectedTrainingDays = selectedDaysJson,
            Status = PlanEnrollmentStatus.Active,
            CompletedAt = null
        };
    }

    public static PlanEnrollment Reconstitute(
        Guid id,
        Guid userId,
        Guid workoutPlanId,
        DateTime enrolledAt,
        DateTime startDate,
        DateTime plannedEndDate,
        string selectedTrainingDays,
        PlanEnrollmentStatus status,
        DateTime? completedAt)
    {
        return new PlanEnrollment
        {
            Id = id,
            UserId = userId,
            WorkoutPlanId = workoutPlanId,
            EnrolledAt = enrolledAt,
            StartDate = startDate,
            PlannedEndDate = plannedEndDate,
            SelectedTrainingDays = selectedTrainingDays,
            Status = status,
            CompletedAt = completedAt
        };
    }

    public void Complete()
    {
        Status = PlanEnrollmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        Status = PlanEnrollmentStatus.Archived;
    }
}
