using Document.Services.IngestionManagementAPI.Models;

namespace Document.Services.IngestionManagementAPI.Models.DTOs;

public record IngestionTriggerRequestDto(Guid DocumentId);
public record IngestionStatusDto(Guid Id, Guid DocumentId, string ExternalProcessId, IngestionStatus Status, string StatusDetails, DateTime LastUpdatedAt);
public record IngestionProcessDetailsDto(
    Guid Id,
    Guid DocumentId,
    string ExternalProcessId,
    IngestionStatus Status,
    string StatusDetails,
    DateTime CreatedAt,
    DateTime LastUpdatedAt,
    Guid TriggeredByUserId
);

// DTO for the webhook payload from Spring Boot
public record WebhookStatusUpdateDto(
    string ExternalProcessId,
    IngestionStatus Status,
    string Details
);

public record IngestionStatusUpdateResponseDto(
    Guid? ProcessId ,
    IngestionStatus? Status,
    string Details
);

public record IngestionStatusUpdateDto(
    Guid Id,
    string ExternalProcessId,
    IngestionStatus Status,
    string StatusDetails
);

// DTO based on what Spring Boot returns
public record SpringBootIngestionResponseDto(string ExternalProcessId);
