using System.ComponentModel.DataAnnotations;

namespace Document.Services.DocumentManagementAPI.Models;

public class FileMetaData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    [MaxLength(255)]
    public  string FileName { get; set; }
    [MaxLength(50)]
    public  string ContentType { get; set; } 
    public long FileSize { get; set; } 
    public  string S3Url { get; set; } 
    [Required]
    public Guid UploadedByUserId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}