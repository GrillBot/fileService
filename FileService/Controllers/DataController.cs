using FileService.Managers;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers;

[ApiController]
[Route("api/data")]
public class DataController : Controller
{
    private StorageManager StorageManager { get; }

    public DataController(StorageManager storageManager)
    {
        StorageManager = storageManager;
    }

    [HttpPost]
    public async Task<ActionResult> StoreFileAsync(IFormFile file)
    {
        await StorageManager.UploadFileAsync(file);
        return Ok();
    }
}
