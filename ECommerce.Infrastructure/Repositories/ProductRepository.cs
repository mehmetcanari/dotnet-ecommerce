using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
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

    protected virtual IFindFluent<Product, Product> GetProductQuery(FilterDefinition<Product> filter)
    {
        return _products.Find(filter);
    }

    public async Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new FindOptions<Product>
            {
                Skip = (pageNumber - 1) * pageSize,
                Limit = pageSize,
                Sort = Builders<Product>.Sort.Descending(p => p.ProductCreated)
            };
            
            var cursor = await _products.FindAsync(_ => true, options);
            return await cursor.ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<Product> GetProductById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cursor = await _products.FindAsync(p => p.ProductId == id);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<bool> CheckProductExistsWithName(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _products.CountDocumentsAsync(p => p.Name == name);
            return count > 0;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Create(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            if (product.ProductId == 0)
            {
                product.ProductId = await _context.GetNextSequenceValue("product_id");
            }
            
            await _products.InsertOneAsync(product, new InsertOneOptions(), cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Update(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            product.ProductUpdated = DateTime.UtcNow;
            await _products.ReplaceOneAsync(p => p.ProductId == product.ProductId, product, new ReplaceOptions(), cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Delete(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            await _products.DeleteOneAsync(p => p.ProductId == product.ProductId, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task DeleteById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _products.DeleteOneAsync(p => p.ProductId == id, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<List<Product>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new FindOptions<Product>
            {
                Skip = (pageNumber - 1) * pageSize,
                Limit = pageSize,
                Sort = Builders<Product>.Sort.Descending(p => p.ProductCreated)
            };

            var cursor = await _products.FindAsync(p => p.CategoryId == categoryId, options);
            return await cursor.ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task UpdateStock(int productId, int newStock, CancellationToken cancellationToken = default)
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
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}