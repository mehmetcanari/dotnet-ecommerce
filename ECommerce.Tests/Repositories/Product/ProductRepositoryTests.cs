using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace ECommerce.Tests.Repositories.Product;

[Trait("Category", "Product")]
[Trait("Category", "Repository")]
public class ProductRepositoryTests
{
    private readonly Mock<IMongoCollection<Domain.Model.Product>> _mockCollection;
    private readonly TestMongoDbContext _mockContext;
    private readonly IProductRepository _repository;
    private readonly List<Domain.Model.Product> _testProducts;

    public ProductRepositoryTests()
    {
        _testProducts = new List<Domain.Model.Product>();
        
        // Mock MongoDB collection
        _mockCollection = new Mock<IMongoCollection<Domain.Model.Product>>();
        
        // Setup the indexes mock for the product collection
        var mockIndexManager = new Mock<IMongoIndexManager<Domain.Model.Product>>();
        mockIndexManager.Setup(x => x.CreateOneAsync(It.IsAny<CreateIndexModel<Domain.Model.Product>>(), It.IsAny<CreateOneIndexOptions>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult("test-index"));
        
        _mockCollection.Setup(x => x.Indexes).Returns(mockIndexManager.Object);
        
        // Use a test implementation to avoid configuration issues
        _mockContext = new TestMongoDbContext(_mockCollection.Object);

        _repository = new ProductRepository(_mockContext);
    }

    // Test implementation of MongoDbContext
    private class TestMongoDbContext : MongoDbContext
    {
        private readonly IMongoCollection<Domain.Model.Product> _productCollection;

        public TestMongoDbContext(IMongoCollection<Domain.Model.Product> productCollection) 
            : base(CreateTestConfiguration())
        {
            _productCollection = productCollection;
        }

        public override IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            if (typeof(T) == typeof(Domain.Model.Product) && collectionName == "products")
            {
                return (IMongoCollection<T>)_productCollection;
            }
            
            // Return a mock collection for other types to avoid null reference exceptions
            var mockCollection = new Mock<IMongoCollection<T>>();
            var mockIndexManagerGeneric = new Mock<IMongoIndexManager<T>>();
            mockIndexManagerGeneric.Setup(x => x.CreateOneAsync(It.IsAny<CreateIndexModel<T>>(), It.IsAny<CreateOneIndexOptions>(), It.IsAny<CancellationToken>()))
                          .Returns(Task.FromResult("test-index"));
            mockCollection.Setup(x => x.Indexes).Returns(mockIndexManagerGeneric.Object);
            return mockCollection.Object;
        }

        private static IConfiguration CreateTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
                ["MongoDB:DatabaseName"] = "TestECommerceStore"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }
    }

    private Domain.Model.Product CreateProduct(int id = 1, string name = "Test Product")
        => new Domain.Model.Product
        {
            ProductId = id,
            Name = name,
            Description = "Test Description",
            Price = 100,
            StockQuantity = 10,
            CategoryId = 1,
            DiscountRate = 0,
            ImageUrl = "test.jpg",
            ProductCreated = DateTime.UtcNow,
            ProductUpdated = DateTime.UtcNow
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task Create_Should_Add_Product_To_Database()
    {
        // Arrange
        var product = CreateProduct();
        _mockCollection.Setup(x => x.InsertOneAsync(It.IsAny<Domain.Model.Product>(), null, default))
                      .Returns(Task.CompletedTask);

        // Act
        await _repository.Create(product);

        // Assert
        _mockCollection.Verify(x => x.InsertOneAsync(It.Is<Domain.Model.Product>(p => 
            p.Name == product.Name && 
            p.Description == product.Description && 
            p.Price == product.Price && 
            p.StockQuantity == product.StockQuantity), null, default), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Read")]
    public async Task Read_Should_Return_Products_With_Pagination()
    {
        // Arrange
        var products = new List<Domain.Model.Product>
        {
            CreateProduct(1, "Product 1"),
            CreateProduct(2, "Product 2"),
            CreateProduct(3, "Product 3")
        };

        var mockCursor = new Mock<IAsyncCursor<Domain.Model.Product>>();
        mockCursor.Setup(x => x.Current).Returns(products.Take(2));
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.FromResult(true))
                  .Returns(Task.FromResult(false));

        var mockOrderedFluent = new Mock<IOrderedFindFluent<Domain.Model.Product, Domain.Model.Product>>();
        mockOrderedFluent.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(products.Take(2).ToList());

        var mockFluent = new Mock<IFindFluent<Domain.Model.Product, Domain.Model.Product>>();
        mockFluent.Setup(x => x.Skip(It.IsAny<int?>())).Returns(mockFluent.Object);
        mockFluent.Setup(x => x.Limit(It.IsAny<int?>())).Returns(mockFluent.Object);
        mockFluent.Setup(x => x.SortByDescending(It.IsAny<Expression<Func<Domain.Model.Product, object>>>()))
                  .Returns(mockOrderedFluent.Object);

        _mockCollection.Setup(x => x.Find(It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(), null))
                      .Returns(mockFluent.Object);

        // Act
        var result = await _repository.Read(1, 2);

        // Assert
        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().Contain(new[] {"Product 1", "Product 2"});
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetProductById_Should_Return_Product_When_Exists()
    {
        // Arrange
        var product = CreateProduct();
        
        var mockCursor = new Mock<IAsyncCursor<Domain.Model.Product>>();
        mockCursor.Setup(x => x.Current).Returns(new[] { product });
        mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>()))
                  .Returns(true)
                  .Returns(false);
        mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.FromResult(true))
                  .Returns(Task.FromResult(false));

        var mockFluent = new Mock<IFindFluent<Domain.Model.Product, Domain.Model.Product>>();
        mockFluent.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(product);

        _mockCollection.Setup(x => x.Find(It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(), null))
                      .Returns(mockFluent.Object);

        // Act
        var result = await _repository.GetProductById(product.ProductId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Price.Should().Be(product.Price);
        result.StockQuantity.Should().Be(product.StockQuantity);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetProductById_Should_Return_Null_When_Product_Not_Exists()
    {
        // Arrange
        var mockFluent = new Mock<IFindFluent<Domain.Model.Product, Domain.Model.Product>>();
        mockFluent.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Domain.Model.Product)null);

        _mockCollection.Setup(x => x.Find(It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(), null))
                      .Returns(mockFluent.Object);

        // Act
        var result = await _repository.GetProductById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "CheckExists")]
    public async Task CheckProductExistsWithName_Should_Return_True_When_Product_Exists()
    {
        // Arrange
        var product = CreateProduct();
        _mockCollection.Setup(x => x.CountDocumentsAsync(It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(), null, default))
                      .ReturnsAsync(1);

        // Act
        var result = await _repository.CheckProductExistsWithName(product.Name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Operation", "CheckExists")]
    public async Task CheckProductExistsWithName_Should_Return_False_When_Product_Not_Exists()
    {
        // Arrange
        _mockCollection.Setup(x => x.CountDocumentsAsync(It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(), null, default))
                      .ReturnsAsync(0);

        // Act
        var result = await _repository.CheckProductExistsWithName("NonExistentProduct");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task Update_Should_Modify_Product()
    {
        // Arrange
        var product = CreateProduct();
        product.Name = "Updated Name";
        product.Description = "Updated Description";
        product.Price = 200;
        product.StockQuantity = 20;

        var mockResult = new Mock<ReplaceOneResult>();
        mockResult.Setup(x => x.IsAcknowledged).Returns(true);
        mockResult.Setup(x => x.ModifiedCount).Returns(1);

        _mockCollection.Setup(x => x.ReplaceOneAsync(
            It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(),
            It.IsAny<Domain.Model.Product>(),
            It.IsAny<ReplaceOptions>(),
            default))
                      .ReturnsAsync(mockResult.Object);

        // Act
        await _repository.Update(product);

        // Assert
        _mockCollection.Verify(x => x.ReplaceOneAsync(
            It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(),
            It.Is<Domain.Model.Product>(p => 
                p.Name == "Updated Name" && 
                p.Description == "Updated Description" && 
                p.Price == 200 && 
                p.StockQuantity == 20),
            It.IsAny<ReplaceOptions>(),
            default), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_Product()
    {
        // Arrange
        var product = CreateProduct();
        var mockResult = new Mock<DeleteResult>();
        mockResult.Setup(x => x.IsAcknowledged).Returns(true);
        mockResult.Setup(x => x.DeletedCount).Returns(1);

        _mockCollection.Setup(x => x.DeleteOneAsync(
            It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(),
            default))
                      .ReturnsAsync(mockResult.Object);

        // Act
        await _repository.Delete(product);

        // Assert
        _mockCollection.Verify(x => x.DeleteOneAsync(
            It.IsAny<Expression<Func<Domain.Model.Product, bool>>>(),
            default), Times.Once);
    }

    private void Dispose()
    {
        // MongoDB context doesn't need explicit disposal in tests
    }
} 