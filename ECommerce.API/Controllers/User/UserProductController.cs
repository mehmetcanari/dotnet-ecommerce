using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Validations.Attribute;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/v1/user/products")]
[Authorize(Roles = "User")]
[ApiVersion("1.0")]
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
        var products = await _productService.GetAllProductsAsync();
        return Ok(new { message = "All products fetched successfully", data = products });
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        var product = await _productService.GetProductWithIdAsync(id);
        return Ok(new { message = $"Product with id {id} fetched successfully", data = product });
    }
}