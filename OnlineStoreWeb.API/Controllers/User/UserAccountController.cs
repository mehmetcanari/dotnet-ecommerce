using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.User;
using OnlineStoreWeb.API.Services.Account;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/account")]
public class UserAccountController(IAccountService accountService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(AccountRegisterDto accountRegisterRequest)
    {
        try
        {
            await accountService.AddAccountAsync(accountRegisterRequest);
            return Created($"users", new { message = "User created successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while creating the user");
        }
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateProfile(int id, AccountUpdateDto accountUpdateRequest)
    {
        try
        {
            await accountService.UpdateAccountAsync(id, accountUpdateRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while updating the user");
        }
    }
} 