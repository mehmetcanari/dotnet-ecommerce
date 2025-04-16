using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository : ICategoryRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public CategoryRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Category> Create(Category category)
    {
        try
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
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
            return await _context.Categories
            .Include(c => c.Products)
            .AsNoTracking()
            .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading categories");
            throw;
        }
    }

    public async Task<Category> Update(Category category)
    {
        try
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            throw;
        }
    }

    public async Task<Category> Delete(int categoryId)
    {
        try
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                throw new Exception("Category not found");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            throw;
        }
    }
}
