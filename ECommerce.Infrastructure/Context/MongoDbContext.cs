using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "mongodb://localhost:27017";
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")  ?? "ecommerce-products";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public virtual IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
} 