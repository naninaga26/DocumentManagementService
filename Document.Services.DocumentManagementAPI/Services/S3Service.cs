using Document.Services.DocumentManagementAPI.Models.DTOs;
using Document.Services.DocumentManagementAPI.Services.IServices;
using Amazon.S3;
using Amazon.S3.Model;
namespace Document.Services.DocumentManagementAPI.Services;
public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;
    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<S3ResponseDto> UploadFileAsync(IFormFile file, string bucketName)
    {
        try
        {
            // Verify bucket exists and is accessible
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists)
            {
                throw new Exception($"Bucket {bucketName} does not exist or you don't have permission to access it");
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
            Console.Write("response" + response);
            var Success = response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            return new S3ResponseDto(Success, bucketName, objectKey);

        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3");
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string bucketName, string objectKey)
    {
        try
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            var response = await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            Console.WriteLine($"Deleted object '{objectKey}' from bucket '{bucketName}'.");
            return true;
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when deleting object");
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unknown error occurred. Message:'{e.Message}' when deleting object");
            return false;
        }
    }

    public async Task<FileFounDto> FileExistsAsync(string bucketName, string objectKey)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = objectKey
            };

            var response = await _s3Client.GetObjectMetadataAsync(request);
            return new FileFounDto(response.HttpStatusCode == System.Net.HttpStatusCode.OK, "File Found");
        }
        catch (AmazonS3Exception ex)
        {
            // Return false only if the object was not found
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new FileFounDto(false, "File not Found");
            }
            throw;
        }
    }


    public async Task<(byte[] FileData, string ContentType)> DownloadFileAsync(string bucketName, string objectKey)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = objectKey
        };

        try
        {
            var response = await _s3Client.GetObjectAsync(request);
            var responseStream = response.ResponseStream;
            var memoryStream = new MemoryStream();

            await responseStream.CopyToAsync(memoryStream);
            return (memoryStream.ToArray(), response.Headers["Content-Type"]);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException("File not found in S3.", ex);
        }
    }

}
