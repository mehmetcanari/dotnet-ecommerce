using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface IUserRepository
{
    Task CreateAsync(User userAccount, CancellationToken cancellationToken = default);
    Task<List<User>> Read(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<User?> GetById(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);
    void Update(User account);
    void Delete(User account);
}