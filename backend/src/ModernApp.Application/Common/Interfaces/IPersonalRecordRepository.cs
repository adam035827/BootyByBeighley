using ModernApp.Domain.PersonalRecords;

namespace ModernApp.Application.Common.Interfaces;

public interface IPersonalRecordRepository
{
    Task AddAsync(PersonalRecord pr, CancellationToken ct);
    Task UpdateAsync(PersonalRecord pr, CancellationToken ct);
    Task<PersonalRecord?> GetByStudentAndMovementAsync(Guid studentId, Guid movementId, CancellationToken ct);
    Task<List<PersonalRecord>> GetByStudentAsync(Guid studentId, CancellationToken ct);
}
