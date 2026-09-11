using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Domain.PersonalRecords;
using BootyByBeighley.Domain.WorkoutLogs;

namespace BootyByBeighley.Application.Features.WorkoutLogging.Commands;

public record LogWorkoutCompletionCommand(
    Guid StudentId,
    Guid WorkoutId,
    Guid PlanEnrollmentId,
    List<LoggedSetInput> LoggedSets,
    string? Notes
) : ICommand<WorkoutLogEntryDto>;

public record LoggedSetInput(
    Guid WorkoutMovementId,
    int SetNumber,
    int RepsCompleted,
    decimal? WeightUsed,
    int? DurationSeconds);

public sealed class LogWorkoutCompletionCommandHandler(
    IWorkoutLogRepository logRepository,
    IPersonalRecordRepository prRepository)
    : ICommandHandler<LogWorkoutCompletionCommand, WorkoutLogEntryDto>
{
    public async Task<WorkoutLogEntryDto> ExecuteAsync(
        LogWorkoutCompletionCommand command, CancellationToken ct)
    {
        var loggedSetEntries = command.LoggedSets
            .Select(s => WorkoutLogSetEntry.Create(
                Guid.Empty, // Will be set by EF Core
                s.WorkoutMovementId,
                s.SetNumber,
                s.RepsCompleted,
                s.WeightUsed,
                s.DurationSeconds))
            .ToList();

        var logEntry = WorkoutLogEntry.CreateCompleted(
            command.StudentId,
            command.WorkoutId,
            command.PlanEnrollmentId,
            loggedSetEntries,
            command.Notes);

        await logRepository.AddAsync(logEntry, ct);

        // Check for new PRs
        var prDetections = new List<PersonalRecordDetectionDto>();

        foreach (var setEntry in logEntry.LoggedSets)
        {
            if (setEntry.WeightUsed.HasValue && setEntry.WeightUsed > 0)
            {
                var existingPr = await prRepository
                    .GetByStudentAndMovementAsync(
                        command.StudentId,
                        setEntry.WorkoutMovementId,
                        ct);

                if (existingPr == null || setEntry.WeightUsed > existingPr.MaxWeightKg)
                {
                    var newPr = PersonalRecord.Create(
                        command.StudentId,
                        setEntry.WorkoutMovementId,
                        setEntry.WeightUsed.Value,
                        setEntry.Id);

                    if (existingPr == null)
                    {
                        await prRepository.AddAsync(newPr, ct);
                    }
                    else
                    {
                        existingPr.UpdateMaxWeight(setEntry.WeightUsed.Value, setEntry.Id);
                        await prRepository.UpdateAsync(existingPr, ct);
                    }

                    prDetections.Add(new PersonalRecordDetectionDto(
                        setEntry.WorkoutMovementId,
                        setEntry.WeightUsed.Value));
                }
            }
        }

        return new WorkoutLogEntryDto(
            logEntry.Id,
            logEntry.CompletedAt,
            logEntry.Status.ToString(),
            prDetections);
    }
}

public record WorkoutLogEntryDto(
    Guid Id,
    DateTime CompletedAt,
    string Status,
    List<PersonalRecordDetectionDto> NewPRs);

public record PersonalRecordDetectionDto(Guid MovementId, decimal Weight);
