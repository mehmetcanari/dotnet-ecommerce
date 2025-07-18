using Amazon.S3;
using Amazon.S3.Transfer;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.FileUpload;
using ECommerce.Application.Utility;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Services.AWS;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILoggingService _loggingService;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILoggingService loggingService)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS:BucketName"]!;
        _loggingService = loggingService;
    }

    public async Task<Result<string>> UploadFileAsync(FileUploadRequestDto request, string keyPrefix)
    {
        try
        {
            var key = $"{keyPrefix}/{Guid.NewGuid()}_{request.File.FileName}";

            await using var stream = request.File.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _bucketName,
                ContentType = request.File.ContentType,
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            _loggingService.LogInformation($"File uploaded successfully to S3: {key}");

            var fileUrl = $"https://{_bucketName}.s3.{_configuration["AWS:Region"]}.amazonaws.com/{key}";
            return Result<string>.Success(fileUrl);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Error uploading file to S3: {Message}", ex.Message);
            return Result<string>.Failure(ex.Message);
        }
    }
}