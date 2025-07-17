using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface ICategoryRepository
{
    Task Create(Category category, CancellationToken cancellationToken = default);
    Task<List<Category>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<bool> CheckCategoryExistsWithName(string name, CancellationToken cancellationToken = default);
    Task<Category> GetCategoryById(int id, CancellationToken cancellationToken = default);
    void Update(Category category);
    void Delete(Category category);
}