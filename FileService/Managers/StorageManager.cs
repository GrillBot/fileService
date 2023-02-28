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
}
