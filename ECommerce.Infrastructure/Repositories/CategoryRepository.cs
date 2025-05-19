using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.Infrastructure.Repositories;
public class CategoryRepository : ICategoryRepository
{
    private readonly StoreDbContext _context;

    public CategoryRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task Create(Category category)
    {
        try
        {
            await _context.Categories.AddAsync(category);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating category", exception);
        }
    }

    public async Task<List<Category>> Read()
    {
        try
        {
            IQueryable<Category> query = _context.Categories;

            var categories = await query
            .AsNoTracking()
            .Include(c => c.Products)
            .ToListAsync();

            return categories;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while reading categories", exception);
        }
    }
    
    public async Task<bool> CheckCategoryExistsWithName(string name)
    {
        try
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Name == name)
                .FirstOrDefaultAsync();

            return category != null;
        }
        catch (Exception exception)
        {
            throw new Exception($"An unexpected error occurred while checking category existence with name: {name}", exception);
        }
    }
    
    public async Task<Category?> GetCategoryById(int id)
    {
        try
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == id)
                .Include(c => c.Products)
                .FirstOrDefaultAsync();

            return category;
        }
        catch (Exception exception)
        {
            throw new Exception($"An unexpected error occurred while retrieving category by id", exception);
        }
    }

    public void Update(Category category)
    {
        try
        {
            _context.Categories.Update(category);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating category", exception);
        }
    }

    public void Delete(Category category)
    {
        try
        {
            _context.Categories.Remove(category);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting category", exception);
        }
    }
}
