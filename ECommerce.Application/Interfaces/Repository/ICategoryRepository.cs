using ECommerce.Domain.Model;

public interface ICategoryRepository
{
    Task<Category> Create(Category category);
    Task<List<Category>> Read();
    Task<Category> Update(Category category);
    Task<Category> Delete(int categoryId);
}