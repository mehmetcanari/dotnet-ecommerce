using ECommerce.Application.DTO.Request.BasketItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using MediatR;
using ECommerce.Application.Queries.Basket;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize(Roles = "User")]
public class CartItemsController : ControllerBase
{
    private readonly IBasketItemService _basketItemService;
    private readonly IMediator _mediator;

    public CartItemsController(IBasketItemService basketItemService, IMediator mediator)
    {
        _basketItemService = basketItemService;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBasketItems()
    {
        var basketItems = await _mediator.Send(new GetAllBasketItemsQuery());
        if (basketItems.IsFailure)
        {
            return NotFound(new { message = "Failed to fetch basket items", error = basketItems.Error });
        }
        return Ok(new { message = "Basket items fetched successfully", data = basketItems.Data });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBasketItem([FromBody] CreateBasketItemRequestDto createBasketItemRequest)
    {
        var result = await _basketItemService.CreateBasketItemAsync(createBasketItemRequest);
        if (result.IsFailure)
        {
            return BadRequest(new { message = "Failed to create basket item", error = result.Error });
        }
        return Created("basket", new { message = "Basket item created successfully" });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] UpdateBasketItemRequestDto basketItemRequestUpdateRequest)
    {
        var result = await _basketItemService.UpdateBasketItemAsync(basketItemRequestUpdateRequest);
        if (result.IsFailure)
        {
            return BadRequest(new { message = "Failed to update basket item", error = result.Error });
        }
        return Ok(new { message = "Basket item updated successfully" });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAllBasketItems()
    {
        var result = await _basketItemService.DeleteAllNonOrderedBasketItemsAsync();
        if (result.IsFailure)
        {
            return BadRequest(new { message = "Failed to delete basket items", error = result.Error });
        }
        return Ok(new { message = "All basket items deleted successfully" });
    }
}