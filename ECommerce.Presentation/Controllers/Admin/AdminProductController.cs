using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Validations.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTO.Request.FileUpload;
using ECommerce.Application.Queries.Product;
using MediatR;

namespace ECommerce.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/products")]
[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
public class AdminProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IS3Service _s3Service;
    private readonly IMediator _mediator;
    private readonly IProductSearchService _productSearchService;
    
    public AdminProductController(
        IProductService productService, 
        IS3Service s3Service,
        IMediator mediator,
        IProductSearchService productSearchService)
    {
        _productService = productService;
        _s3Service = s3Service;
        _mediator = mediator;
        _productSearchService = productSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }
        return Ok(new { message = "All products fetched successfully", data = result.Data });
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        var result = await _mediator.Send(new GetProductWithIdQuery { ProductId = id });
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }
        return Ok(new { message = $"Product with id {id} fetched successfully", data = result.Data });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto productCreateRequestRequest)
    {
        var result = await _productService.CreateProductAsync(productCreateRequestRequest);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Product created successfully" });
    }
    
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImage([FromForm] FileUploadRequestDto request)
    {
        var result = await _s3Service.UploadFileAsync(request, "products");
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Image uploaded successfully" });
    }
    
    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductUpdateRequestDto productUpdateRequestRequest)
    {
        var result = await _productService.UpdateProductAsync(id, productUpdateRequestRequest);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = $"Product with id {id} updated successfully" });
    }

    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteProduct([FromRoute] int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = $"Product with id {id} deleted successfully" });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new ProductSearchQuery(query, page, pageSize));
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Product search completed successfully", data = result.Data });
    }
}