using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Order;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Queries.Order;
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
        if (result.IsFailure)
        {
            return BadRequest(new { message = "Failed to create order", error = result.Error });
        }
        return Ok(new { message = "Order created successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var userOrders = await _mediator.Send(new GetUserOrdersQuery());
        if (userOrders.IsFailure)
        {
            return BadRequest(new { message = userOrders.Error });
        }
        return Ok(new { message = "Orders fetched successfully", data = userOrders.Data });
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelOrder()
    {
        var result = await _mediator.Send(new CancelOrderCommand());
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = "Order cancelled successfully" });
    }
}