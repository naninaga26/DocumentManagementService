using System.Reflection.Metadata.Ecma335;
using Document.Services.IngestionManagementAPI.Data;
using Document.Services.IngestionManagementAPI.Models;
using Document.Services.IngestionManagementAPI.Models.DTOs;
using Document.Services.IngestionManagementAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
namespace Document.Services.IngestionManagementAPI.Services;

public class IngestionService : IIngestionService
{
    private readonly AppDbContext _db;
    private readonly IHttpHelperService _httpHelperService;
    private readonly IConfiguration _configuration;


    public IngestionService(AppDbContext db, IHttpHelperService httpHelperService, IConfiguration configuration)
    {
        _db = db;
        _httpHelperService = httpHelperService;
        _configuration = configuration;
    }

    public async Task<(IngestionProcess process, string message)> TriggerIngestionAsync(Guid documentId, Guid userId)
    {

        //Create an IngestionProcess record in our DB
        var ingestionProcess = new IngestionProcess
        {
            DocumentId = documentId,
            Status = IngestionStatus.Pending,
            TriggeredByUserId = userId,
            StatusDetails = $"Ingestion process initiated by user {userId}.",
            ExternalProcessId = $"TEMP-{Guid.NewGuid()}" // Temporary, to be updated by Spring Boot
        };
        _db.IngestionProcesses.Add(ingestionProcess);
        await _db.SaveChangesAsync();
        //Call the Spring Boot backend to Trigger the ingestion process

        return (ingestionProcess, "Ingestion process triggered successfully.");

    }

    public async Task<IngestionStatusDto> GetIngestionStatusAsync(Guid processId)
    {
        try
        {
            // Fetch the ingestion process status from the database
            var process = await _db.IngestionProcesses
                .Where(p => p.Id == processId)
                .Select(p => new IngestionStatusDto(p.Id, p.DocumentId, p.ExternalProcessId, p.Status, p.StatusDetails, p.LastUpdatedAt))
                .FirstOrDefaultAsync();

            if (process == null)
            {
                return null; // Process not found
            }

            return process;
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching ingestion status: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<IngestionProcessDetailsDto>> GetAllIngestionProcessesAsync(Guid? documentId = null)
    {
        try
        {
            // Fetch all ingestion processes from the database
            var query = _db.IngestionProcesses.AsQueryable();

            if (documentId.HasValue)
            {
                query = query.Where(p => p.DocumentId == documentId.Value);
            }

            var ingestionProcesses = await query
                .Select(p => new IngestionProcessDetailsDto(p.Id, p.DocumentId, p.ExternalProcessId, p.Status, p.StatusDetails, p.CreatedAt, p.LastUpdatedAt, p.TriggeredByUserId))
                .ToListAsync();

            return ingestionProcesses;
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Error fetching ingestion processes: {ex.Message}");
            return null;
        }
    }



    // This method would be called  to update status
    public async Task<IngestionStatusUpdateResponseDto> UpdateIngestionStatus(Guid userId, IngestionStatusUpdateDto updateDto)
    {
        try
        {
            var process = await _db.IngestionProcesses.FirstOrDefaultAsync(p => p.Id == updateDto.Id);

            if (process == null || process.Status == IngestionStatus.Completed || process.Status == IngestionStatus.Failed || process.Status == IngestionStatus.Cancelled)
            {
                return new IngestionStatusUpdateResponseDto(null, null, "'process not found or already completed/failed/cancelled");
            }

            process.Status = updateDto.Status;
            process.StatusDetails = $"{updateDto.StatusDetails}- updated by user ${userId}";
            process.LastUpdatedAt = DateTime.UtcNow;
            _db.IngestionProcesses.Update(process);
            await _db.SaveChangesAsync();
            return new IngestionStatusUpdateResponseDto(process.Id, process.Status, process.StatusDetails);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling Spring Boot webhook: {ex.Message}");
            return new IngestionStatusUpdateResponseDto(null, null, ex.Message);
        }

    }
    public async Task<object> TriggerSpringServiceAsync(Guid processId)
    {
        try
        {
            var url = "http://35.172.181.120:5292/api/Auth/login";
            var payload = new
            {
                username = "naninaga26@gmail.com",
                password = "Naga@110197"
            };
            var response = await _httpHelperService.SendRequestAsync<object>(HttpMethod.Post, url, payload);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling Spring Boot webhook: {ex.Message}");
            return false;
        }
    }
}