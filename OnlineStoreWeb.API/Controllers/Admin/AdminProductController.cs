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
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
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
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct(ProductCreateDto productCreateRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await productService.AddProductAsync(productCreateRequest);
            return Created($"products/{productCreateRequest.Name}", new { message = "Product created successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto productUpdateRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await productService.UpdateProductAsync(id, productUpdateRequest);
            return Ok(new { message = "Product updated successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
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
        catch (Exception exception)
        {
            return BadRequest(exception.Message); 
        }
    }
}