using System.ComponentModel.DataAnnotations;
using ModernApp.Application.Common.Interfaces;

namespace ModernApp.Application.Features.Movements.Commands;

public record UploadMovementVideoCommand(
    Guid MovementId,
    [StringLength(5000, MinimumLength = 1)] string Caption,
    Stream FileStream,
    string FileName
) : ICommand<MovementVideoUploadDto>;

public sealed class UploadMovementVideoCommandHandler(
    IMovementRepository movementRepository,
    IBlobStorageService blobStorageService)
    : ICommandHandler<UploadMovementVideoCommand, MovementVideoUploadDto>
{
    public async Task<MovementVideoUploadDto> ExecuteAsync(
        UploadMovementVideoCommand command, CancellationToken ct)
    {
        var movement = await movementRepository.GetByIdAsync(command.MovementId, ct);
        if (movement == null)
            throw new InvalidOperationException($"Movement {command.MovementId} not found");

        // Upload to Azure Blob Storage
        var blobName = $"{command.MovementId}/{Guid.NewGuid()}-{command.FileName}";
        var videoUrl = await blobStorageService.UploadVideoAsync(
            "movement-videos", blobName, command.FileStream, ct);

        // Update movement record
        movement.UpdateVideo(videoUrl, command.Caption);
        await movementRepository.UpdateAsync(movement, ct);

        return new MovementVideoUploadDto(movement.Id, videoUrl, command.Caption);
    }
}

public record MovementVideoUploadDto(Guid MovementId, string VideoUrl, string Caption);
