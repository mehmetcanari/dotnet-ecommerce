using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly StoreDbContext _context;

    public ProductRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            IQueryable<Product> query = _context.Products;

            var products = await query
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        
            return products;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while reading products", exception);
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
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while retrieving product by id", exception);
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
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while checking product existence with name", exception);
        }
    }

    public async Task Create(Product product)
    {
        try
        {
            await _context.Products.AddAsync(product);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating product", exception);
        }
    }

    public void Update(Product product)
    {
        try
        {
            _context.Entry(product).State = EntityState.Modified;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating product", exception);
        }
    }

    public void Delete(Product product)
    {
        try
        {
            _context.Products.Remove(product);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting product", exception);
        }
    }
}