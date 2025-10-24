using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.FileUpload;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Queries.Product;
using ECommerce.Application.Validations.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class ProductController(IMediator _mediator, IProductService _productService, IS3Service _s3Service) : ApiBaseController
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllProducts() => HandleResult(await _mediator.Send(new GetAllProductsQuery()));

    [HttpGet("{id}")]
    [ValidateId]
    [Authorize]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id) => HandleResult(await _mediator.Send(new GetProductByIdQuery { Id = id }));

    [HttpPost("create")]
    [Authorize("Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto productCreateRequestRequest) => HandleResult(await _productService.CreateProductAsync(productCreateRequestRequest));

    [Authorize("Admin")]
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadRequestDto request) => HandleResult(await _s3Service.UploadFileAsync(request, "products"));

    [Authorize("Admin")]
    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromBody] ProductUpdateRequestDto productUpdateRequestRequest) => HandleResult(await _productService.UpdateProductAsync(id, productUpdateRequestRequest));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id) => HandleResult(await _productService.DeleteProductAsync(id));

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) => HandleResult(await _mediator.Send(new ProductSearchQuery(query, page, pageSize)));
}