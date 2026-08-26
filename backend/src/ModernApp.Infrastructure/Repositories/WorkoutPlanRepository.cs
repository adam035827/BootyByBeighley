using Microsoft.EntityFrameworkCore;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.WorkoutPlans;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class WorkoutPlanRepository(AppDbContext context) : IWorkoutPlanRepository
{
    public async Task AddAsync(WorkoutPlan plan, CancellationToken ct)
    {
        context.WorkoutPlans.Add(plan);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WorkoutPlan plan, CancellationToken ct)
    {
        context.WorkoutPlans.Update(plan);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var plan = await context.WorkoutPlans.FindAsync(new object[] { id }, cancellationToken: ct);
        if (plan != null)
        {
            context.WorkoutPlans.Remove(plan);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<WorkoutPlan?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.WorkoutPlans.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<WorkoutPlan?> GetByIdWithWorkoutsAsync(Guid id, CancellationToken ct)
    {
        return await context.WorkoutPlans
            .Include(p => p.Workouts)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<WorkoutPlan>> GetAllAsync(CancellationToken ct)
    {
        return await context.WorkoutPlans.ToListAsync(ct);
    }

    public async Task<List<WorkoutPlan>> GetPublishedAsync(CancellationToken ct)
    {
        return await context.WorkoutPlans
            .Where(p => p.IsPublished)
            .ToListAsync(ct);
    }
}
