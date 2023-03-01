using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using FileService.Cache;

namespace FileService.Managers;

public class StorageManager
{
    private IWebHostEnvironment Environment { get; }
    private BlobContainerClient Client { get; }
    private StorageCacheManager StorageCache { get; }

    private string ContainerName => Environment.EnvironmentName.ToLower();

    public StorageManager(IWebHostEnvironment environment, IConfiguration configuration, StorageCacheManager storageCache)
    {
        Environment = environment;
        StorageCache = storageCache;

        var connectionString = configuration.GetConnectionString("StorageAccount");
        Client = new BlobContainerClient(connectionString, ContainerName);
    }

    public async Task<int> UploadFileAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();

        try
        {
            await Client.UploadBlobAsync(file.FileName, stream);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
        {
            return StatusCodes.Status409Conflict;
        }

        return StatusCodes.Status200OK;
    }

    public async Task<(byte[] content, string contentType)?> DownloadFileAsync(string filename)
    {
        if (StorageCache.TryGet(filename, out var contentType, out var content))
            return (content, contentType);

        var blobClient = Client.GetBlobClient(filename);

        try
        {
            var response = await blobClient.DownloadContentAsync();
            if (response == null) return null;

            content = response.Value.Content.ToArray();
            contentType = response.Value.Details.ContentType;

            StorageCache.Add(filename, contentType, content);
            return (content, contentType);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            return null;
        }
    }

    public string? GenerateLink(string filename)
    {
        var blobClient = Client.GetBlobClient(filename);
        if (!blobClient.CanGenerateSasUri) return null;

        var builder = new BlobSasBuilder
        {
            BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.Now.AddHours(1)
        };
        builder.SetPermissions(BlobSasPermissions.Read);

        var uri = blobClient.GenerateSasUri(builder);
        return uri.ToString();
    }

    public async Task DeleteAsync(string filename)
    {
        var blobClient = Client.GetBlobClient(filename);

        await blobClient.DeleteIfExistsAsync();
        StorageCache.Remove(filename);
    }
}
