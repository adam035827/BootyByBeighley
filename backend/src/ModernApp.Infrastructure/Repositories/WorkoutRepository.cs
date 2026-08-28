using Microsoft.EntityFrameworkCore;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.WorkoutPlans;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class WorkoutRepository(AppDbContext context) : IWorkoutRepository
{
    public async Task AddAsync(Workout workout, CancellationToken ct)
    {
        context.Workouts.Add(workout);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Workout workout, CancellationToken ct)
    {
        context.Workouts.Update(workout);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var workout = await context.Workouts.FindAsync(new object[] { id }, cancellationToken: ct);
        if (workout != null)
        {
            context.Workouts.Remove(workout);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<Workout?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Workouts.FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<List<Workout>> GetByPlanIdAsync(Guid planId, CancellationToken ct)
    {
        return await context.Workouts
            .Where(w => w.WorkoutPlanId == planId)
            .OrderBy(w => w.Order)
            .ToListAsync(ct);
    }
}
