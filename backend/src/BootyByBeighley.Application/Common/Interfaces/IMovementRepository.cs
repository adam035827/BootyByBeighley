using BootyByBeighley.Domain.Movements;

namespace BootyByBeighley.Application.Common.Interfaces;

public interface IMovementRepository
{
    Task AddAsync(Movement movement, CancellationToken ct);
    Task UpdateAsync(Movement movement, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<Movement?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Movement>> GetAllAsync(CancellationToken ct);
}
