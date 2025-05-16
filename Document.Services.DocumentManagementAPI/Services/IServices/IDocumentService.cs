
using Document.Services.DocumentManagementAPI.Models.DTOs;
using Document.Services.DocumentManagementAPI.Models; // Required for the Document model in the return type
namespace Document.Services.DocumentManagementAPI.Services.IServices;
public interface IDocumentService
{
    Task<(FileMetaData document, string message)> UploadDocumentAsync(IFormFile file, Guid userId);
    Task<DocumentMetadataDto> GetDocumentMetadataAsync(Guid documentId);
    Task<(Stream fileStream, string contentType, string fileName)?> GetDocumentFileAsync(Guid documentId);
    Task<IEnumerable<DocumentMetadataDto>> GetAllDocumentsAsync(Guid? userId = null);
    Task<bool> UpdateDocumentAsync(Guid documentId, /* DocumentUpdateDto updateDto, */ Guid userId);
    Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId, bool isAdmin);
}
