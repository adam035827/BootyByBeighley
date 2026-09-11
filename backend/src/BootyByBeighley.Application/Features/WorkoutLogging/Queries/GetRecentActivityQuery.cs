using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Domain.WorkoutPlans;

namespace BootyByBeighley.Application.Features.WorkoutLogging.Queries;

public record GetRecentActivityQuery(int LimitDays = 7) : IQuery<List<ActivityItemDto>>;

public record ActivityItemDto(
    Guid Id,
    Guid UserId,
    string StudentName,
    string StudentEmail,
    string WorkoutName,
    DateTime CompletedAt,
    int TotalSets,
    int TotalMovements);

public class GetRecentActivityQueryHandler : IQueryHandler<GetRecentActivityQuery, List<ActivityItemDto>>
{
    private readonly IWorkoutLogRepository _workoutLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWorkoutRepository _workoutRepository;

    public GetRecentActivityQueryHandler(
        IWorkoutLogRepository workoutLogRepository,
        IUserRepository userRepository,
        IWorkoutRepository workoutRepository)
    {
        _workoutLogRepository = workoutLogRepository;
        _userRepository = userRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task<List<ActivityItemDto>> ExecuteAsync(GetRecentActivityQuery query, CancellationToken ct)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-query.LimitDays);
        var recentLogs = await _workoutLogRepository.GetRecentCompletedLogsAsync(cutoffDate, ct);

        var result = new List<ActivityItemDto>();

        foreach (var log in recentLogs.OrderByDescending(l => l.CompletedAt))
        {
            var user = await _userRepository.GetByIdAsync(log.UserId, ct);
            var workout = await _workoutRepository.GetByIdAsync(log.WorkoutId, ct);

            var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User";
            var userEmail = user?.Email ?? "unknown@example.com";
            var workoutName = workout?.Name ?? "Unknown Workout";
            var totalSets = log.LoggedSets?.Count ?? 0;
            var totalMovements = log.LoggedSets?.Select(s => s.WorkoutMovementId).Distinct().Count() ?? 0;

            result.Add(new ActivityItemDto(
                Id: log.Id,
                UserId: log.UserId,
                StudentName: userName,
                StudentEmail: userEmail,
                WorkoutName: workoutName,
                CompletedAt: log.CompletedAt,
                TotalSets: totalSets,
                TotalMovements: totalMovements));
        }

        return result;
    }
}
