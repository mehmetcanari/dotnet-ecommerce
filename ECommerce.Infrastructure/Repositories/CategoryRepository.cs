using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore;


namespace ECommerce.Infrastructure.Repositories;
public class CategoryRepository : ICategoryRepository
{
    private readonly StoreDbContext _context;

    public CategoryRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task Create(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Categories.AddAsync(category, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<List<Category>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Category> query = _context.Categories;

            var categories = await query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return categories;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<bool> CheckCategoryExistsWithName(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Category> query = _context.Categories;

            var category = await query
                .AsNoTracking()
                .Where(c => c.Name == name)
                .FirstOrDefaultAsync(cancellationToken);

            return category != null;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<Category> GetCategoryById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Category> query = _context.Categories;

            var category = await query
                .AsNoTracking()
                .Where(c => c.CategoryId == id)
                .FirstOrDefaultAsync(cancellationToken);

            return category;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
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
            throw new Exception(ErrorMessages.UnexpectedError, exception);
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
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
