using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Document.Services.IngestionManagementAPI.Models.DTOs;
using System.Security.Claims;
using Document.Services.IngestionManagementAPI.Services.IServices;
namespace Document.Services.IngestionManagementAPI.Controllers;

[ApiController]
[Route("ingestion-service/api/[controller]")]
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
    [Authorize(Policy = "AdminOnly")] // Only Editors or Admins can trigger ingestion
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

    // POST api/ingestion/status-update
    [HttpPost("status-update")]
    [Authorize(Policy = "AdminOnly")] // Only Admins can send status updates
    public async Task<IActionResult> IngestionStatusWebhook([FromBody] IngestionStatusUpdateDto updateDto)
    {
        if (updateDto == null || string.IsNullOrEmpty(updateDto.ExternalProcessId))
        {
            return BadRequest("Invalid webhook payload.");
        }
        var res = await _ingestionService.UpdateIngestionStatus(GetCurrentUserId(), updateDto);
        return Ok(res);
    }
    
    // This endpoint is for triggering a spring event in the ingestion process
     // POST api/ingestion/status-update
    [HttpPost("{processId}/trigger-spring-event")]
    [Authorize(Policy = "AdminOnly")] // Only Admins can trigger spring event
    public async Task<IActionResult> TriggerSpringService(Guid processId)
    {
        try
        {
            // Validate the processId
            if (processId == Guid.Empty)
            {
                return BadRequest("Invalid process ID.");
            }
            var res = await _ingestionService.TriggerSpringServiceAsync(processId);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }
}
