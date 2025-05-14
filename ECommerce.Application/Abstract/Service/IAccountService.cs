using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;
using ECommerce.Application.Utility;

namespace ECommerce.Application.Abstract.Service;

public interface IAccountService
{
    Task RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role);
    Task<Result<List<AccountResponseDto>>> GetAllAccountsAsync();
    Task<Result<Domain.Model.Account>> GetAccountByEmailAsEntity(string email);
    Task<Result<AccountResponseDto>> GetAccountWithIdAsync(int id);
    Task<Result<AccountResponseDto>> GetResponseAccountByEmailAsync(string email);
    Task DeleteAccountAsync(int id);
    Task BanAccountAsync(string email, DateTime until, string reason);
    Task UnbanAccountAsync(string email);
}