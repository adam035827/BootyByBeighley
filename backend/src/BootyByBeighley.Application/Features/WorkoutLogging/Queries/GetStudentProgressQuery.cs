using BootyByBeighley.Application.Common.Interfaces;

namespace BootyByBeighley.Application.Features.WorkoutLogging.Queries;

public record GetStudentProgressQuery(Guid StudentId) : IQuery<StudentProgressDto>;

public sealed class GetStudentProgressQueryHandler(
    IWorkoutLogRepository logRepository,
    IPersonalRecordRepository prRepository)
    : IQueryHandler<GetStudentProgressQuery, StudentProgressDto>
{
    public async Task<StudentProgressDto> ExecuteAsync(
        GetStudentProgressQuery query, CancellationToken ct)
    {
        var completedCount = await logRepository.CountCompletedByStudentAsync(query.StudentId, ct);
        var missedCount = await logRepository.CountMissedByStudentAsync(query.StudentId, ct);
        var personalRecords = await prRepository.GetByStudentAsync(query.StudentId, ct);

        return new StudentProgressDto(completedCount, missedCount, personalRecords.Count);
    }
}

public record StudentProgressDto(int WorkoutsCompleted, int WorkoutsMissed, int PersonalRecords);
