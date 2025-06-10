using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.DTO.Request.FileUpload
{
    public class FileUploadRequestDto
    {
        public IFormFile File { get; set; } = null!;
    }
} 