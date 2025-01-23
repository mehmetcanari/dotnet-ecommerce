using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Product;
using OnlineStoreWeb.API.Services.Product;

namespace OnlineStoreWeb.API.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
public class AdminProductController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while fetching products");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await productService.GetProductWithIdAsync(id);
            return Ok(product);
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while fetching the product");
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct(CreateProductDto productCreateRequest)
    {
        try
        {
            await productService.AddProductAsync(productCreateRequest);
            return Created($"products/{productCreateRequest.Name}", new { message = "Product created successfully" });
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while creating the product");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productUpdateRequest)
    {
        try
        {
            await productService.UpdateProductAsync(id, productUpdateRequest);
            return Ok(new { message = "Product updated successfully" });
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while updating the product");
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteProductAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }
        catch
        {
            return StatusCode(500, "An unexpected error occurred while deleting the product");
        }
    }
}