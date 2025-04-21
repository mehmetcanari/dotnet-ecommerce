using ECommerce.Domain.Model;

public interface ICategoryRepository
{
    Task Create(Category category);
    Task<List<Category>> Read();
    void Update(Category category);
    void Delete(Category category);
}