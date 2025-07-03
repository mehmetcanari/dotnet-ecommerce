using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Abstract.Service;

public interface IAccountService
{
    Task<Result> RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role);
    Task<Result<Domain.Model.Account>> GetAccountByEmailAsEntityAsync(string email);
    Task<Result> BanAccountAsync(AccountBanRequestDto request);
    Task<Result> UnbanAccountAsync(AccountUnbanRequestDto request);
}