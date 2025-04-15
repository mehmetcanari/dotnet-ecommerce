using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/user/categories")]
[ApiVersion("1.0")]
public class UserCategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public UserCategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize(Roles = "User")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}


