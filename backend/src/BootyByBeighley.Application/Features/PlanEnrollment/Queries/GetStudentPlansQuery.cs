using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Domain;
using BootyByBeighley.Domain.PlanEnrollments;

namespace BootyByBeighley.Application.Features.PlanEnrollment.Queries;

public record GetStudentPlansQuery(Guid StudentId) : IQuery<List<StudentPlanDto>>;

public sealed class GetStudentPlansQueryHandler(IPlanEnrollmentRepository enrollmentRepository)
    : IQueryHandler<GetStudentPlansQuery, List<StudentPlanDto>>
{
    public async Task<List<StudentPlanDto>> ExecuteAsync(
        GetStudentPlansQuery query, CancellationToken ct)
    {
        var enrollments = await enrollmentRepository
            .GetByStudentIdAsync(query.StudentId, ct);

        return enrollments
            .Where(e => e.Status == PlanEnrollmentStatus.Active)
            .Select(e => new StudentPlanDto(e.Id, e.WorkoutPlanId, e.PlannedEndDate))
            .ToList();
    }
}

public record StudentPlanDto(Guid EnrollmentId, Guid PlanId, DateTime PlannedEndDate);
