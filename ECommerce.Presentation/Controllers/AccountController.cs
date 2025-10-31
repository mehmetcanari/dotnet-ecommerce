using ECommerce.Application.Commands.Account;
using ECommerce.Application.Commands.Token;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Queries.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class AccountController(IMediator mediator) : ApiBaseController
{
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile() => HandleResult(await mediator.Send(new GetProfileQuery()));

    [Authorize("Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts(int page, int pageSize) => HandleResult(await mediator.Send(new GetAllAccountsQuery(pageSize, page)));

    [Authorize("Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById([FromRoute] Guid id) => HandleResult(await mediator.Send(new GetAccountByIdQuery(id)));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] Guid id) => HandleResult(await mediator.Send(new DeleteAccountCommand(id)));

    [Authorize("Admin")]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequestDto request) => HandleResult(await mediator.Send(new RevokeRefreshTokenCommand(request)));

    [Authorize("Admin")]
    [HttpPost("restrict")]
    public async Task<IActionResult> BanAccount([FromBody] AccountBanRequestDto request) => HandleResult(await mediator.Send(new BanAccountCommand(request)));

    [Authorize("Admin")]
    [HttpPost("unrestrict")]
    public async Task<IActionResult> UnbanAccount([FromBody] AccountUnbanRequestDto request) => HandleResult(await mediator.Send(new UnbanAccountCommand(request)));
} 