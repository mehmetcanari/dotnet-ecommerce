using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(MongoDbContext context)
    {
        _products = context.GetCollection<Product>("products");
        CreateIndexes();
    }

    private void CreateIndexes()
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

    public async Task<List<Product>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new FindOptions<Product>
            {
                Skip = (pageNumber - 1) * pageSize,
                Limit = pageSize,
                Sort = Builders<Product>.Sort.Descending(p => p.CreatedOn)
            };
            
            var cursor = await _products.FindAsync(_ => true, options, cancellationToken);
            return await cursor.ToListAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<Product> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var cursor = await _products.FindAsync(p => p.Id == id, cancellationToken: cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<bool> CheckExistsWithName(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = await _products.CountDocumentsAsync(p => p.Name == name, cancellationToken: cancellationToken);
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
            product.UpdatedOn = DateTime.UtcNow;
            await _products.ReplaceOneAsync(p => p.Id == product.Id, product, new ReplaceOptions(), cancellationToken);
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
            await _products.DeleteOneAsync(p => p.Id == product.Id, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task UpdateStock(Guid productId, int newValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var update = Builders<Product>.Update
                .Set(p => p.StockQuantity, newValue)
                .Set(p => p.UpdatedOn, DateTime.UtcNow);

            await _products.UpdateOneAsync(p => p.Id == productId, update, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}