using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;

namespace ECommerce.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public ProductRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Product>> Read()
    {
        try
        {
            IQueryable<Product> query = _context.Products;

            var products = await query
            .AsNoTracking()
            .ToListAsync();
            
            return products;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to fetch products");
            throw new DbUpdateException("Failed to fetch products", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Create(Product product)
    {
        try
        {
            await _context.Products.AddAsync(product);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save product");
            throw new DbUpdateException("Failed to save product", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public void Update(Product product)
    {
        try
        {
            _context.Products.Update(product);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update product");
            throw new DbUpdateException("Failed to update product", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public void Delete(Product product)
    {
        try
        {
            _context.Products.Remove(product);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete product");
            throw new DbUpdateException("Failed to delete product", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}