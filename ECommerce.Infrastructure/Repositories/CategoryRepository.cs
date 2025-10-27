using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository(StoreDbContext context) : ICategoryRepository
{
    public async Task Create(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Categories.AddAsync(category, cancellationToken);
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
            IQueryable<Category> query = context.Categories;

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
    
    public async Task<bool> CheckNameExists(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Category> query = context.Categories;

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
    
    public async Task<Category?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Category> query = context.Categories;

            var category = await query
                .AsNoTracking()
                .Where(c => c.Id == id)
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
            context.Categories.Update(category);
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
            context.Categories.Remove(category);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}
