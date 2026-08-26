using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.PlanEnrollments;

namespace ModernApp.Application.Features.PlanEnrollment.Commands;

public record EnrollStudentToPlanCommand(
    Guid StudentId,
    Guid WorkoutPlanId,
    List<DayOfWeek> SelectedTrainingDays
) : ICommand<PlanEnrollmentDto>;

public sealed class EnrollStudentToPlanCommandHandler(
    IPlanEnrollmentRepository enrollmentRepository,
    IWorkoutPlanRepository planRepository)
    : ICommandHandler<EnrollStudentToPlanCommand, PlanEnrollmentDto>
{
    public async Task<PlanEnrollmentDto> ExecuteAsync(
        EnrollStudentToPlanCommand command, CancellationToken ct)
    {
        var plan = await planRepository.GetByIdAsync(command.WorkoutPlanId, ct);
        if (plan == null)
            throw new InvalidOperationException($"Plan {command.WorkoutPlanId} not found");

        if (command.SelectedTrainingDays.Count != plan.TrainingFrequencyDaysPerWeek)
            throw new InvalidOperationException(
                $"Plan requires {plan.TrainingFrequencyDaysPerWeek} training days, but {command.SelectedTrainingDays.Count} were provided");

        var enrollment = Domain.PlanEnrollments.PlanEnrollment.Create(
            command.StudentId,
            command.WorkoutPlanId,
            command.SelectedTrainingDays,
            plan.DurationWeeks);

        await enrollmentRepository.AddAsync(enrollment, ct);

        return new PlanEnrollmentDto(
            enrollment.Id,
            command.StudentId,
            command.WorkoutPlanId,
            enrollment.Status.ToString());
    }
}

public record PlanEnrollmentDto(Guid Id, Guid StudentId, Guid PlanId, string Status);
