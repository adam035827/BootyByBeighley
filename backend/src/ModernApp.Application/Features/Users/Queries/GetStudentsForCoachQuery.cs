using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain;

namespace ModernApp.Application.Features.Users.Queries;

public record GetStudentsForCoachQuery : IQuery<List<StudentRosterItemDto>>;

public sealed class GetStudentsForCoachQueryHandler(
    IUserRepository userRepository,
    IPlanEnrollmentRepository planEnrollmentRepository,
    IWorkoutLogRepository workoutLogRepository)
    : IQueryHandler<GetStudentsForCoachQuery, List<StudentRosterItemDto>>
{
    public async Task<List<StudentRosterItemDto>> ExecuteAsync(GetStudentsForCoachQuery query, CancellationToken ct)
    {
        // Get all students
        var students = await userRepository.GetAllByRoleAsync(UserRole.Student, ct);

        var result = new List<StudentRosterItemDto>();

        foreach (var student in students)
        {
            // Get student's active plan enrollments
            var enrollments = await planEnrollmentRepository.GetByStudentIdAsync(student.Id, ct);
            var activePlans = enrollments
                .Where(e => e.Status == PlanEnrollmentStatus.Active)
                .Select(e => e.WorkoutPlanId)
                .ToList();

            // Get last workout log entry for this student
            var lastWorkout = await workoutLogRepository.GetLastLogForStudentAsync(student.Id, ct);

            result.Add(new StudentRosterItemDto(
                student.Id,
                student.Email,
                student.FirstName,
                student.LastName,
                activePlans.Count,
                lastWorkout?.CompletedAt,
                false, // TODO: Flag for pending feedback
                false  // TODO: Flag for unread PRs
            ));
        }

        return result.OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ToList();
    }
}

public record StudentRosterItemDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    int ActivePlanCount,
    DateTime? LastWorkoutDate,
    bool HasPendingFeedback,
    bool HasUnreadPrs);
