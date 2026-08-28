using ModernApp.Domain.WorkoutLogs;

namespace ModernApp.Application.Common.Interfaces;

public interface IWorkoutLogRepository
{
    Task AddAsync(WorkoutLogEntry logEntry, CancellationToken ct);
    Task UpdateAsync(WorkoutLogEntry logEntry, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<WorkoutLogEntry?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<WorkoutLogEntry>> GetByStudentIdAsync(Guid studentId, CancellationToken ct);
    Task<int> CountCompletedByStudentAsync(Guid studentId, CancellationToken ct);
    Task<int> CountMissedByStudentAsync(Guid studentId, CancellationToken ct);
    Task<WorkoutLogEntry?> GetLastLogForStudentAsync(Guid studentId, CancellationToken ct);
}
