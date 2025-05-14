using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service;

public interface IAccountService
{
    Task<Result> RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role);
    Task<Result<List<AccountResponseDto>>> GetAllAccountsAsync();
    Task<Result<Domain.Model.Account>> GetAccountByEmailAsEntityAsync(string email);
    Task<Result<AccountResponseDto>> GetAccountWithIdAsync(int id);
    Task<Result<AccountResponseDto>> GetAccountByEmailAsResponseAsync(string email);
    Task<Result> DeleteAccountAsync(int id);
    Task<Result> BanAccountAsync(string email, DateTime until, string reason);
    Task<Result> UnbanAccountAsync(string email);
}