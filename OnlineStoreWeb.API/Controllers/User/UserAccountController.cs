using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.User;
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
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(AccountRegisterDto accountRegisterRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            await _accountService.AddAccountAsync(accountRegisterRequest);
            return Created($"users", new { message = "User created successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while creating the user");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AccountLoginDto accountLoginRequest)
    {
        try
        {
            await _accountService.LoginAccountAsync(accountLoginRequest);
            return Ok(new { message = "User logged in successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, "Invalid email or password");
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
            await _accountService.UpdateAccountAsync(id, accountUpdateRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while updating the user");
        }
    }
} 