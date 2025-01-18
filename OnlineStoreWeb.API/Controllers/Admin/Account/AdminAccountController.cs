using Microsoft.AspNetCore.Mvc;

namespace OnlineStoreWeb.API.Controllers.Admin.Account;

[ApiController]
[Route("api/admin/accounts")]
public class AdminAccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<AdminAccountController> _logger;

    public AdminAccountController(IAccountRepository accountRepository, ILogger<AdminAccountController> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        try
        {
            var accounts = await _accountRepository.GetAllAccountsAsync();
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching accounts: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching accounts");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(int id)
    {
        try
        {
            var account = await _accountRepository.GetAccountWithIdAsync(id);
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching account: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while fetching the account");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            await _accountRepository.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting account: {Message}", ex.Message);
            return StatusCode(500, "An unexpected error occurred while deleting the account");
        }
    }
}