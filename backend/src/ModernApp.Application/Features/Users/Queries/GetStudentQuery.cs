using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain;
using ModernApp.Domain.Users;

namespace ModernApp.Application.Features.Users.Queries;

public record GetStudentQuery(Guid StudentId) : IQuery<StudentDetailsDto?>;

public sealed class GetStudentQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetStudentQuery, StudentDetailsDto?>
{
    public async Task<StudentDetailsDto?> ExecuteAsync(GetStudentQuery query, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(query.StudentId, ct);
        if (user == null || user.Role != UserRole.Student)
            return null;

        return new StudentDetailsDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt);
    }
}

public record StudentDetailsDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime EnrolledAt);
