using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Customer.Product;

[ApiController]
[Route("api/user/products")]
public class UserProductController : ControllerBase
{
    private readonly IProductService _productService;

    public UserProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new { message = "Products fetched successfully", data = products });
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
            var product = await _productService.GetProductWithIdAsync(id);
            return Ok(new { message = "Product fetched successfully", data = product });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the product");
        }
    }
}