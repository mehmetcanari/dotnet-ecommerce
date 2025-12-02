using ECommerce.Application.Commands.Wishlist;
using ECommerce.Application.Queries.Wishlist;
using ECommerce.Shared.DTO.Request.Wishlist;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize(Roles = "User")]
public class WishlistController(IMediator mediator) : ApiBaseController
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateWishlistItem([FromBody] WishlistItemCreateRequestDto request) => HandleResult(await mediator.Send(new CreateWishlistItemCommand(request)));

    [HttpPost("delete")]
    public async Task<IActionResult> DeleteWishlistItem([FromBody] WishlistItemDeleteRequestDto request) => HandleResult(await mediator.Send(new DeleteWishlistItemCommand(request)));

    [HttpPost]
    public async Task<IActionResult> GetWishlistItems(QueryPagination pagination) => HandleResult(await mediator.Send(new GetWishlistItemQuery(pagination)));
}