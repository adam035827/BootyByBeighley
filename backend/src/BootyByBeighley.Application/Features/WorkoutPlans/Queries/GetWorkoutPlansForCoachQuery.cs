using BootyByBeighley.Application.Common.Interfaces;

namespace BootyByBeighley.Application.Features.WorkoutPlans.Queries;

public record GetWorkoutPlansForCoachQuery : IQuery<List<WorkoutPlanItemDto>>;

public record WorkoutPlanItemDto(
    Guid Id,
    string Name,
    string Difficulty,
    int DurationWeeks,
    int TrainingFrequencyDaysPerWeek,
    string Description,
    int EnrolledStudentsCount);

public class GetWorkoutPlansForCoachQueryHandler : IQueryHandler<GetWorkoutPlansForCoachQuery, List<WorkoutPlanItemDto>>
{
    private readonly IWorkoutPlanRepository _workoutPlanRepository;
    private readonly IPlanEnrollmentRepository _planEnrollmentRepository;

    public GetWorkoutPlansForCoachQueryHandler(
        IWorkoutPlanRepository workoutPlanRepository,
        IPlanEnrollmentRepository planEnrollmentRepository)
    {
        _workoutPlanRepository = workoutPlanRepository;
        _planEnrollmentRepository = planEnrollmentRepository;
    }

    public async Task<List<WorkoutPlanItemDto>> ExecuteAsync(GetWorkoutPlansForCoachQuery query, CancellationToken ct)
    {
        var plans = await _workoutPlanRepository.GetAllAsync(ct);

        var result = new List<WorkoutPlanItemDto>();

        foreach (var plan in plans.OrderBy(p => p.CreatedAt))
        {
            var enrollmentCount = await _planEnrollmentRepository.GetEnrolledCountByPlanAsync(plan.Id, ct);

            result.Add(new WorkoutPlanItemDto(
                Id: plan.Id,
                Name: plan.Name,
                Difficulty: plan.Difficulty.ToString(),
                DurationWeeks: plan.DurationWeeks,
                TrainingFrequencyDaysPerWeek: plan.TrainingFrequencyDaysPerWeek,
                Description: plan.Description ?? string.Empty,
                EnrolledStudentsCount: enrollmentCount));
        }

        return result;
    }
}
