using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;
    private readonly MongoDbContext _context;

    public ProductRepository(MongoDbContext context)
    {
        _context = context;
        _products = context.GetCollection<Product>("products");
        CreateIndexes();
    }

    protected virtual void CreateIndexes()
    {
        var indexKeys = Builders<Product>.IndexKeys
            .Ascending(p => p.Name)
            .Ascending(p => p.CategoryId);
        
        var categoryIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.CategoryId);
        var nameIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.Name);
        var stockIndexKeys = Builders<Product>.IndexKeys.Ascending(p => p.StockQuantity);

        _products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(indexKeys));
        _products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(categoryIndexKeys));
        _products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(nameIndexKeys));
        _products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(stockIndexKeys));
    }

    public async Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            return await _products
                .Find(_ => true)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .SortByDescending(p => p.ProductCreated)
                .ToListAsync();
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
            return await _products
                .Find(p => p.ProductId == id)
                .FirstOrDefaultAsync();
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
            var count = await _products.CountDocumentsAsync(p => p.Name == name);
            return count > 0;
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
            // Generate auto-increment ID if not set
            if (product.ProductId == 0)
            {
                product.ProductId = await _context.GetNextSequenceValue("product_id");
            }
            
            await _products.InsertOneAsync(product);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating product", exception);
        }
    }

    public async Task Update(Product product)
    {
        try
        {
            product.ProductUpdated = DateTime.UtcNow;
            await _products.ReplaceOneAsync(p => p.ProductId == product.ProductId, product);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating product", exception);
        }
    }

    public async Task Delete(Product product)
    {
        try
        {
            await _products.DeleteOneAsync(p => p.ProductId == product.ProductId);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting product", exception);
        }
    }

    public async Task<List<Product>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            return await _products
                .Find(p => p.CategoryId == categoryId)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .SortByDescending(p => p.ProductCreated)
                .ToListAsync();
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while retrieving products by category", exception);
        }
    }

    public async Task UpdateStock(int productId, int newStock)
    {
        try
        {
            var update = Builders<Product>.Update
                .Set(p => p.StockQuantity, newStock)
                .Set(p => p.ProductUpdated, DateTime.UtcNow);

            await _products.UpdateOneAsync(p => p.ProductId == productId, update);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating product stock", exception);
        }
    }
}