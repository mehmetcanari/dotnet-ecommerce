using ECommerce.Application.Commands.Product;
using ECommerce.Application.Queries.Product;
using ECommerce.Application.Utility;
using ECommerce.Shared.DTO.Request.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class ProductController(IMediator mediator) : ApiBaseController
{
    [HttpPost("create")]
    [Authorize("Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequestDto request) => HandleResult(await mediator.Send(new CreateProductCommand(request)));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid id) => HandleResult(await mediator.Send(new DeleteProductCommand(id)));

    [Authorize("Admin")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateRequestDto request) => HandleResult(await mediator.Send(new UpdateProductCommand(request)));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> GetAllProducts(QueryPagination pagination) => HandleResult(await mediator.Send(new GetAllProductsQuery(pagination)));

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetProductById([FromRoute] Guid id) => HandleResult(await mediator.Send(new GetProductByIdQuery(id)));

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) => HandleResult(await mediator.Send(new GetProductBySearchQuery(query, page, pageSize)));
}