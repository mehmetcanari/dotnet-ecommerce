using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Application.Validations.Attribute;
using MediatR;
using ECommerce.Application.Queries.Product;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/v1/user/products")]
[Authorize(Roles = "User")]
[ApiVersion("1.0")]
public class UserProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "All products fetched successfully", data = result.Data });
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetProductById([FromRoute] int id)
    {
        var product = await _mediator.Send(new GetProductWithIdQuery { ProductId = id });
        if (product.IsFailure)
        {
            return NotFound(new { message = product.Error });
        }
        return Ok(new { message = $"Product with id {id} fetched successfully", data = product.Data });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new ProductSearchQuery(query, page, pageSize));
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Product search completed successfully", data = result.Data });
    }
}