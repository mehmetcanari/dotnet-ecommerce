using Asp.Versioning;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Interfaces.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;
[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
public class AdminAccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IRefreshTokenService _refreshTokenService;
    public AdminAccountController(IAccountService accountService, IRefreshTokenService refreshTokenService)
    {
        _accountService = accountService;
        _refreshTokenService = refreshTokenService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        try
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(new { message = "All accounts fetched successfully", accounts });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById([FromRoute] int id)
    {
        try
        {
            var account = await _accountService.GetAccountWithIdAsync(id);
            return Ok(new { message = $"Account with id {id} fetched successfully", account });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] int id)
    {
        try
        {
            await _accountService.DeleteAccountAsync(id);
            return Ok(new { message = $"Account with id {id} deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
  
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequestDto request)
    {
        try
        {
            await _refreshTokenService.RevokeUserTokens(request.Email, "Admin revoked");
            return Ok(new { message = $"{request.Email} tokens revoked successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("ban")]
    public async Task<IActionResult> BanAccount([FromBody] AccountBanRequestDto request)
    {
        try
        {
            await _accountService.BanAccountAsync(request.Email, request.Until, request.Reason);
            return Ok(new { message = $"{request.Email} account banned successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("unban")]
    public async Task<IActionResult> UnbanAccount([FromBody] AccountUnbanRequestDto request)
    {
        try
        {
            await _accountService.UnbanAccountAsync(request.Email);
            return Ok(new { message = $"{request.Email} account unbanned successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}