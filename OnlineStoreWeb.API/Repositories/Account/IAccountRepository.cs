public interface IAccountRepository
{
    Task<List<Account>> Get();
    Task Add(Account userAccount);
    Task Update(Account account);
    Task Delete(Account account);
}