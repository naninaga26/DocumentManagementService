using Document.Services.DocumentManagementAPI.Models.DTOs;
using Document.Services.DocumentManagementAPI.Services.IServices;
using Amazon.S3;
using Amazon.S3.Model;
public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;
    private readonly string _bucketName;
    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _logger = logger;
        _bucketName = configuration["AWS:BucketName"];

        // Verify configuration
        if (string.IsNullOrEmpty(_bucketName))
        {
            throw new ArgumentNullException("AWS:BucketName is not configured");
        }
    }

    public async Task<S3ResponseDto> UploadFileAsync(IFormFile file, string bucketName)
    {
        try
        {
            // Verify bucket exists and is accessible
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
            if (!bucketExists)
            {
                throw new Exception($"Bucket {_bucketName} does not exist or you don't have permission to access it");
            }
            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var objectKey = $"{Guid.NewGuid()}_{file.FileName}";

            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                InputStream = memoryStream,
                ContentType = file.ContentType,
                Metadata =
                {
                    ["x-amz-meta-originalname"] = file.FileName,
                    ["x-amz-meta-uploaddate"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(putRequest);
            Console.Write("response"+response);
            var Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            return new S3ResponseDto(Success, bucketName, objectKey);
           
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3");
            throw;
        }
    }
}
