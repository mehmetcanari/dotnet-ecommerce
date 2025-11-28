using ECommerce.Application.Abstract;
using ECommerce.Shared.DTO.Request.FileUpload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize("Admin")]
public class FileUploadController(IFileUploadService uploadService) : ApiBaseController
{

    [Authorize("Admin")]
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadRequestDto request) => HandleResult(await uploadService.UploadFileAsync(request));
}
