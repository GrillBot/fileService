using FileService.Managers;
using FileService.Models.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers;

[ApiController]
[Route("api/diag")]
public class DiagnosticController : Controller
{
    private DiagnosticManager DiagnosticManager { get; }

    public DiagnosticController(DiagnosticManager diagnosticManager)
    {
        DiagnosticManager = diagnosticManager;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<DiagnosticInfo> GetInfo()
    {
        var result = DiagnosticManager.GetInfo();
        return Ok(result);
    }
}
