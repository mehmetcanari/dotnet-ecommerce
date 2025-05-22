using Asp.Versioning;
using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Request.Token;
using ECommerce.Application.Validations.Attribute;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers.Admin;
[ApiController]
[Route("api/v1/admin/accounts")]
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
        var accounts = await _accountService.GetAllAccountsAsync();
        return Ok(new { message = "All accounts fetched successfully", accounts });
    }

    [HttpGet("{id}")]
    [ValidateId]
    public async Task<IActionResult> GetAccountById([FromRoute] int id)
    {
        var account = await _accountService.GetAccountWithIdAsync(id);
        return Ok(new { message = $"Account with id {id} fetched successfully", account });
    }

    [HttpDelete("delete/{id}")]
    [ValidateId]
    public async Task<IActionResult> DeleteAccount([FromRoute] int id)
    {
        var result = await _accountService.DeleteAccountAsync(id);
        return Ok(new { message = $"Account with id {id} deleted successfully", data = result });
    }
  
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRevokeRequestDto request)
    {
        var result = await _refreshTokenService.RevokeUserTokens(request);
        return Ok(new { message = $"{request.Email} tokens revoked successfully", data = result });
    }

    [HttpPost("ban")]
    public async Task<IActionResult> BanAccount([FromBody] AccountBanRequestDto request)
    {
        var result = await _accountService.BanAccountAsync(request);
        return Ok(new { message = $"{request.Email} account banned successfully", data = result });
    }

    [HttpPost("unban")]
    public async Task<IActionResult> UnbanAccount([FromBody] AccountUnbanRequestDto request)
    {
        var result = await _accountService.UnbanAccountAsync(request);
        return Ok(new { message = $"{request.Email} account unbanned successfully", data = result });
    }
}