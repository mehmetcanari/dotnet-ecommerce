using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces.Service;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/user/products")]
public class UserProductController : ControllerBase
{
    private readonly IProductService _productService;

    public UserProductController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "User")]
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
            return Ok(new { message = "All products fetched successfully", data = products });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [Authorize(Roles = "User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        try
        {
            var product = await _productService.GetProductWithIdAsync(id);
            return Ok(new { message = $"Product with id {id} fetched successfully", data = product });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}