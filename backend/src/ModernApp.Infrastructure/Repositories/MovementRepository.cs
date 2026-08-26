using Microsoft.EntityFrameworkCore;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.Movements;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class MovementRepository(AppDbContext context) : IMovementRepository
{
    public async Task AddAsync(Movement movement, CancellationToken ct)
    {
        context.Movements.Add(movement);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Movement movement, CancellationToken ct)
    {
        context.Movements.Update(movement);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var movement = await context.Movements.FindAsync(new object[] { id }, cancellationToken: ct);
        if (movement != null)
        {
            context.Movements.Remove(movement);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<Movement?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Movements.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<List<Movement>> GetAllAsync(CancellationToken ct)
    {
        return await context.Movements.ToListAsync(ct);
    }
}
