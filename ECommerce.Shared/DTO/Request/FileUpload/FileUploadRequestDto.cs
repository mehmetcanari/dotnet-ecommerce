using Microsoft.AspNetCore.Http;

namespace ECommerce.Shared.DTO.Request.FileUpload
{
    public class FileUploadRequestDto
    {
        public required string KeyPrefix { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}