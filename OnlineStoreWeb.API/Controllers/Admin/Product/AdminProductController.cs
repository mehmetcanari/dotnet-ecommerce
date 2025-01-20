using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Admin.Product;

[ApiController]
[Route("api/admin/products")]
public class AdminProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<AdminProductController> _logger;

    public AdminProductController(IProductService productService, ILogger<AdminProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching products: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching products");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        try
        {
            var product = await _productService.GetProductWithIdAsync(id);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the product");
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateProduct(CreateProductDto productCreateRequest)
    {
        try
        {
            if (productCreateRequest == null)
                return BadRequest(new { message = "Product data is required" });

            await _productService.AddProductAsync(productCreateRequest);
            return Created($"products/{productCreateRequest.Name}", new { message = "Product created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating product: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while creating the product");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productUpdateRequest)
    {
        try
        {
            await _productService.UpdateProductAsync(productUpdateRequest);
            return Ok(new { message = "Product updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating product: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while updating the product");
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting product: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the product");
        }
    }
}