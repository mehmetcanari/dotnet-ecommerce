using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Account;

namespace OnlineStoreWeb.API.Services.Account;

public interface IAccountService
{
    Task RegisterAccountAsync(AccountRegisterRequestDto createUserRequestDto, string role);
    Task UpdateAccountAsync(string email, AccountUpdateRequestDto updateRequestUserDto);
    Task<List<AccountResponseDto>> GetAllAccountsAsync();
    Task<AccountResponseDto> GetAccountWithIdAsync(int id);
    Task<AccountResponseDto> GetAccountByEmailAsync(string email);
    Task DeleteAccountAsync(int id);
}