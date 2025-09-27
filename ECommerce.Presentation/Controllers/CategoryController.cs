using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Queries.Category;
using ECommerce.Application.Validations.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class CategoryController(IMediator _mediator, ICategoryService _categoryService) : ControllerBase
{
    [Authorize("Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
    {
        var result = await _categoryService.CreateCategoryAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Category created successfully" });
    }

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Category deleted successfully" });
    }

    [Authorize("Admin")]
    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryRequestDto request)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, request);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Category updated successfully" });
    }

    [Authorize]
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


