using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}