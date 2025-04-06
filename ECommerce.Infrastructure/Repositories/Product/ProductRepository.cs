using ECommerce.Application.Interfaces.Repository;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.Product;

public class ProductRepository(StoreDbContext context) : IProductRepository
{
    public async Task<List<Domain.Model.Product>> Read()
    {
        try
        {
            return await context.Products.AsNoTracking().ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch products", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Create(Domain.Model.Product product)
    {
        try
        {
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to save product", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Update(Domain.Model.Product product)
    {
        try
        {
            context.Products.Update(product);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update product", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Delete(Domain.Model.Product product)
    {
        try
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete product", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}