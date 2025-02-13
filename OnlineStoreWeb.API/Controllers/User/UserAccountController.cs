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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await accountService.AddAccountAsync(accountRegisterRequest);
            return Created($"users", new { message = "User created successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
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
            await accountService.UpdateAccountAsync(id, accountUpdateRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPatch("update/{id}")]
    public async Task<IActionResult> PartialUpdateProfile(int id, AccountPatchDto accountPatchRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await accountService.PartialUpdateAccountAsync(id, accountPatchRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch (Exception exception)
        {
            return BadRequest(exception.Message);
        }
    }
} 