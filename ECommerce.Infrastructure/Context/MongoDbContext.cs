using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using ECommerce.Domain.Model;

namespace ECommerce.Infrastructure.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration?.GetConnectionString("MongoDB") 
            ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? "mongodb://localhost:27017";
        
        var databaseName = configuration?["MongoDB:DatabaseName"] 
            ?? Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME") 
            ?? "ECommerceStore";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public virtual IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public async Task<int> GetNextSequenceValue(string sequenceName)
    {
        var counters = GetCollection<Counter>("counters");
        
        var filter = Builders<Counter>.Filter.Eq(c => c.Id, sequenceName);
        var update = Builders<Counter>.Update.Inc(c => c.SequenceValue, 1);
        var options = new FindOneAndUpdateOptions<Counter>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var result = await counters.FindOneAndUpdateAsync(filter, update, options);
        return result.SequenceValue;
    }
} 