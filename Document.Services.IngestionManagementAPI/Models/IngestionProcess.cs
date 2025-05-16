using System.ComponentModel.DataAnnotations;

namespace Document.Services.IngestionManagementAPI.Models;

public class IngestionProcess
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; } 
    [Required]
    public  string ExternalProcessId { get; set; } 
    public IngestionStatus Status { get; set; } = IngestionStatus.Pending;
    public  string StatusDetails { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid TriggeredByUserId { get; set; }
}