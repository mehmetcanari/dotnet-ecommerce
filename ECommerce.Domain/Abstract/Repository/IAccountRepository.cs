using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IAccountRepository
{
    Task Create(Account userAccount, CancellationToken cancellationToken = default);
    Task<List<Account>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Account> GetAccountById(int id, CancellationToken cancellationToken = default);
    Task<Account> GetAccountByEmail(string email, CancellationToken cancellationToken = default);
    Task<Account> GetAccountByIdentityNumber(string identityNumber, CancellationToken cancellationToken = default);
    void Update(Account account);
    void Delete(Account account);
}