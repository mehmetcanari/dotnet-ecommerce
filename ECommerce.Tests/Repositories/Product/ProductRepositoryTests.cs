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
    private readonly Mock<MongoDbContext> _mockContext;
    private readonly IProductRepository _repository;

    public ProductRepositoryTests()
    {
        _mockCollection = new Mock<IMongoCollection<Domain.Model.Product>>();
        
        var mockIndexManager = new Mock<IMongoIndexManager<Domain.Model.Product>>();
        mockIndexManager.Setup(x => x.CreateOneAsync(It.IsAny<CreateIndexModel<Domain.Model.Product>>(), It.IsAny<CreateOneIndexOptions>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.FromResult("test-index"));
        
        _mockCollection.Setup(x => x.Indexes).Returns(mockIndexManager.Object);
        
        _mockContext = new Mock<MongoDbContext>(CreateTestConfiguration());
        _mockContext.Setup(c => c.GetCollection<Domain.Model.Product>("products")).Returns(_mockCollection.Object);

        _repository = new ProductRepository(_mockContext.Object);
    }

    private static IConfiguration CreateTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            ["ConnectionStrings:MongoDB"] = "mongodb://localhost:27017",
            ["MongoDB:DatabaseName"] = "TestECommerceStore"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();
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
        _mockContext.Setup(c => c.GetNextSequenceValue(It.IsAny<string>())).ReturnsAsync(1);

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
        var expectedProducts = products.Take(2).ToList();

        var mockCursor = new Mock<IAsyncCursor<Domain.Model.Product>>();
        mockCursor.Setup(_ => _.Current).Returns(expectedProducts);
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));

        _mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), It.IsAny<FindOptions<Domain.Model.Product, Domain.Model.Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

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
        mockCursor.Setup(_ => _.Current).Returns(new List<Domain.Model.Product> { product });
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true)).Returns(Task.FromResult(false));

        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), It.IsAny<FindOptions<Domain.Model.Product, Domain.Model.Product>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _repository.GetProductById(product.ProductId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Price.Should().Be(product.Price);
        result.StockQuantity.Should().Be(product.StockQuantity);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetProductById_Should_Return_Null_When_Product_Not_Exists()
    {
        var emptyProductList = new List<Domain.Model.Product>();
        var mockCursor = new Mock<IAsyncCursor<Domain.Model.Product>>();
        mockCursor.Setup(_ => _.Current).Returns(emptyProductList);
        mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
        mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

        _mockCollection.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), It.IsAny<FindOptions<Domain.Model.Product, Domain.Model.Product>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(mockCursor.Object);

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
        _mockCollection.Setup(x => x.CountDocumentsAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), null, default))
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
        _mockCollection.Setup(x => x.CountDocumentsAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), null, default))
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
        var updateResult = new ReplaceOneResult.Acknowledged(1, 1, null);

        _mockCollection.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), product, It.IsAny<ReplaceOptions>(), default))
            .ReturnsAsync(updateResult);

        // Act
        await _repository.Update(product);

        // Assert
        _mockCollection.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), 
            It.Is<Domain.Model.Product>(p => p.ProductId == product.ProductId), 
            It.IsAny<ReplaceOptions>(), 
            default), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_Product()
    {
        // Arrange
        var product = CreateProduct();
        var deleteResult = new DeleteResult.Acknowledged(1);

        _mockCollection.Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), default))
            .ReturnsAsync(deleteResult);

        // Act
        await _repository.Delete(product);

        // Assert
        _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Domain.Model.Product>>(), default), Times.Once);
    }
} 