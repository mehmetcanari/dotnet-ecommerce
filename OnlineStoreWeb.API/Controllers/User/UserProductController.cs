using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.Repositories.Product;
using OnlineStoreWeb.API.Services.Product;

namespace OnlineStoreWeb.API.Controllers.User;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(new { message = "Products fetched successfully", data = products });
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
            var product = await _productService.GetProductWithIdAsync(id);
            return Ok(new { message = "Product fetched successfully", data = product });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
            throw;
        }
    }
}