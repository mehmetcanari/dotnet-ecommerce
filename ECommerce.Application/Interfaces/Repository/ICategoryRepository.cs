using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Repository;

public interface ICategoryRepository
{
    Task Create(Category category);
    Task<List<Category>> Read();
    void Update(Category category);
    void Delete(Category category);
}