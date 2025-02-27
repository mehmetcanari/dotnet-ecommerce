using OnlineStoreWeb.API.DTO.Request.Account;
using OnlineStoreWeb.API.DTO.Response.Account;

namespace OnlineStoreWeb.API.Services.Account;

public interface IAccountService
{
    Task AddAccountAsync(AccountRegisterDto createUserDto);
    Task LoginAccountAsync(AccountLoginDto accountLoginDto);
    Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto);
    Task<List<AccountResponseDto>> GetAllAccountsAsync();
    Task<AccountResponseDto> GetAccountWithIdAsync(int id);
    Task DeleteAccountAsync(int id);
}