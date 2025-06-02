using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Abstract.Service;

public interface IS3Service
{
    Task<Result<string>> UploadFileAsync(IFormFile file, string keyPrefix);
}