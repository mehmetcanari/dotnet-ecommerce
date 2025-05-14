using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IAccountRepository
{
    Task Create(Account userAccount);
    Task<List<Account>> Read();
    Task<Account?> GetAccountById(int id);
    Task<Account?> GetAccountByEmail(string email);
    void Update(Account account);
    void Delete(Account account);
}