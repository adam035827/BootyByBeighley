using System.ComponentModel.DataAnnotations;
using BootyByBeighley.Application.Common.Interfaces;
using BootyByBeighley.Domain.Movements;

namespace BootyByBeighley.Application.Features.Movements.Commands;

public record CreateMovementCommand(
    [StringLength(255, MinimumLength = 1)] string Name,
    string? Description,
    [Range(1, 20)] int DefaultSets,
    [Range(1, 100)] int DefaultReps,
    [Range(0, 300)] int? DefaultRestSeconds
) : ICommand<MovementDto>;

public sealed class CreateMovementCommandHandler(IMovementRepository movementRepository)
    : ICommandHandler<CreateMovementCommand, MovementDto>
{
    public async Task<MovementDto> ExecuteAsync(CreateMovementCommand command, CancellationToken ct)
    {
        var movement = Movement.Create(
            command.Name,
            command.DefaultSets,
            command.DefaultReps,
            command.Description,
            command.DefaultRestSeconds);

        await movementRepository.AddAsync(movement, ct);

        return new MovementDto(
            movement.Id,
            movement.Name,
            movement.DefaultSets,
            movement.DefaultReps,
            null);
    }
}

public record MovementDto(
    Guid Id,
    string Name,
    int DefaultSets,
    int DefaultReps,
    string? VideoUrl);
