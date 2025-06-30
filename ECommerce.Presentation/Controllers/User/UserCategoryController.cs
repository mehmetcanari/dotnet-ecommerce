using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using ECommerce.Application.Validations.Attribute;
using MediatR;
using ECommerce.Application.Queries.Category;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/v1/user/categories")]
[Authorize(Roles = "User")]
[ApiVersion("1.0")]
public class UserCategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserCategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery { CategoryId = id });
        if (category.IsFailure)
        {
            return NotFound(new { message = category.Error });
        }
        return Ok(new { message = "Category retrieved successfully", data = category.Data });
    }
}


