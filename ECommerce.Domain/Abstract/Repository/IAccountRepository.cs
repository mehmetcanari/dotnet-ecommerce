using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IAccountRepository
{
    Task Create(Account userAccount);
    Task<List<Account>> Read(int pageNumber = 1, int pageSize = 50);
    Task<Account?> GetAccountById(int id);
    Task<Account?> GetAccountByEmail(string email);
    void Update(Account account);
    void Delete(Account account);
}