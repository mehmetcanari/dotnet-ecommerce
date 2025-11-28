using ECommerce.Application.Commands.Basket;
using ECommerce.Application.Queries.Basket;
using ECommerce.Shared.DTO.Request.BasketItem;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize(Roles = "User")]
public class BasketController(IMediator mediator) : ApiBaseController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateBasketItem([FromBody] CreateBasketItemRequestDto request) => HandleResult(await mediator.Send(new CreateBasketItemCommand(request)));

    [HttpPut("update")]
    public async Task<IActionResult> UpdateBasketItem([FromBody] UpdateBasketItemRequestDto request) => HandleResult(await mediator.Send(new UpdateBasketItemCommand(request)));

    [HttpPost("clear")]
    public async Task<IActionResult> ClearBasket() => HandleResult(await mediator.Send(new ClearBasketCommand()));

    [HttpPost("remove/{id}")]
    public async Task<IActionResult> RemoveItem([FromRoute] Guid id) => HandleResult(await mediator.Send(new RemoveBasketItemById(id)));

    [HttpGet]
    public async Task<IActionResult> GetAllBasketItems() => HandleResult(await mediator.Send(new GetBasketQuery()));
}