using Document.Services.DocumentManagementAPI.Models.DTOs;
namespace  Document.Services.DocumentManagementAPI.Services.IServices;

public interface IS3Service
{
    Task<S3ResponseDto> UploadFileAsync(IFormFile file, string bucketName);
}
