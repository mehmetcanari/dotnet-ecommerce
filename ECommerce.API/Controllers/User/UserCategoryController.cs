using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Validations.Attribute;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/v1/user/categories")]
[Authorize(Roles = "User")]
[ApiVersion("1.0")]
public class UserCategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public UserCategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(new { message = "Category retrieved successfully", data = category });
    }
}


