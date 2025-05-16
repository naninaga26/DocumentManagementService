using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Document.Services.IngestionManagementAPI.Models.DTOs;
using Document.Services.IngestionManagementAPI.Services;
using System.Security.Claims;
using Document.Services.IngestionManagementAPI.Services.IServices;
using Document.Services.IngestionManagementAPI.Models; 
using System.ComponentModel.DataAnnotations;
namespace Document.Services.IngestionManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All ingestion actions require authentication
public class IngestionController : ControllerBase
{
    private readonly IIngestionService _ingestionService;

    public IngestionController(IIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    // POST api/ingestion/trigger
    [HttpPost("trigger")]
    [Authorize(Policy = "EditorOrAdmin")] // Only Editors or Admins can trigger ingestion
    public async Task<IActionResult> TriggerIngestion([FromBody] IngestionTriggerRequestDto requestDto)
    {
        if (requestDto == null || requestDto.DocumentId == Guid.Empty)
        {
            return BadRequest("DocumentId is required.");
        }

        var userId = GetCurrentUserId();
        var (process, message) = await _ingestionService.TriggerIngestionAsync(requestDto.DocumentId, userId);

        if (process == null)
        {
            return BadRequest(new { Message = message });
        }

        var statusDto = new IngestionStatusDto(process.Id, process.DocumentId, process.ExternalProcessId, process.Status, process.StatusDetails, process.LastUpdatedAt);
        return Ok(statusDto); // Or CreatedAtAction if you have a Get specific endpoint
    }

    // GET api/ingestion/{processId}/status
    [HttpGet("{processId}/status")]
    [Authorize(Policy = "EditorOrAdmin")] // Or Viewers if they can see status
    public async Task<IActionResult> GetIngestionStatus(Guid processId)
    {
        var status = await _ingestionService.GetIngestionStatusAsync(processId);
        if (status == null)
        {
            return NotFound();
        }
        // Add authorization: can current user view this specific process's status?
        return Ok(status);
    }

    // GET api/ingestion/processes
    [HttpGet("processes")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> GetAllIngestionProcesses([FromQuery] Guid? documentId = null)
    {
        // Potentially filter by user if not admin, or based on document access
        var processes = await _ingestionService.GetAllIngestionProcessesAsync(documentId);
        return Ok(processes);
    }


    // POST api/ingestion/{processId}/cancel
    [HttpPost("{processId}/cancel")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> CancelIngestion(Guid processId)
    {
        var userId = GetCurrentUserId(); // For audit/permission
        var success = await _ingestionService.CancelIngestionAsync(processId, userId);
        if (!success)
        {
            // Could be NotFound if process doesn't exist, or BadRequest if cannot be cancelled
            return BadRequest(new { Message = "Ingestion process not found or could not be cancelled." });
        }
        return Ok(new { Message = "Ingestion cancellation requested." });
    }

    // This is a WEBHOOK endpoint that Spring Boot would call.
    // It should ideally be secured (e.g., with a shared secret, IP whitelisting, or mutual TLS).
    // For simplicity, we'll make it [AllowAnonymous] but add a note about security.
    [AllowAnonymous] // SECURITY NOTE: Secure this endpoint properly in production!
    [HttpPost("webhook/status-update")]
    public async Task<IActionResult> IngestionStatusWebhook([FromBody] WebhookStatusUpdateDto updateDto)
    {
        if (updateDto == null || string.IsNullOrEmpty(updateDto.ExternalProcessId)) {
            return BadRequest("Invalid webhook payload.");
        }

        // TODO: Add a security check here, e.g., validate a secret header from Spring Boot
        // string sharedSecretFromHeader = Request.Headers["X-Webhook-Secret"];
        // if (sharedSecretFromHeader != _configuration["SpringBootService:WebhookSecret"]) {
        //    return Unauthorized("Invalid webhook secret.");
        // }

        await _ingestionService.UpdateIngestionStatusFromWebhookAsync(
            updateDto.ExternalProcessId,
            updateDto.Status,
            updateDto.Details
        );
        return Ok(new { Message = "Webhook received and processed."});
    }
}
