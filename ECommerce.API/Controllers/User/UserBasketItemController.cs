using System.Security.Claims;
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
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;
            var basketItems = await _basketItemService.GetAllBasketItemsAsync(userEmail);
            return Ok(new { message = "Basket items fetched successfully", data = basketItems });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateBasketItem([FromBody] CreateBasketItemRequestDto createBasketItemRequest)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Email);
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User identity not found" });
        }

        var userEmail = userIdClaim.Value;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _basketItemService.CreateBasketItemAsync(createBasketItemRequest, userEmail);
            return Created("basket", new { message = "Basket item created successfully", data = result });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] UpdateBasketItemRequestDto basketItemRequestUpdateRequest)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.Email);
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User identity not found" });
        }

        var userEmail = userIdClaim.Value;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _basketItemService.UpdateBasketItemAsync(basketItemRequestUpdateRequest, userEmail);
            return Ok(new { message = "Basket item updated successfully", data = result });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAllBasketItems()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;

            var result = await _basketItemService.DeleteAllBasketItemsAsync(userEmail);
            return Ok(new { message = "All basket items deleted successfully", data = result });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}