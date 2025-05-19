using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Validations.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/products")]
[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
public class AdminProductController : ControllerBase
{
    private readonly IProductService _productService;
    
    public AdminProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(new { message = "All products fetched successfully", products });
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        var product = await _productService.GetProductWithIdAsync(id);
        return Ok(new { message = $"Product with id {id} fetched successfully", product });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto productCreateRequestRequest)
    {
        var result = await _productService.CreateProductAsync(productCreateRequestRequest);
        return Ok(new { message = "Product created successfully", data = result });
    }

    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductUpdateRequestDto productUpdateRequestRequest)
    {
        var result = await _productService.UpdateProductAsync(id, productUpdateRequestRequest);
        return Ok(new { message = $"Product with id {id} updated successfully", data = result });
    }

    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteProduct([FromRoute] int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return Ok(new { message = $"Product with id {id} deleted successfully", data = result });
    }
}