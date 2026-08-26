using Microsoft.EntityFrameworkCore;
using ModernApp.Application.Common.Interfaces;
using ModernApp.Domain.PersonalRecords;
using ModernApp.Infrastructure.Persistence;

namespace ModernApp.Infrastructure.Repositories;

internal sealed class PersonalRecordRepository(AppDbContext context) : IPersonalRecordRepository
{
    public async Task AddAsync(PersonalRecord pr, CancellationToken ct)
    {
        context.PersonalRecords.Add(pr);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PersonalRecord pr, CancellationToken ct)
    {
        context.PersonalRecords.Update(pr);
        await context.SaveChangesAsync(ct);
    }

    public async Task<PersonalRecord?> GetByStudentAndMovementAsync(Guid studentId, Guid movementId, CancellationToken ct)
    {
        return await context.PersonalRecords
            .FirstOrDefaultAsync(pr => pr.UserId == studentId && pr.MovementId == movementId, ct);
    }

    public async Task<List<PersonalRecord>> GetByStudentAsync(Guid studentId, CancellationToken ct)
    {
        return await context.PersonalRecords
            .Where(pr => pr.UserId == studentId)
            .ToListAsync(ct);
    }
}
