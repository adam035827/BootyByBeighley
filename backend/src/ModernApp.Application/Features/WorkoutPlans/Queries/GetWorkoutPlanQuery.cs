using ModernApp.Application.Common.Interfaces;

namespace ModernApp.Application.Features.WorkoutPlans.Queries;

public record GetWorkoutPlanQuery(Guid PlanId) : IQuery<WorkoutPlanDetailDto?>;

public sealed class GetWorkoutPlanQueryHandler(IWorkoutPlanRepository planRepository)
    : IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDetailDto?>
{
    public async Task<WorkoutPlanDetailDto?> ExecuteAsync(GetWorkoutPlanQuery query, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdWithWorkoutsAsync(query.PlanId, ct);
        if (plan == null)
            return null;

        var workouts = plan.Workouts
            .OrderBy(w => w.Order)
            .Select(w => new WorkoutSummaryDto(w.Id, w.Name, w.Order))
            .ToList();

        return new WorkoutPlanDetailDto(
            plan.Id,
            plan.Name,
            plan.Difficulty.ToString(),
            plan.DurationWeeks,
            plan.IsPublished,
            workouts);
    }
}

public record WorkoutPlanDetailDto(
    Guid Id,
    string Name,
    string Difficulty,
    int DurationWeeks,
    bool IsPublished,
    List<WorkoutSummaryDto> Workouts);

public record WorkoutSummaryDto(Guid Id, string Name, int Order);
