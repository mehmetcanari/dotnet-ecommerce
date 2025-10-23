using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IAccountRepository
{
    Task CreateAsync(User userAccount, CancellationToken cancellationToken = default);
    Task<List<User>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<User> GetAccountById(string userId, CancellationToken cancellationToken = default);
    Task<User> GetAccountByEmail(string email, CancellationToken cancellationToken = default);
    Task<User> GetAccountByIdentityNumber(string identityNumber, CancellationToken cancellationToken = default);
    void Update(User account);
    void Delete(User account);
}