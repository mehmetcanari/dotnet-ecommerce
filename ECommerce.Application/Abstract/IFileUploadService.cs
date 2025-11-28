using ECommerce.Application.DTO.Request.FileUpload;
using ECommerce.Shared.Wrappers;

namespace ECommerce.Application.Abstract;

public interface IFileUploadService
{
    Task<Result<string>> UploadFileAsync(FileUploadRequestDto request);
}