namespace BootyByBeighley.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadVideoAsync(
        string containerName,
        string blobName,
        Stream fileStream,
        CancellationToken ct);

    Task<Uri> GetSignedUrlAsync(
        string containerName,
        string blobName,
        int expirationMinutes = 60,
        CancellationToken ct = default);

    Task DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken ct);
}
