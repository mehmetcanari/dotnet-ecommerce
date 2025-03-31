using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.Services.Account;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/account")]
public class UserAccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    
    public UserAccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProfile(int id, AccountUpdateDto accountUpdateRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await _accountService.UpdateAccountAsync(id, accountUpdateRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
} 