using MongoDB.Driver;

namespace ECommerce.Infrastructure.Context;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ?? "mongodb://localhost:27017";
        var databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")  ?? "ecommerce-products";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
} 