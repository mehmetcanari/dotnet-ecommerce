using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Account;

namespace OnlineStoreWeb.API.Services.Account;

public interface IAccountService
{
    Task RegisterAccountAsync(AccountRegisterDto createUserDto, string role);
    Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto);
    Task<List<AccountResponseDto>> GetAllAccountsAsync();
    Task<AccountResponseDto> GetAccountWithIdAsync(int id);
    Task<AccountResponseDto> GetAccountByEmailAsync(string email);
    Task DeleteAccountAsync(int id);
}