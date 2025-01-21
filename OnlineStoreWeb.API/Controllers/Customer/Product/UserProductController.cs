using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Product;
using OnlineStoreWeb.API.Services.Product;

namespace OnlineStoreWeb.API.Controllers.Customer.Product;

[ApiController]
[Route("api/user/products")]
public class UserProductController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await productService.GetAllProductsAsync();
            return Ok(new { message = "Products fetched successfully", data = products });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching products");
        }
    }

    [HttpGet("{Id}")]
    public async Task<IActionResult> GetProductById(ViewProductDto viewProductDto)
    {
        try
        {
            var product = await productService.GetProductWithIdAsync(viewProductDto);
            return Ok(new { message = "Product fetched successfully", data = product });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while fetching the product");
        }
    }
}