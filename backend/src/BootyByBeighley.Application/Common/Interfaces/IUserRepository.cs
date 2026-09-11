using BootyByBeighley.Domain;
using BootyByBeighley.Domain.Users;

namespace BootyByBeighley.Application.Common.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<List<User>> GetAllByRoleAsync(UserRole role, CancellationToken ct);
}
