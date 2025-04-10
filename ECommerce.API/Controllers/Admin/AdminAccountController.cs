using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;

[ApiController]
[Route("api/admin/accounts")]
public class AdminAccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IRefreshTokenService _refreshTokenService;
    public AdminAccountController(IAccountService accountService, IRefreshTokenService refreshTokenService)
    {
        _accountService = accountService;
        _refreshTokenService = refreshTokenService;
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        try
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById([FromRoute] int id)
    {
        try
        {
            var account = await _accountService.GetAccountWithIdAsync(id);
            return Ok(account);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] int id)
    {
        try
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequestDto request)
    {
        try
        {
            await _refreshTokenService.RevokeUserTokensAsync(request.Email, "Admin revoked");
            return Ok(new { message = "Tokens revoked successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}