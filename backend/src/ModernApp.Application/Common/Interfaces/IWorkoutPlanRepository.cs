using ModernApp.Domain.WorkoutPlans;

namespace ModernApp.Application.Common.Interfaces;

public interface IWorkoutPlanRepository
{
    Task AddAsync(WorkoutPlan plan, CancellationToken ct);
    Task UpdateAsync(WorkoutPlan plan, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<WorkoutPlan?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<WorkoutPlan?> GetByIdWithWorkoutsAsync(Guid id, CancellationToken ct);
    Task<List<WorkoutPlan>> GetAllAsync(CancellationToken ct);
    Task<List<WorkoutPlan>> GetPublishedAsync(CancellationToken ct);
}
