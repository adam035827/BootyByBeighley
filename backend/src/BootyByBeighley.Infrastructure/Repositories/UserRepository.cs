using Microsoft.EntityFrameworkCore;
using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Domain;
using BootyByBeighley.Domain.Users;
using BootyByBeighley.Infrastructure.Persistence;

namespace BootyByBeighley.Infrastructure.Repositories;

internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken ct)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        context.Users.Update(user);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var user = await context.Users.FindAsync(new object[] { id }, cancellationToken: ct);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<List<User>> GetAllByRoleAsync(UserRole role, CancellationToken ct)
    {
        return await context.Users
            .Where(u => u.Role == role)
            .ToListAsync(ct);
    }
}
