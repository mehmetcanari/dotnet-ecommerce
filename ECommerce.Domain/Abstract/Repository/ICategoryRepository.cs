using ECommerce.Domain.Model;

namespace ECommerce.Domain.Abstract.Repository;

public interface ICategoryRepository
{
    Task Create(Category category);
    Task<List<Category>> Read(int pageNumber = 1, int pageSize = 50);
    Task<bool> CheckCategoryExistsWithName(string name);
    Task<Category?> GetCategoryById(int id);
    void Update(Category category);
    void Delete(Category category);
}