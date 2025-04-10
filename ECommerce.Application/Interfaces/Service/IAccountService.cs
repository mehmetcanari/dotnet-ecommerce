using ECommerce.Application.DTO.Request.Account;
using ECommerce.Application.DTO.Response.Account;

namespace ECommerce.Application.Interfaces.Service;

public interface IAccountService
{
    Task RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role);
    Task<List<AccountResponseDto>> GetAllAccountsAsync();
    Task<AccountResponseDto> GetAccountWithIdAsync(int id);
    Task<AccountResponseDto> GetAccountByEmailAsync(string email);
    Task DeleteAccountAsync(int id);    
}