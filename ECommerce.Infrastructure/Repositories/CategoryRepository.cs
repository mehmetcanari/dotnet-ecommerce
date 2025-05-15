using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.Infrastructure.Repositories;
public class CategoryRepository : ICategoryRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public CategoryRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Create(Category category)
    {
        try
        {
            await _context.Categories.AddAsync(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading categories");
            throw;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category exists with name");
            throw;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by ID");
            throw;
        }
    }

    public void Update(Category category)
    {
        try
        {
            _context.Categories.Update(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            throw;
        }
    }

    public void Delete(Category category)
    {
        try
        {
            _context.Categories.Remove(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            throw;
        }
    }
}
