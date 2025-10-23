using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Account;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Queries.Account;
using ECommerce.Application.Validations.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class AccountController(IMediator _mediator, IAccountService _accountService, IRefreshTokenService _refreshTokenService) : ApiBaseController
{
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile() => HandleResult(await _mediator.Send(new GetClientAccountQuery()));

    [Authorize("Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts() => HandleResult(await _mediator.Send(new GetAllAccountsQuery()));

    [Authorize("Admin")]
    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetAccountById([FromRoute] int id) => HandleResult(await _mediator.Send(new GetAccountWithIdQuery { Id = id }));

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteAccount([FromRoute] int id) => HandleResult(await _mediator.Send(new DeleteAccountCommand { Id = id }));
    [Authorize("Admin")]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequestDto request) => HandleResult(await _refreshTokenService.RevokeUserTokens(request));

    [Authorize("Admin")]
    [HttpPost("restrict")]
    public async Task<IActionResult> BanAccount([FromBody] AccountBanRequestDto request) => HandleResult(await _accountService.BanAccountAsync(request));

    [Authorize("Admin")]
    [HttpPost("unrestrict")]
    public async Task<IActionResult> UnbanAccount([FromBody] AccountUnbanRequestDto request) => HandleResult(await _accountService.UnbanAccountAsync(request));
} 