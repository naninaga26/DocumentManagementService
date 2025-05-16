using Document.Services.IngestionManagementAPI.Data;
using Document.Services.IngestionManagementAPI.Models;
using Document.Services.IngestionManagementAPI.Models.DTOs;
using Document.Services.IngestionManagementAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
namespace Document.Services.IngestionManagementAPI.Services;
public class IngestionService : IIngestionService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient; // Injected for calling Spring Boot
    private readonly IConfiguration _configuration;


    public IngestionService(AppDbContext db, HttpClient httpClient, IConfiguration configuration)
    {
        _db= db;
        _httpClient = httpClient; // Base address configured in Program.cs
        _configuration = configuration;
    }

    private async Task<List<IngestionProcess>> GetDocumentMetadataAsync(Guid documentId)
    {
        return await _db.IngestionProcesses
            .FromSqlRaw("SELECT * FROM \"IngestionProcesses\" WHERE \"Status\" = {0}", documentId)
            .ToListAsync();
    }



    public async Task<(IngestionProcess process, string message)> TriggerIngestionAsync(Guid documentId, Guid userId)
    {
        //use rawsql query to check if the document exists
        // var document = await GetDocumentMetadataAsync(documentId);
        // if (document == null)
        // {
        //     return (null, "Document not found.");
        // }

        // 1. Create an IngestionProcess record in our DB
        var ingestionProcess = new IngestionProcess
        {
            DocumentId = documentId,
            Status = IngestionStatus.Pending,
            TriggeredByUserId = userId,
            ExternalProcessId = $"TEMP-{Guid.NewGuid()}" // Temporary, to be updated by Spring Boot
        };
        _db.IngestionProcesses.Add(ingestionProcess);
        await _db.SaveChangesAsync();


        // 2. Call the Spring Boot backend
        var springBootIngestionEndpoint = _configuration["SpringBootService:IngestionEndpoint"];
        // You might need to send more data, e.g., document URL or content if not accessible by Spring Boot directly
        var requestPayload = new
        { // Define a DTO for this if complex
            documentId = documentId.ToString(),
            // callbackUrl = $"https://your-dotnet-service.com/api/ingestion/webhook/{ingestionProcess.Id}" // If Spring Boot needs to call back
            internalProcessId = ingestionProcess.Id.ToString() // So Spring Boot can correlate
        };

        try
        {
            // Consider what Spring Boot returns. Here we assume it returns some kind of external ID.
            var response = await _httpClient.PostAsJsonAsync(springBootIngestionEndpoint, requestPayload);

            if (response.IsSuccessStatusCode)
            {
                // Example: Spring Boot returns an ID for its process
                var springBootResponse = await response.Content.ReadFromJsonAsync<SpringBootIngestionResponseDto>(); // Define this DTO

                ingestionProcess.ExternalProcessId = springBootResponse?.ExternalProcessId ?? $"FAILED_TO_GET_EXT_ID_{ingestionProcess.Id}";
                ingestionProcess.Status = IngestionStatus.InProgress; // Or whatever initial status Spring Boot confirms
                ingestionProcess.LastUpdatedAt = DateTime.UtcNow;
                _db.IngestionProcesses.Update(ingestionProcess);
                await _db.SaveChangesAsync();

                return (ingestionProcess, "Ingestion triggered successfully with Spring Boot.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ingestionProcess.Status = IngestionStatus.Failed;
                ingestionProcess.StatusDetails = $"Failed to trigger Spring Boot: {response.StatusCode} - {errorContent}";
                ingestionProcess.LastUpdatedAt = DateTime.UtcNow;
                _db.IngestionProcesses.Update(ingestionProcess);
                await _db.SaveChangesAsync();
                return (null, $"Failed to trigger Spring Boot ingestion: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            // Log exception
            ingestionProcess.Status = IngestionStatus.Failed;
            ingestionProcess.StatusDetails = $"Error communicating with Spring Boot: {ex.Message}";
            ingestionProcess.LastUpdatedAt = DateTime.UtcNow;
            _db.IngestionProcesses.Update(ingestionProcess);
            await _db.SaveChangesAsync();
            return (null, $"Error communicating with Spring Boot: {ex.Message}");
        }
    }
    // Define this DTO based on what Spring Boot returns
    private record SpringBootIngestionResponseDto(string ExternalProcessId);


    public async Task<IngestionStatusDto> GetIngestionStatusAsync(Guid processId)
    {
        var process = await _db.IngestionProcesses
            .Where(p => p.Id == processId)
            .Select(p => new IngestionStatusDto(p.Id, p.DocumentId, p.ExternalProcessId, p.Status, p.StatusDetails, p.LastUpdatedAt))
            .FirstOrDefaultAsync();

        // Optionally, you could also query the Spring Boot backend for the latest status if your system is the primary status holder
        // and Spring Boot provides a status check API.
        // if (process != null && process.Status == IngestionStatus.InProgress && !string.IsNullOrEmpty(process.ExternalProcessId)) {
        //    // Call Spring Boot status endpoint using process.ExternalProcessId
        // }
        return process;
    }

    public async Task<IEnumerable<IngestionProcessDetailsDto>> GetAllIngestionProcessesAsync(Guid? documentId = null)
    {
       //implement  code to fetch all ingestion processes
        return null;
    }


    public async Task<bool> CancelIngestionAsync(Guid processId, Guid userId)
    {
        var process = await _db.IngestionProcesses.FindAsync(processId);
        if (process == null || process.Status == IngestionStatus.Completed || process.Status == IngestionStatus.Failed || process.Status == IngestionStatus.Cancelled)
        {
            return false; // Cannot cancel or already finalized
        }

        // TODO: Call Spring Boot backend to attempt cancellation if it supports it
        // var springBootCancelEndpoint = $"/api/ingest/{process.ExternalProcessId}/cancel";
        // var response = await _httpClient.PostAsync(springBootCancelEndpoint, null);
        // if (response.IsSuccessStatusCode) {
        //    process.Status = IngestionStatus.Cancelled;
        //    process.StatusDetails = $"Cancelled by user {userId} via .NET service.";
        // } else {
        //    process.StatusDetails = $"Attempted cancellation; Spring Boot responded with {response.StatusCode}";
        //    // Decide if you still mark as Cancelled in your DB or handle failure
        //    return false; // Or throw exception
        // }

        process.Status = IngestionStatus.Cancelled; // Assuming cancellation is successful or forced on .NET side
        process.StatusDetails = $"Cancellation requested by user {userId}.";
        process.LastUpdatedAt = DateTime.UtcNow;
        _db.IngestionProcesses.Update(process);
        await _db.SaveChangesAsync();
        return true;
    }

    // This method would be called by a webhook from Spring Boot
    public async Task UpdateIngestionStatusFromWebhookAsync(string externalProcessId, IngestionStatus status, string details)
    {
        var process = await _db.IngestionProcesses.FirstOrDefaultAsync(p => p.ExternalProcessId == externalProcessId);
        if (process != null)
        {
            process.Status = status;
            process.StatusDetails = details;
            process.LastUpdatedAt = DateTime.UtcNow;
            _db.IngestionProcesses.Update(process);
            await _db.SaveChangesAsync();
        }
        else
        {
            // Log: Received webhook for unknown externalProcessId
            Console.WriteLine($"Webhook received for unknown externalProcessId: {externalProcessId}");
        }
    }
}