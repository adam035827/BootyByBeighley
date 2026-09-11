using BootyByBeighley.Domain.PlanEnrollments;

namespace BootyByBeighley.Application.Common.Interfaces;

public interface IPlanEnrollmentRepository
{
    Task AddAsync(PlanEnrollment enrollment, CancellationToken ct);
    Task UpdateAsync(PlanEnrollment enrollment, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<PlanEnrollment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<PlanEnrollment>> GetByStudentIdAsync(Guid studentId, CancellationToken ct);
    Task<PlanEnrollment?> GetByStudentAndPlanAsync(Guid studentId, Guid planId, CancellationToken ct);
    Task<int> GetEnrolledCountByPlanAsync(Guid planId, CancellationToken ct);
}
