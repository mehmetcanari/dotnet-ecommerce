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
public class CategoryController(IMediator _mediator, ICategoryService _categoryService) : ApiBaseController
{
    [Authorize("Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request) => HandleResult(await _categoryService.CreateCategoryAsync(request));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id) => HandleResult(await _categoryService.DeleteCategoryAsync(id));

    [Authorize("Admin")]
    [HttpPut("update/{id}")]
    [ValidateId]
    public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryRequestDto request) => HandleResult(await _categoryService.UpdateCategoryAsync(id, request));

    [Authorize]
    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetCategoryById([FromRoute] int id) => HandleResult(await _mediator.Send(new GetCategoryByIdQuery { CategoryId = id }));
}


