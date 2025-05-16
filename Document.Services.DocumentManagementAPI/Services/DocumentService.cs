using Document.Services.DocumentManagementAPI.Data;
using Document.Services.DocumentManagementAPI.Models;
using Document.Services.DocumentManagementAPI.Models.DTOs;
using Document.Services.DocumentManagementAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
namespace Document.Services.DocumentManagementAPI.Services;
public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly string _fileStorageBasePath;
    private readonly IS3Service _s3Service;
    private readonly string _bucketName;

    public DocumentService(AppDbContext db, IConfiguration configuration, IS3Service s3Service)
    {
        _db = db;
        _s3Service = s3Service;
        _bucketName = configuration["AWS:BucketName"];
        _fileStorageBasePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(_fileStorageBasePath))
        {
            Directory.CreateDirectory(_fileStorageBasePath);
        }
    }

    public async Task<(FileMetaData document, string message)> UploadDocumentAsync(IFormFile file, Guid userId)
    {
        if (file == null || file.Length == 0)
        {
            return (null, "No file uploaded or file is empty.");
        }

        // Sanitize filename (basic example)
        var fileName = Path.GetFileName(file.FileName);
        //var uniqueFileName = $"{Guid.NewGuid()}_{fileName}"; // Ensure unique file name
        //var filePath = Path.Combine(_fileStorageBasePath, uniqueFileName);

        try
        {
            // await using (var stream = new FileStream(filePath, FileMode.Create))
            // {
            //     await file.CopyToAsync(stream);
            // }

            // Upload to S3
            var res = await _s3Service.UploadFileAsync(file,_bucketName ); // Upload to S3

            var document = new FileMetaData
            {
                FileName = fileName, // Store original filename
                ContentType = file.ContentType,
                FileSize = file.Length,
                S3Url = res.ObjectKey, // Store the path to the saved file
                UploadedByUserId = userId,
                UploadedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.FilesMetadata.Add(document);
            await _db.SaveChangesAsync();
            return (document, "File uploaded successfully.");
        }
        catch (Exception ex)
        {
            // Log exception
            return (null, $"Error uploading file: {ex.Message}");
        }
    }

    public async Task<DocumentMetadataDto> GetDocumentMetadataAsync(Guid documentId)
    {
        var document = await _db.FilesMetadata
            .Where(d => d.Id == documentId)
            .Select(d => new DocumentMetadataDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.UploadedAt, d.UploadedByUserId))
            .FirstOrDefaultAsync();
        return document;
    }

    public async Task<(Stream fileStream, string contentType, string fileName)?> GetDocumentFileAsync(Guid documentId)
    {
        var document = await _db.FilesMetadata.FindAsync(documentId);
        if (document == null || !File.Exists(document.S3Url))
        {
            return null;
        }
        var stream = new FileStream(document.S3Url, FileMode.Open, FileAccess.Read);
        return (stream, document.ContentType, document.FileName);
    }


    public async Task<IEnumerable<DocumentMetadataDto>> GetAllDocumentsAsync(Guid? userId = null)
    {
        var query = _db.FilesMetadata.AsQueryable();
        if (userId.HasValue)
        {
            query = query.Where(d => d.UploadedByUserId == userId.Value);
        }
        return await query
            .Select(d => new DocumentMetadataDto(d.Id, d.FileName, d.ContentType, d.FileSize, d.UploadedAt, d.UploadedByUserId))
            .ToListAsync();
    }

    public async Task<bool> UpdateDocumentAsync(Guid documentId, /* DocumentUpdateDto updateDto, */ Guid userId)
    {
        var document = await _db.FilesMetadata.FindAsync(documentId);
        if (document == null || document.UploadedByUserId != userId) // Basic ownership check
        {
            return false;
        }
        // Implement update logic, e.g., renaming, replacing file (more complex)
        // document.FileName = updateDto.NewFileName;
        document.UpdatedAt = DateTime.UtcNow;
        _db.FilesMetadata.Update(document);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid userId, bool isAdmin)
    {
        var document = await _db.FilesMetadata.FindAsync(documentId);
        if (document == null)
        {
            return false; // Not found
        }

        // Authorization: Only uploader or admin can delete
        if (document.UploadedByUserId != userId && !isAdmin)
        {
            return false; // Forbidden
        }

        // Delete physical file
        if (File.Exists(document.S3Url))
        {
            try
            {
                File.Delete(document.S3Url);
            }
            catch (Exception ex)
            {
                // Log error, but proceed to delete DB record or handle as critical failure
                Console.WriteLine($"Error deleting file {document.S3Url}: {ex.Message}");
            }
        }

        _db.FilesMetadata.Remove(document);
        await _db.SaveChangesAsync();
        return true;
    }
}