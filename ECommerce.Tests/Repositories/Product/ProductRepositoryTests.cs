using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Tests.Repositories.Product;

[Trait("Category", "Product")]
[Trait("Category", "Repository")]
public class ProductRepositoryTests
{
    private readonly StoreDbContext _context;
    private readonly IProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StoreDbContext(options);
        _repository = new ProductRepository(_context);
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
            ImageUrl = "test.jpg"
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task Create_Should_Add_Product_To_Database()
    {
        // Arrange
        var product = CreateProduct();

        // Act
        await _repository.Create(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Products.FindAsync(product.ProductId);
        result.Should().NotBeNull();
        result.Name.Should().Be(product.Name);
        result.Description.Should().Be(product.Description);
        result.Price.Should().Be(product.Price);
        result.StockQuantity.Should().Be(product.StockQuantity);
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

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

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
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

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
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckProductExistsWithName(product.Name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Operation", "CheckExists")]
    public async Task CheckProductExistsWithName_Should_Return_False_When_Product_Not_Exists()
    {
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
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        product.Name = "Updated Name";
        product.Description = "Updated Description";
        product.Price = 200;
        product.StockQuantity = 20;
        _repository.Update(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Products.FindAsync(product.ProductId);
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Price.Should().Be(200);
        result.StockQuantity.Should().Be(20);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_Product()
    {
        // Arrange
        var product = CreateProduct();
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(product);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Products.FindAsync(product.ProductId);
        result.Should().BeNull();
    }

    private void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 