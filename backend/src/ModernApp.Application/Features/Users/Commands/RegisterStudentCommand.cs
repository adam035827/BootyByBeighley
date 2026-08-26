using System.ComponentModel.DataAnnotations;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.Users;

namespace ModernApp.Application.Features.Users.Commands;

public record RegisterStudentCommand(
    [EmailAddress] string Email,
    [StringLength(100, MinimumLength = 1)] string FirstName,
    [StringLength(100, MinimumLength = 1)] string LastName
) : ICommand<UserDto>;

public sealed class RegisterStudentCommandHandler(IUserRepository userRepository)
    : ICommandHandler<RegisterStudentCommand, UserDto>
{
    public async Task<UserDto> ExecuteAsync(RegisterStudentCommand command, CancellationToken ct)
    {
        // Check if user already exists
        var existing = await userRepository.GetByEmailAsync(command.Email, ct);
        if (existing != null)
            throw new InvalidOperationException($"User with email {command.Email} already exists");

        var user = User.CreateStudent(command.Email, command.FirstName, command.LastName);
        await userRepository.AddAsync(user, ct);

        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.SubscriptionStatus.ToString());
    }
}

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string SubscriptionStatus);
