using Amazon.S3;
using Amazon.S3.Transfer;
using ECommerce.Application.Abstract;
using ECommerce.Shared.Constants;
using ECommerce.Shared.DTO.Request.FileUpload;
using ECommerce.Shared.Wrappers;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Application.Services.Upload;

public class FileUploadService : IFileUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;
    private readonly string _bucketName;

    public FileUploadService(IAmazonS3 s3Client, IConfiguration configuration, ILogService logService)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = _configuration["AWS:BucketName"]!;
        _logService = logService;
    }

    public async Task<Result<string>> UploadFileAsync(FileUploadRequestDto request)
    {
        try
        {
            var key = $"{request.KeyPrefix}/{Guid.NewGuid()}_{request.File.FileName}";
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

            var fileUrl = $"https://{_bucketName}.s3.{_configuration["AWS:Region"]}.amazonaws.com/{key}";
            return Result<string>.Success(fileUrl);
        }
        catch (Exception ex)
        {
            _logService.LogError(ex, ErrorMessages.FileUploadFailed, ex.Message);
            return Result<string>.Failure(ex.Message);
        }
    }
}