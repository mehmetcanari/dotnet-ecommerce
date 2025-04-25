using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Services.Product;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Tests.Services.Product;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<IBasketItemService> _basketItemServiceMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IProductService _sut;
    private const string AllProductsCacheKey = "products";
    private const string ProductCacheKey = "product:{0}";

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _basketItemServiceMock = new Mock<IBasketItemService>();
        _loggerMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new ProductService(
            _productRepositoryMock.Object,
            _categoryServiceMock.Object,
            _basketItemServiceMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateProduct_WhenProductExists_ThrowsException()
    {
        // Arrange
        var productCreateRequest = new ProductCreateRequestDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product>
            {
                new()
                {
                    Name = "Test Product",
                    Description = "Test Description",
                    Price = 100.00m,
                    DiscountRate = 0.10m,
                    ImageUrl = "http://example.com/image.jpg",
                    StockQuantity = 10,
                    CategoryId = 1
                }
            });

        // Act
        var act = () => _sut.CreateProductAsync(productCreateRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Product already exists in the database");

        _productRepositoryMock.Verify(x => x.Create(It.IsAny<Domain.Model.Product>()), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync("products"), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task CreateProduct_WhenProductDoesNotExist_CreatesProductAndLogsSuccess()
    {
        // Arrange
        var productCreateRequest = new ProductCreateRequestDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

        var category = new CategoryResponseDto
        {
            CategoryId = 1,
            Name = "Test Category",
            Description = "Test Description"
        };

        _categoryServiceMock.Setup(x => x.GetCategoryByIdAsync(productCreateRequest.CategoryId))
            .ReturnsAsync(category);

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product>());

        var discountedPrice = MathService.CalculateDiscount(productCreateRequest.Price, productCreateRequest.DiscountRate);

        // Act
        await _sut.CreateProductAsync(productCreateRequest);

        // Assert
        _productRepositoryMock.Verify(x => x.Create(It.Is<Domain.Model.Product>(p =>
            p.Name == productCreateRequest.Name &&
            p.Description == productCreateRequest.Description &&
            p.Price == discountedPrice &&
            p.DiscountRate == productCreateRequest.DiscountRate &&
            p.ImageUrl == productCreateRequest.ImageUrl &&
            p.StockQuantity == productCreateRequest.StockQuantity &&
            p.CategoryId == category.CategoryId
        )), Times.Once);

        _cacheServiceMock.Verify(x => x.RemoveAsync("products"), Times.Once);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);

        _loggerMock.Verify(x => x.LogInformation(
            It.Is<string>(s => s.Contains("Product created successfully")),
            It.IsAny<object[]>()
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductExists_UpdatesProductAndLogsSuccess()
    {
        // Arrange
        int productId = 1;
        var initialDateTime = DateTime.UtcNow.AddMinutes(-5); // 5 dakika öncesi
        var productUpdateRequest = new ProductUpdateRequestDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 150.00m,
            DiscountRate = 0.20m,
            ImageUrl = "http://example.com/updated-image.jpg",
            StockQuantity = 25,
            CategoryId = 2
        };

        var existingProduct = new Domain.Model.Product
        {
            ProductId = productId,
            Name = "Original Product",
            Description = "Original Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/original-image.jpg",
            StockQuantity = 10,
            CategoryId = 1,
            ProductCreated = initialDateTime,
            ProductUpdated = initialDateTime
        };

        var category = new CategoryResponseDto
        {
            CategoryId = 2,
            Name = "Updated Category",
            Description = "Updated Category Description"
        };

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product> { existingProduct });

        _categoryServiceMock.Setup(x => x.GetCategoryByIdAsync(productUpdateRequest.CategoryId))
            .ReturnsAsync(category);

        Domain.Model.Product capturedProduct = null;
        _productRepositoryMock.Setup(x => x.Update(It.IsAny<Domain.Model.Product>()))
            .Callback<Domain.Model.Product>(p => capturedProduct = p);

        // Act
        await _sut.UpdateProductAsync(productId, productUpdateRequest);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct.ProductId.Should().Be(productId);
        capturedProduct.Name.Should().Be(productUpdateRequest.Name);
        capturedProduct.Description.Should().Be(productUpdateRequest.Description);
        capturedProduct.Price.Should().Be(MathService.CalculateDiscount(productUpdateRequest.Price, productUpdateRequest.DiscountRate));
        capturedProduct.DiscountRate.Should().Be(productUpdateRequest.DiscountRate);
        capturedProduct.ImageUrl.Should().Be(productUpdateRequest.ImageUrl);
        capturedProduct.StockQuantity.Should().Be(productUpdateRequest.StockQuantity);
        capturedProduct.CategoryId.Should().Be(category.CategoryId);
        
        // Tarih kontrolünü güncelliyoruz
        capturedProduct.ProductUpdated.Should().BeAfter(initialDateTime);

        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Domain.Model.Product>()), Times.Once);
        
        _basketItemServiceMock.Verify(x => 
            x.ClearBasketItemsIncludeOrderedProductAsync(It.Is<Domain.Model.Product>(p => 
                p.ProductId == productId)), 
            Times.Once);

        _cacheServiceMock.Verify(x => x.RemoveAsync("products"), Times.Once);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);

        _loggerMock.Verify(x => x.LogInformation(
            It.Is<string>(s => s.Contains("Product updated successfully")),
            It.IsAny<object[]>()
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_WhenProductNotFound_ThrowsException()
    {
        // Arrange
        int nonExistentProductId = 999;
        var productUpdateRequest = new ProductUpdateRequestDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 150.00m,
            DiscountRate = 0.20m,
            ImageUrl = "http://example.com/updated-image.jpg",
            StockQuantity = 25,
            CategoryId = 2
        };

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product>());

        // Act
        var act = () => _sut.UpdateProductAsync(nonExistentProductId, productUpdateRequest);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Product not found");

        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Domain.Model.Product>()), Times.Never);
        _basketItemServiceMock.Verify(x => 
            x.ClearBasketItemsIncludeOrderedProductAsync(It.IsAny<Domain.Model.Product>()), 
            Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync("products"), Times.Never);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }

    [Fact]
    public async Task GetAllProducts_WhenCacheExists_ReturnsCachedProducts()
    {
        // Arrange
        var cachedProducts = new List<ProductResponseDto>
        {
            new()
            {
                ProductName = "Cached Product",
                Description = "Cached Description",
                Price = 100.00m,
                DiscountRate = 0.10m,
                ImageUrl = "http://example.com/image.jpg",
                StockQuantity = 10,
                CategoryId = 1
            }
        };

        _cacheServiceMock.Setup(x => x.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey))
            .ReturnsAsync(cachedProducts);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Should().BeEquivalentTo(cachedProducts);
        _productRepositoryMock.Verify(x => x.Read(), Times.Never);
    }

    [Fact]
    public async Task GetProductWithId_WhenCacheExists_ReturnsCachedProduct()
    {
        // Arrange
        var cachedProduct = new ProductResponseDto
        {
            ProductName = "Cached Product",
            Description = "Cached Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

        _cacheServiceMock.Setup(x => x.GetAsync<ProductResponseDto>(string.Format(ProductCacheKey, 1)))
            .ReturnsAsync(cachedProduct);

        // Act
        var result = await _sut.GetProductWithIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(cachedProduct);
        _productRepositoryMock.Verify(x => x.Read(), Times.Never);
    }
    
    [Fact]
    public async Task GetAllProducts_WhenCacheEmpty_ReturnsFromDatabaseAndCache()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey))
            .ReturnsAsync((List<ProductResponseDto>)null!);

        var testProducts = new List<Domain.Model.Product>
        {
            new()
            {
                ProductId = 1,
                Name = "Test Product 1",
                Description = "Test Description 1",
                Price = 100.00m,
                DiscountRate = 0.15m,
                ImageUrl = "http://example.com/image1.jpg",
                StockQuantity = 10,
                CategoryId = 1,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            },
            new()
            {
                ProductId = 2,
                Name = "Test Product 2",
                Description = "Test Description 2",
                Price = 200.00m,
                DiscountRate = 0.20m,
                ImageUrl = "http://example.com/image2.jpg",
                StockQuantity = 20,
                CategoryId = 1,
                ProductCreated = DateTime.UtcNow,
                ProductUpdated = DateTime.UtcNow
            }
        };

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(testProducts);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        // Verify product mapping
        result[0].Should().BeEquivalentTo(new ProductResponseDto
        {
            ProductName = "Test Product 1",
            Description = "Test Description 1",
            Price = 100.00m,
            DiscountRate = 0.15m,
            ImageUrl = "http://example.com/image1.jpg",
            StockQuantity = 10,
            CategoryId = 1
        });

        // Verify cache was set
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                AllProductsCacheKey,
                It.IsAny<List<ProductResponseDto>>(),
                It.IsAny<TimeSpan>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAllProducts_WhenNoProducts_ThrowsException()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey))
            .ReturnsAsync((List<ProductResponseDto>)null!);

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product>());

        // Act
        var act = () => _sut.GetAllProductsAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("No products found");
    }

    [Fact]
    public async Task GetAllProducts_WhenRepositoryThrowsException_LogsAndRethrowsException()
    {
        // Arrange
        _cacheServiceMock.Setup(x => x.GetAsync<List<ProductResponseDto>>(AllProductsCacheKey))
            .ReturnsAsync((List<ProductResponseDto>)null!);

        var expectedException = new Exception("Database error");
        _productRepositoryMock.Setup(x => x.Read())
            .ThrowsAsync(expectedException);

        // Act
        var act = () => _sut.GetAllProductsAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>();
        
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Unexpected error while fetching all products")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteProduct_WhenProductExists_DeletesProductAndLogsSuccess()
    {
        // Arrange
        int productId = 1;
        var existingProduct = new Domain.Model.Product
        {
            ProductId = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product> { existingProduct });

        // Act
        await _sut.DeleteProductAsync(productId);

        // Assert
        _productRepositoryMock.Verify(x => x.Delete(It.Is<Domain.Model.Product>(p => 
            p.ProductId == productId)), 
            Times.Once);

        _cacheServiceMock.Verify(x => x.RemoveAsync("products"), Times.Once);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);

        _loggerMock.Verify(x => x.LogInformation(
            It.Is<string>(s => s.Contains("Product deleted successfully")),
            It.IsAny<object[]>()
        ), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_WhenProductNotFound_ThrowsException()
    {
        // Arrange
        int nonExistentProductId = 999;
        _productRepositoryMock.Setup(x => x.Read())
            .ReturnsAsync(new List<Domain.Model.Product>());

        // Act
        var act = () => _sut.DeleteProductAsync(nonExistentProductId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Product not found");

        _productRepositoryMock.Verify(x => x.Delete(It.IsAny<Domain.Model.Product>()), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync("products"), Times.Never);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }
}
