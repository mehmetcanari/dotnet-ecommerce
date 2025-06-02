using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Abstract.Service;

public interface IS3Service
{
    Task<string> UploadFileAsync(IFormFile file, string keyPrefix);
}