using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Document.Services.DocumentManagementAPI.Models.DTOs;
using Document.Services.DocumentManagementAPI.Services;
using Document.Services.DocumentManagementAPI.Services.IServices;
using System.Security.Claims; 
using Document.Services.DocumentManagementAPI.Models;

namespace DocumentManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All document actions require authentication
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
    private bool IsCurrentUserAdmin() => User.IsInRole(Role.Admin.ToString());


    // POST api/documents/upload
    [HttpPost("upload")]
    [Authorize(Policy = "EditorOrAdmin")] // Only Editors or Admins can upload
    [RequestSizeLimit(100_000_000)] // Limit file size e.g. 100MB, configure as needed
    public async Task<IActionResult> UploadDocument([FromForm] DocumentCreateDto createDto)
    {
        if (createDto.File == null || createDto.File.Length == 0)
        {
            return BadRequest("No file uploaded or file is empty.");
        }

        var userId = GetCurrentUserId();
        var (document, message) = await _documentService.UploadDocumentAsync(createDto.File, userId);

        if (document == null)
        {
            return BadRequest(new { Message = message });
        }
        // Return DocumentMetadataDto or full Document object
        return CreatedAtAction(nameof(GetDocumentMetadata), new { id = document.Id },
            new DocumentMetadataDto(document.Id, document.FileName, document.ContentType, document.FileSize, document.UploadedAt, document.UploadedByUserId));
    }

    // GET api/documents/{id}/metadata
    [HttpGet("{id}/metadata")]
    [Authorize(Policy = "EditorOrAdmin")] // Or allow Viewers if they should see metadata
    public async Task<IActionResult> GetDocumentMetadata(Guid id)
    {
        var metadata = await _documentService.GetDocumentMetadataAsync(id);
        if (metadata == null)
        {
            return NotFound();
        }
        // Add authorization check: does current user have permission to view this document's metadata?
        return Ok(metadata);
    }

    // GET api/documents/{id}/download
    [HttpGet("{id}/download")]
    [Authorize(Policy = "EditorOrAdmin")] // Or Viewers if they should download
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var fileData = await _documentService.GetDocumentFileAsync(id);
        if (fileData == null)
        {
            return NotFound("Document not found or file is missing.");
        }
        // Add authorization check here too
        return File(fileData.Value.fileStream, fileData.Value.contentType, fileData.Value.fileName);
    }


    // GET api/documents
    [HttpGet]
    [Authorize(Policy = "EditorOrAdmin")] // Or Viewers if they should list documents
    public async Task<IActionResult> GetAllDocuments([FromQuery] Guid? userId = null)
    {
        // If not admin, user can only see their own documents if userId query param is used for that
        // Or, by default, show only user's own documents if userId is not provided
        var documents = await _documentService.GetAllDocumentsAsync(IsCurrentUserAdmin() ? userId : GetCurrentUserId());
        return Ok(documents);
    }

    // PUT api/documents/{id} - Placeholder for update
    [HttpPut("{id}")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> UpdateDocument(Guid id /*, [FromBody] DocumentUpdateDto updateDto */)
    {
        var currentUserId = GetCurrentUserId();
        // bool success = await _documentService.UpdateDocumentAsync(id, updateDto, currentUserId);
        // For now, a simple placeholder:
        bool success = await _documentService.UpdateDocumentAsync(id, currentUserId);
        if (!success)
        {
            return NotFound(new { Message = "Document not found or update failed (check permissions)." });
        }
        return NoContent(); // Or return updated document metadata
    }

    // DELETE api/documents/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorOrAdmin")] // Typically editors can delete their own, admins can delete any
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsCurrentUserAdmin();

        var success = await _documentService.DeleteDocumentAsync(id, currentUserId, isAdmin);
        if (!success)
        {
            return NotFound(new { Message = "Document not found or delete failed (check permissions)." });
        }
        return NoContent();
    }
}