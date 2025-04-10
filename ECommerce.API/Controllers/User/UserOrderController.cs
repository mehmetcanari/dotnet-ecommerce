using System.Security.Claims;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/user/orders")]
public class UserOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public UserOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = "User")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequestDto orderCreateRequestRequest)
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
            await _orderService.AddOrderAsync(orderCreateRequestRequest, userEmail);
            return Created($"orders", new { message = "Order created successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [Authorize(Roles = "User")]
    [HttpGet("orders")]
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

            var order = await _orderService.GetOrdersAsync(userEmail);
            return Ok(new { message = "Order fetched successfully", data = order });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [Authorize(Roles = "User")]
    [HttpPut("cancel-order")]
    public async Task<IActionResult> CancelOrder([FromBody] OrderCancelRequestDto orderCancelRequestDto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Email);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "User identity not found" });
            }

            var userEmail = userIdClaim.Value;

            await _orderService.CancelOrderAsync(userEmail, orderCancelRequestDto);
            return Ok(new { message = "Order cancelled successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
}