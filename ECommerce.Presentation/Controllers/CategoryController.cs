using ECommerce.Application.Commands.Category;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Queries.Category;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class CategoryController(IMediator mediator) : ApiBaseController
{
    [Authorize("Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request) => HandleResult(await mediator.Send(new CreateCategoryCommand(request)));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id) => HandleResult(await mediator.Send(new DeleteCategoryCommand(id)));

    [Authorize("Admin")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequestDto request) => HandleResult(await mediator.Send(new UpdateCategoryCommand(request)));

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById([FromRoute] Guid id) => HandleResult(await mediator.Send(new GetCategoryByIdQuery(id)));
}


