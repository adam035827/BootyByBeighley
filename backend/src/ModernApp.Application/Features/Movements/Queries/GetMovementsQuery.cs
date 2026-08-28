using ModernApp.Application.Common.Interfaces;

namespace ModernApp.Application.Features.Movements.Queries;

public record GetMovementsQuery : IQuery<List<MovementItemDto>>;

public record MovementItemDto(
    Guid Id,
    string Name,
    string Description,
    int DefaultSets,
    int DefaultReps,
    int DefaultRestSeconds,
    string? VideoUrl,
    string? VideoCaption);

public class GetMovementsQueryHandler : IQueryHandler<GetMovementsQuery, List<MovementItemDto>>
{
    private readonly IMovementRepository _movementRepository;

    public GetMovementsQueryHandler(IMovementRepository movementRepository)
    {
        _movementRepository = movementRepository;
    }

    public async Task<List<MovementItemDto>> ExecuteAsync(GetMovementsQuery query, CancellationToken ct)
    {
        var movements = await _movementRepository.GetAllAsync(ct);

        return movements
            .OrderBy(m => m.Name)
            .Select(m => new MovementItemDto(
                Id: m.Id,
                Name: m.Name,
                Description: m.Description ?? string.Empty,
                DefaultSets: m.DefaultSets,
                DefaultReps: m.DefaultReps,
                DefaultRestSeconds: m.DefaultRestSeconds ?? 60,
                VideoUrl: m.VideoUrl,
                VideoCaption: m.VideoCaption))
            .ToList();
    }
}
