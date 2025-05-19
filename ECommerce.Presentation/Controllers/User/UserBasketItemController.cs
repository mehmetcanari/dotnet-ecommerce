using ECommerce.Application.DTO.Request.BasketItem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/v1/user/basket")]
[Authorize(Roles = "User")]
[ApiVersion("1.0")]
public class UserBasketItemController : ControllerBase
{
    private readonly IBasketItemService _basketItemService;

    public UserBasketItemController(IBasketItemService basketItemService)
    {
        _basketItemService = basketItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBasketItems()
    {
        var basketItems = await _basketItemService.GetAllBasketItemsAsync();
        return Ok(new { message = "Basket items fetched successfully", data = basketItems });
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBasketItem([FromBody] CreateBasketItemRequestDto createBasketItemRequest)
    {
        var result = await _basketItemService.CreateBasketItemAsync(createBasketItemRequest);
        return Created("basket", new { message = "Basket item created successfully", data = result });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] UpdateBasketItemRequestDto basketItemRequestUpdateRequest)
    {
        var result = await _basketItemService.UpdateBasketItemAsync(basketItemRequestUpdateRequest);
        return Ok(new { message = "Basket item updated successfully", data = result });
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAllBasketItems()
    {
        var result = await _basketItemService.DeleteAllNonOrderedBasketItemsAsync();
        return Ok(new { message = "All basket items deleted successfully", data = result });
    }
}