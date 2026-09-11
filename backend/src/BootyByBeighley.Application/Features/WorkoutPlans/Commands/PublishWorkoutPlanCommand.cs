using BootyByBeighley.Application.Common.Interfaces;

namespace BootyByBeighley.Application.Features.WorkoutPlans.Commands;

public record PublishWorkoutPlanCommand(Guid PlanId) : ICommand<bool>;

public sealed class PublishWorkoutPlanCommandHandler(IWorkoutPlanRepository planRepository)
    : ICommandHandler<PublishWorkoutPlanCommand, bool>
{
    public async Task<bool> ExecuteAsync(PublishWorkoutPlanCommand command, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdAsync(command.PlanId, ct);
        if (plan == null)
            throw new InvalidOperationException($"Plan {command.PlanId} not found");

        plan.Publish();
        await planRepository.UpdateAsync(plan, ct);
        return true;
    }
}
