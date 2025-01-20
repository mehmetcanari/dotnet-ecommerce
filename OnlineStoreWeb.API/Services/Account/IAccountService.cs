public interface IAccountService
{
    Task AddAccountAsync(AccountRegisterDto createUserDto);
    Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto);
    Task<List<Account>> GetAllAccountsAsync();
    Task<Account?> GetAccountWithIdAsync(int id);
    Task DeleteAccountAsync(int id);
}