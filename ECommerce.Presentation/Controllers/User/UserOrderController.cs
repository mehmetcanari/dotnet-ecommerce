using System.Security.Claims;
using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using MediatR;
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
    private readonly IMediator _mediator;

    public UserOrderController(IOrderService orderService, IMediator mediator)
    {
        _orderService = orderService;
        _mediator = mediator;
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
        var result = await _mediator.Send(new CancelOrderCommand());
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Order cancelled successfully", data = result });
    }
}