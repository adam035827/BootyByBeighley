using BootyByBeighley.Domain.WorkoutPlans;

namespace BootyByBeighley.Application.Common.Interfaces;

public interface IWorkoutRepository
{
    Task AddAsync(Workout workout, CancellationToken ct);
    Task UpdateAsync(Workout workout, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<Workout?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Workout>> GetByPlanIdAsync(Guid planId, CancellationToken ct);
}
