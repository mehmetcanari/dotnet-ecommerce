using System.Security.Claims;
using Asp.Versioning;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/user/orders")]
[Authorize(Roles = "User")]
[ApiVersion("1.0")]
public class UserOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public UserOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderCreateRequestDto)
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
            await _orderService.AddOrderAsync(orderCreateRequestDto, userEmail);
            return Created($"orders", new { message = "Order created successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;

            var order = await _orderService.GetUserOrdersAsync(userEmail);
            return Ok(new { message = "Order fetched successfully", data = order });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelOrder()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;

            await _orderService.CancelOrderAsync(userEmail);
            return Ok(new { message = "Order cancelled successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}