public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task AddAccountAsync(AccountRegisterDto createUserDto)
    {
        try
        {
            List<Account> accounts = await _accountRepository.Get();
            if(accounts.Any(a => a.Email == createUserDto.Email)) //Duplicate email check
            {
                throw new Exception("Email already exists");
            }

            Account account = new Account
            {
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                Password = createUserDto.Password,
                Address = createUserDto.Address,
                PhoneNumber = createUserDto.PhoneNumber,
                DateOfBirth = createUserDto.DateOfBirth,
                UserCreated = DateTime.UtcNow,
                UserUpdated = DateTime.UtcNow
            };

            await _accountRepository.Add(account);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateAccountAsync(int id, AccountUpdateDto updateUserDto)
    {
         try
        {
            List<Account> accounts = await _accountRepository.Get();
            Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");

            account.Email = updateUserDto.Email;
            account.Password = updateUserDto.Password;
            account.Address = updateUserDto.Address;
            account.PhoneNumber = updateUserDto.PhoneNumber;
            account.UserUpdated = DateTime.UtcNow;

            await _accountRepository.Update(account);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        try
        {
            List<Account> accounts = await _accountRepository.Get();
            return accounts;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Account?> GetAccountWithIdAsync(int id)
    {
        try
        {
            List<Account> accounts = await _accountRepository.Get();
            Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            return account;
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAccountAsync(int id)
    {
        try
        {
            List<Account> accounts = await _accountRepository.Get();
            Account account = accounts.FirstOrDefault(a => a.Id == id) ?? throw new Exception("User not found");
            await _accountRepository.Delete(account);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}