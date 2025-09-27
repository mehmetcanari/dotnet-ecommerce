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
public class AccountController(IMediator _mediator, IAccountService _accountService, IRefreshTokenService _refreshTokenService) : ControllerBase
{
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.Send(new GetClientAccountQuery());
        if (result.IsFailure)
        {
            return NotFound(new { message = "Failed to fetch profile", error = result.Error });
        }
        return Ok(new { message = "Profile fetched successfully", data = result.Data });
    }

    [Authorize("Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var result = await _mediator.Send(new GetAllAccountsQuery());
        if (result.IsFailure)
        {
            return NotFound(new { message = result.Error });
        }
        return Ok(new { message = "All accounts fetched successfully", data = result.Data });
    }

    [Authorize("Admin")]
    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetAccountById([FromRoute] int id)
    {
        var account = await _mediator.Send(new GetAccountWithIdQuery { Id = id });
        if (account.IsFailure)
        {
            return NotFound(new { message = account.Error });
        }
        return Ok(new { message = $"Account with id {id} fetched successfully", data = account.Data });
    }

    [Authorize("Admin")]
    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteAccount([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteAccountCommand { Id = id });
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = $"Account with id {id} deleted successfully" });
    }

    [Authorize("Admin")]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequestDto request)
    {
        var result = await _refreshTokenService.RevokeUserTokens(request);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = $"{request.Email} tokens revoked successfully" });
    }

    [Authorize("Admin")]
    [HttpPost("restrict")]
    public async Task<IActionResult> BanAccount([FromBody] AccountBanRequestDto request)
    {
        var result = await _accountService.BanAccountAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = $"{request.Email} account banned successfully" });
    }

    [Authorize("Admin")]
    [HttpPost("unrestrict")]
    public async Task<IActionResult> UnbanAccount([FromBody] AccountUnbanRequestDto request)
    {
        var result = await _accountService.UnbanAccountAsync(request);
        if (result.IsFailure)
        {
            return BadRequest(new { message = result.Error });
        }
        return Ok(new { message = $"{request.Email} account unbanned successfully" });
    }
} 