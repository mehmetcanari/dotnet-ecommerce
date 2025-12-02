using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface ICategoryRepository
{
    Task Create(Category category, CancellationToken cancellationToken = default);
    Task<List<Category>> Read(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<bool> CheckNameExists(string name, CancellationToken cancellationToken = default);
    Task<Category?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task Update(Category category, CancellationToken cancellationToken = default);
    Task Delete(Category category, CancellationToken cancellationToken = default);
}