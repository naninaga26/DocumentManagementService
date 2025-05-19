using Document.Services.DocumentManagementAPI.Models.DTOs;
namespace  Document.Services.DocumentManagementAPI.Services.IServices;

public interface IS3Service
{
    Task<S3ResponseDto> UploadFileAsync(IFormFile file, string bucketName);
    Task<(byte[] FileData, string ContentType)> DownloadFileAsync(string bucketName, string objectKey);
    Task<FileFounDto> FileExistsAsync(string bucketName, string objectKey);
    Task<bool> DeleteDocumentAsync(string bucketName, string objectKey);
}
