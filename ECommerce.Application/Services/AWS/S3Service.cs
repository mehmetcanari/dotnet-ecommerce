using Amazon.S3;
using Amazon.S3.Transfer;
using ECommerce.Application.Abstract.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Services.AWS;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS:BucketName"]!;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string keyPrefix)
    {
        var key = $"{keyPrefix}/{Guid.NewGuid()}_{file.FileName}";

        await using var stream = file.OpenReadStream();

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            Key = key,
            BucketName = _bucketName,
            ContentType = file.ContentType,
        };

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(uploadRequest);

        var fileUrl = $"https://{_bucketName}.s3.{_configuration["AWS:Region"]}.amazonaws.com/{key}";
        return fileUrl;
    }
}