using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.Services.Account;

namespace OnlineStoreWeb.API.Controllers.Admin;

[ApiController]
[Route("api/admin/accounts")]
public class AdminAccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    
    public AdminAccountController(IAccountService accountService)
    {
        _accountService = accountService;
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
}