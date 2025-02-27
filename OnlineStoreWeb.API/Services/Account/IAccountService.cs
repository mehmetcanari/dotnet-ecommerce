using OnlineStoreWeb.API.DTO.Request.Account;

namespace OnlineStoreWeb.API.Services.Account;

public interface IAccountService
{
    Task AddAccountAsync(AccountRegisterDto createUserDto);
    Task LoginAccountAsync(AccountLoginDto accountLoginDto);
    Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto);
    Task<List<Model.Account>> GetAllAccountsAsync();
    Task<Model.Account> GetAccountWithIdAsync(int id);
    Task DeleteAccountAsync(int id);
}