using System.ComponentModel.DataAnnotations;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain;
using ModernApp.Domain.WorkoutPlans;

namespace ModernApp.Application.Features.WorkoutPlans.Commands;

public record CreateWorkoutPlanCommand(
    [StringLength(255, MinimumLength = 1)] string Name,
    string? Description,
    Difficulty Difficulty,
    [Range(1, 52)] int DurationWeeks,
    [Range(2, 5)] int TrainingFrequencyDaysPerWeek
) : ICommand<WorkoutPlanDto>;

public sealed class CreateWorkoutPlanCommandHandler(IWorkoutPlanRepository planRepository)
    : ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto>
{
    public async Task<WorkoutPlanDto> ExecuteAsync(
        CreateWorkoutPlanCommand command, CancellationToken ct)
    {
        var plan = WorkoutPlan.Create(
            command.Name,
            command.Difficulty,
            command.DurationWeeks,
            command.TrainingFrequencyDaysPerWeek,
            command.Description);

        await planRepository.AddAsync(plan, ct);
        return new WorkoutPlanDto(plan.Id, plan.Name, plan.Difficulty.ToString(), plan.IsPublished);
    }
}

public record WorkoutPlanDto(Guid Id, string Name, string Difficulty, bool IsPublished);
