using Document.Services.IngestionManagementAPI.Models;
using Document.Services.IngestionManagementAPI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Document.Services.IngestionManagementAPI.Services.IServices;


public interface IIngestionService
{
    Task<(IngestionProcess process, string message)> TriggerIngestionAsync(Guid documentId, Guid userId);
    Task<IngestionStatusDto> GetIngestionStatusAsync(Guid processId);
    Task<IEnumerable<IngestionProcessDetailsDto>> GetAllIngestionProcessesAsync(Guid? documentId = null);
    Task<IngestionStatusUpdateResponseDto> UpdateIngestionStatus(Guid userId, IngestionStatusUpdateDto updateDto);
    Task<object> TriggerSpringServiceAsync(Guid processId);
        
    }

