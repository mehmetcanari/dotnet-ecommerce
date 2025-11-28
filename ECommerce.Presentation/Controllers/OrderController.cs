using ECommerce.Application.Commands.Order;
using ECommerce.Application.Queries.Order;
using ECommerce.Application.Utility;
using ECommerce.Shared.DTO.Request.Order;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class OrderController(IMediator mediator) : ApiBaseController
{
    [Authorize("User")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request) => HandleResult(await mediator.Send(new CreateOrderCommand(request)));

    [Authorize("Admin")]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusRequestDto request) => HandleResult(await mediator.Send(new UpdateOrderStatusCommand(request)));

    [Authorize("User")]
    [HttpGet("client/orders")]
    public async Task<IActionResult> GetUserOrders() => HandleResult(await mediator.Send(new GetUserOrdersQuery()));

    [Authorize("User")]
    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelActiveOrder(Guid id) => HandleResult(await mediator.Send(new CancelOrderCommand(id)));

    [Authorize("Admin")]
    [HttpPost("allOrders")]
    public async Task<IActionResult> GetAllOrders(QueryPagination pagination) => HandleResult(await mediator.Send(new GetAllOrdersQuery(pagination)));

    [Authorize("Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id) => HandleResult(await mediator.Send(new GetOrderByIdQuery(id)));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteOrder([FromRoute] Guid id) => HandleResult(await mediator.Send(new DeleteOrderByIdCommand(id)));
}