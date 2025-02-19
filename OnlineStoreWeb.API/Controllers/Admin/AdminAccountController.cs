using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.Model;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(int id)
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

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
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