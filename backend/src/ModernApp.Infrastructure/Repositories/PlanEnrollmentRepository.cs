using Microsoft.EntityFrameworkCore;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.PlanEnrollments;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class PlanEnrollmentRepository(AppDbContext context) : IPlanEnrollmentRepository
{
    public async Task AddAsync(PlanEnrollment enrollment, CancellationToken ct)
    {
        context.PlanEnrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PlanEnrollment enrollment, CancellationToken ct)
    {
        context.PlanEnrollments.Update(enrollment);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var enrollment = await context.PlanEnrollments.FindAsync(new object[] { id }, cancellationToken: ct);
        if (enrollment != null)
        {
            context.PlanEnrollments.Remove(enrollment);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<PlanEnrollment?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.PlanEnrollments.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<PlanEnrollment>> GetByStudentIdAsync(Guid studentId, CancellationToken ct)
    {
        return await context.PlanEnrollments
            .Where(e => e.UserId == studentId)
            .ToListAsync(ct);
    }

    public async Task<PlanEnrollment?> GetByStudentAndPlanAsync(Guid studentId, Guid planId, CancellationToken ct)
    {
        return await context.PlanEnrollments
            .FirstOrDefaultAsync(e => e.UserId == studentId && e.WorkoutPlanId == planId, ct);
    }

    public async Task<int> GetEnrolledCountByPlanAsync(Guid planId, CancellationToken ct)
    {
        return await context.PlanEnrollments
            .Where(e => e.WorkoutPlanId == planId)
            .CountAsync(ct);
    }
}
