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
        var result = await _orderService.CreateOrderAsync(orderCreateRequestDto);
        return Ok(new { message = "Order created successfully", data = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var userOrders = await _orderService.GetUserOrdersAsync();
        return Ok(new { message = "Order fetched successfully", data = userOrders });
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelOrder()
    {
        var result = await _orderService.CancelOrderAsync();
        return Ok(new { message = "Order cancelled successfully", data = result });
    }
}