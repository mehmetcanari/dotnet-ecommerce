using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using OnlineStoreWeb.API.DTO.User;
using OnlineStoreWeb.API.Services.Account;
using OnlineStoreWeb.API.Validations;

namespace OnlineStoreWeb.API.Controllers.User;

[ApiController]
[Route("api/account")]
public class UserAccountController(
    IAccountService accountService, 
    IValidator<AccountRegisterDto> registerValidator,
    IValidator<AccountUpdateDto> updateValidator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(AccountRegisterDto accountRegisterRequest)
    {
        ValidationResult result = await registerValidator.ValidateAsync(accountRegisterRequest);
        if (!result.IsValid)
        {
            result.AddToModelState(this.ModelState, null);
            return BadRequest(this.ModelState);
        }
        
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
        ValidationResult result = await updateValidator.ValidateAsync(accountUpdateRequest);
        
        try
        {
            if (!result.IsValid)
            {
                result.AddToModelState(this.ModelState, null);
                return BadRequest(this.ModelState);
            }
            
            await accountService.UpdateAccountAsync(id, accountUpdateRequest);
            return Ok(new { message = "User updated successfully" });
        }
        catch 
        {
            return StatusCode(500, "An unexpected error occurred while updating the user");
        }
    }
} 