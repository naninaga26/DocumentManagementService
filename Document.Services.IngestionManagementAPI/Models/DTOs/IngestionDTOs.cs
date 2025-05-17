using Document.Services.IngestionManagementAPI.Models;

namespace Document.Services.IngestionManagementAPI.Models.DTOs;

public record IngestionTriggerRequestDto(Guid DocumentId);
public record IngestionStatusDto(Guid Id, Guid DocumentId, string ExternalProcessId, IngestionStatus Status, string StatusDetails, DateTime LastUpdatedAt);
public record IngestionProcessDetailsDto(
    Guid Id,
    Guid DocumentId,
    string FileName,
    string ExternalProcessId,
    IngestionStatus Status,
    string StatusDetails,
    DateTime CreatedAt,
    DateTime LastUpdatedAt,
    Guid TriggeredByUserId,
    string TriggeredByUsername
);

// DTO for the webhook payload from Spring Boot
public record WebhookStatusUpdateDto(
    string ExternalProcessId,
    IngestionStatus Status,
    string Details
);

  // DTO based on what Spring Boot returns
  public record SpringBootIngestionResponseDto(string ExternalProcessId);
