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
    
    public async Task<Product?> GetProductById(int id)
    {
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .FirstOrDefaultAsync();

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving product by id: {Id}", id);
            throw new Exception($"An unexpected error occurred while retrieving product by id: {id}", ex);
        }
    }
    
    public async Task<bool> CheckProductExistsWithName(string name)
    {
        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Name == name)
                .FirstOrDefaultAsync();

            return product != null;
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