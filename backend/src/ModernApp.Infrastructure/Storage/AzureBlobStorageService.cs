using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ModernApp.Application.Common.Interfaces;

namespace ModernApp.Infrastructure.Storage;

internal sealed class AzureBlobStorageService(BlobContainerClient containerClient) : IBlobStorageService
{
    public async Task<string> UploadVideoAsync(
        string containerName,
        string blobName,
        Stream fileStream,
        CancellationToken ct)
    {
        var blobClient = containerClient.GetBlobClient(blobName);
        
        fileStream.Seek(0, SeekOrigin.Begin);
        await blobClient.UploadAsync(fileStream, overwrite: true, ct);
        
        return blobClient.Uri.ToString();
    }

    public async Task<Uri> GetSignedUrlAsync(
        string containerName,
        string blobName,
        int expirationMinutes = 60,
        CancellationToken ct = default)
    {
        var blobClient = containerClient.GetBlobClient(blobName);
        
        // Generate SAS URI for read-only access
        var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(expirationMinutes))
        {
            BlobContainerName = containerClient.Name,
            BlobName = blobName
        };

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        
        return await Task.FromResult(sasUri);
    }

    public async Task DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken ct)
    {
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteAsync(cancellationToken: ct);
    }
}
