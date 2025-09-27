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
public class ProductController(IMediator _mediator, IProductService _productService, IS3Service _s3Service) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllProducts()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "All products fetched successfully", data = result.Data });
    }

    [HttpGet("{id}")]
    [ValidateId]
    [Authorize]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        var product = await _mediator.Send(new GetProductWithIdQuery { ProductId = id });
        if (product.IsFailure)
        {
            return NotFound(new { message = product.Error });
        }
        return Ok(new { message = $"Product with id {id} fetched successfully", data = product.Data });
    }

    [HttpPost("create")]
    [Authorize("Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto productCreateRequestRequest)
    {
        var result = await _productService.CreateProductAsync(productCreateRequestRequest);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Product created successfully" });
    }

    [Authorize("Admin")]
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

    [Authorize("Admin")]
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

    [Authorize("Admin")]
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
    [Authorize]
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