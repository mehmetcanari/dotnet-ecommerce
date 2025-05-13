using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;

namespace ECommerce.Application.Abstract.Service;

public interface IAccountService
{
    Task RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role);
    Task<List<AccountResponseDto>> GetAllAccountsAsync();
    Task<Domain.Model.Account> GetAccountByEmailAsModel(string email);
    Task<AccountResponseDto> GetAccountWithIdAsync(int id);
    Task<AccountResponseDto> GetAccountByEmailAsync(string email);
    Task DeleteAccountAsync(int id);
    Task BanAccountAsync(string email, DateTime until, string reason);
    Task UnbanAccountAsync(string email);
}