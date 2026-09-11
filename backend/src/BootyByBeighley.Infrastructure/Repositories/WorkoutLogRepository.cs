using Microsoft.EntityFrameworkCore;
using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Domain;
using BootyByBeighley.Domain.WorkoutLogs;
using BootyByBeighley.Infrastructure.Persistence;

namespace BootyByBeighley.Infrastructure.Repositories;

internal sealed class WorkoutLogRepository(AppDbContext context) : IWorkoutLogRepository
{
    public async Task AddAsync(WorkoutLogEntry logEntry, CancellationToken ct)
    {
        context.WorkoutLogEntries.Add(logEntry);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WorkoutLogEntry logEntry, CancellationToken ct)
    {
        context.WorkoutLogEntries.Update(logEntry);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var logEntry = await context.WorkoutLogEntries.FindAsync(new object[] { id }, cancellationToken: ct);
        if (logEntry != null)
        {
            context.WorkoutLogEntries.Remove(logEntry);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<WorkoutLogEntry?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.WorkoutLogEntries
            .Include(e => e.LoggedSets)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<WorkoutLogEntry>> GetByStudentIdAsync(Guid studentId, CancellationToken ct)
    {
        return await context.WorkoutLogEntries
            .Where(e => e.UserId == studentId)
            .Include(e => e.LoggedSets)
            .ToListAsync(ct);
    }

    public async Task<int> CountCompletedByStudentAsync(Guid studentId, CancellationToken ct)
    {
        return await context.WorkoutLogEntries
            .CountAsync(e => e.UserId == studentId && e.Status == WorkoutStatus.Completed, ct);
    }

    public async Task<int> CountMissedByStudentAsync(Guid studentId, CancellationToken ct)
    {
        return await context.WorkoutLogEntries
            .CountAsync(e => e.UserId == studentId && e.Status == WorkoutStatus.Missed, ct);
    }

    public async Task<WorkoutLogEntry?> GetLastLogForStudentAsync(Guid studentId, CancellationToken ct)
    {
        return await context.WorkoutLogEntries
            .Where(e => e.UserId == studentId)
            .OrderByDescending(e => e.CompletedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<WorkoutLogEntry>> GetRecentCompletedLogsAsync(DateTime since, CancellationToken ct)
    {
        return await context.WorkoutLogEntries
            .Where(e => e.Status == WorkoutStatus.Completed && e.CompletedAt >= since)
            .Include(e => e.LoggedSets)
            .OrderByDescending(e => e.CompletedAt)
            .ToListAsync(ct);
    }
}
