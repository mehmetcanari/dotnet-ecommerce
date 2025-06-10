using ECommerce.Application.DTO.Request.FileUpload;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Abstract.Service;

public interface IS3Service
{
    Task<Result<string>> UploadFileAsync(FileUploadRequestDto request, string keyPrefix);
}