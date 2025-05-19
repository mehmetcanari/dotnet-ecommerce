using System.Security.Claims;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.User;

[ApiController]
[Route("api/v1/user/orders")]
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
        /*var userIdClaim = User.FindFirst(ClaimTypes.Email); //TODO: User claim logic must be in service layer
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User identity not found" });
        }

        var userEmail = userIdClaim.Value;*/
        
        var result = await _orderService.CreateOrderAsync(orderCreateRequestDto, userEmail);
        return Ok(new { message = "Order created successfully", data = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        /*var userIdClaim = User.FindFirst(ClaimTypes.Email); //TODO: User claim logic must be in service layer
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User identity not found" });
        }

        var userEmail = userIdClaim.Value;*/

        var userOrders = await _orderService.GetUserOrdersAsync(userEmail);
        return Ok(new { message = "Order fetched successfully", data = userOrders });
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelOrder()
    {
        /*var userIdClaim = User.FindFirst(ClaimTypes.Email);  //TODO: User claim logic must be in service layer
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "User identity not found" });
        }

        var userEmail = userIdClaim.Value;*/

        var result = await _orderService.CancelOrderAsync(userEmail);
        return Ok(new { message = "Order cancelled successfully", data = result });
    }
}