using Azure;
using Azure.Storage.Blobs;

namespace FileService.Managers;

public class StorageManager
{
    private IWebHostEnvironment Environment { get; }
    private BlobContainerClient Client { get; }

    private string ContainerName => Environment.EnvironmentName.ToLower();

    public StorageManager(IWebHostEnvironment environment, IConfiguration configuration)
    {
        Environment = environment;

        var connectionString = configuration.GetConnectionString("StorageAccount");
        Client = new BlobContainerClient(connectionString, ContainerName);
    }

    public async Task UploadFileAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        await Client.UploadBlobAsync(file.FileName, stream);
    }

    public async Task<(byte[] content, string contentType)?> DownloadFileAsync(string filename)
    {
        var blobClient = Client.GetBlobClient(filename);

        try
        {
            var response = await blobClient.DownloadContentAsync();
            if (response == null) return null;

            var data = response.Value.Content.ToArray();
            return (data, response.Value.Details.ContentType);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "BlobNotFound")
        {
            return null;
        }
    }
}
