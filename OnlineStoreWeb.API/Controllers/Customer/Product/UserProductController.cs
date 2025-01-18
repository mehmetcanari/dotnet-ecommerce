using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Customer.Product;

[ApiController]
[Route("api/user/products")]
public class UserProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<UserProductController> _logger;

    public UserProductController(IProductRepository productRepository, ILogger<UserProductController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(new { message = "Products fetched successfully", data = products });
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
            var product = await _productRepository.GetProductWithIdAsync(id);
            return Ok(new { message = "Product fetched successfully", data = product });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching product: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the product");
        }
    }
}