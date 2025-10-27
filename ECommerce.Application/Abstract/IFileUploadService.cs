using ECommerce.Application.DTO.Request.FileUpload;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract;

public interface IFileUploadService
{
    Task<Result<string>> UploadFileAsync(FileUploadRequestDto request);
}