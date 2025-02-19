using OnlineStoreWeb.API.DTO.User;

namespace OnlineStoreWeb.API.Services.Account;

public interface IAccountService
{
    Task AddAccountAsync(AccountRegisterDto createUserDto);
    Task LoginAccountAsync(AccountLoginDto accountLoginDto);
    Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto);
    Task PartialUpdateAccountAsync(int id, AccountPatchDto patchUserDto);
    Task<List<Model.Account>> GetAllAccountsAsync();
    Task<Model.Account> GetAccountWithIdAsync(int id);
    Task DeleteAccountAsync(int id);
}