using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Document.Services.DocumentManagementAPI.Models.DTOs;

public record DocumentMetadataDto(Guid Id, string FileName, string ContentType, long FileSize, DateTime UploadedAt, Guid UploadedByUserId);
public record DocumentCreateDto([Required] IFormFile File);
public record S3ResponseDto( bool Success ,string BucketName, string ObjectKey);
public record FileFounDto(bool found, string message);

// Add DocumentUpdateDto if needed, e.g., for renaming