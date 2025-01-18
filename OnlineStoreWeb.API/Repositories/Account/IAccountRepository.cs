public interface IAccountRepository
{
    Task<List<Account>> GetAllAccountsAsync();
    Task<Account?> GetAccountWithIdAsync(int id);
    Task AddAccountAsync(AccountRegisterDto createUserDto);
    Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto);
    Task DeleteAccountAsync(int id);
}