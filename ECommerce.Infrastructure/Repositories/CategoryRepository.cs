using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly IMongoCollection<Category> _categories;

    public CategoryRepository(MongoDbContext context)
    {
        _categories = context.GetCollection<Category>("categories");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var nameIndexKeys = Builders<Category>.IndexKeys.Ascending(c => c.Name);
        _categories.Indexes.CreateOneAsync(new CreateIndexModel<Category>(nameIndexKeys));
    }

    public async Task Create(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            await _categories.InsertOneAsync(category, new InsertOneOptions(), cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<List<Category>> Read(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new FindOptions<Category>
            {
                Skip = (page - 1) * pageSize,
                Limit = pageSize,
                Sort = Builders<Category>.Sort.Ascending(c => c.Name)
            };

            var cursor = await _categories.FindAsync(_ => true, options, cancellationToken);
            return await cursor.ToListAsync(cancellationToken);
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
            var count = await _categories.CountDocumentsAsync(c => c.Name == name, cancellationToken: cancellationToken);
            return count > 0;
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
            var cursor = await _categories.FindAsync(c => c.Id == id, cancellationToken: cancellationToken);
            return await cursor.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Update(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            category.UpdatedOn = DateTime.UtcNow;
            await _categories.ReplaceOneAsync(c => c.Id == category.Id, category, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Delete(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            await _categories.DeleteOneAsync(c => c.Id == category.Id, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}