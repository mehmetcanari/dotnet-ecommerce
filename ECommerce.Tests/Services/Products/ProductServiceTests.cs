using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.Services.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;

namespace ECommerce.Tests.Services.Product;

[Trait("Category", "Product")]
[Trait("Category", "Service")]
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<IBasketItemService> _basketItemServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly IProductService _sut;

    private const string AllProductsCacheKey = "products";
    private const string ProductCacheKey = "product:{0}";

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _basketItemServiceMock = new Mock<IBasketItemService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILoggingService>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _sut = new ProductService(
            _productRepositoryMock.Object,
            _categoryServiceMock.Object,
            _basketItemServiceMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _unitOfWorkMock.Object,
            _categoryRepositoryMock.Object,
            _serviceProviderMock.Object
        );
    }

    private void SetupProductRead(List<Domain.Model.Product> products)
    {
        _productRepositoryMock.Setup(x => x.Read(1, 50))
            .ReturnsAsync(products);
    }

    private void SetupProductById(Domain.Model.Product product)
    {
        _productRepositoryMock.Setup(x => x.GetProductById(It.IsAny<int>()))
            .ReturnsAsync(product);
    }

    private void SetupCategoryById(CategoryResponseDto category)
    {
        _categoryServiceMock.Setup(x => x.GetCategoryByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Result<CategoryResponseDto>.Success(category));
    }

    private void SetupCacheGet<T>(T value)
    {
        _cacheServiceMock.Setup(x => x.GetAsync<T>(It.IsAny<string>()))
            .ReturnsAsync(value);
    }

    private void SetupCacheSet()
    {
        _cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupCacheRemove()
    {
        _cacheServiceMock.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private ProductService CreateService() => new ProductService(
        _productRepositoryMock.Object,
        _categoryServiceMock.Object,
        _basketItemServiceMock.Object,
        _loggerMock.Object,
        _cacheServiceMock.Object,
        _unitOfWorkMock.Object,
        _categoryRepositoryMock.Object,
        _serviceProviderMock.Object);

    private ProductCreateRequestDto CreateProductRequest()
        => new ProductCreateRequestDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

    private ProductUpdateRequestDto CreateProductUpdateRequest()
        => new ProductUpdateRequestDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 150.00m,
            DiscountRate = 0.20m,
            ImageUrl = "http://example.com/updated-image.jpg",
            StockQuantity = 25,
            CategoryId = 2
        };

    private Domain.Model.Product CreateProduct(int id = 1)
        => new Domain.Model.Product
        {
            ProductId = id,
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

    private ProductResponseDto CreateProductResponse(int id = 1, string name = "Test Product")
        => new ProductResponseDto
        {
            ProductName = name,
            Description = "Test Description",
            Price = 100.00m,
            DiscountRate = 0.10m,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateProductAsync_Should_Return_Failure_When_Product_Exists()
    {
        // Arrange
        var request = CreateProductRequest();
        _productRepositoryMock.Setup(r => r.CheckProductExistsWithName(It.IsAny<string>()))
            .ReturnsAsync(true);
        var service = CreateService();

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product with this name already exists", result.Error);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateProductAsync_Should_Create_Product_When_Not_Exists()
    {
        // Arrange
        var request = CreateProductRequest();
        _productRepositoryMock.Setup(r => r.CheckProductExistsWithName(request.Name)).ReturnsAsync(false);
        _categoryRepositoryMock.Setup(r => r.GetCategoryById(request.CategoryId)).ReturnsAsync(new Domain.Model.Category { CategoryId = request.CategoryId, Name = "Test Category", Description = "Test Description" });
        var service = CreateService();

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _productRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateProductAsync_Should_Update_Product_When_Exists()
    {
        // Arrange
        var product = CreateProduct();
        var request = CreateProductUpdateRequest();
        _productRepositoryMock.Setup(r => r.GetProductById(product.ProductId)).ReturnsAsync(product);
        _categoryRepositoryMock.Setup(r => r.GetCategoryById(request.CategoryId)).ReturnsAsync(new Domain.Model.Category { CategoryId = request.CategoryId, Name = "Test Category", Description = "Test Description" });
        var service = CreateService();

        // Act
        var result = await service.UpdateProductAsync(product.ProductId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Model.Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateProductAsync_Should_Return_Failure_When_Product_Not_Found()
    {
        // Arrange
        var request = CreateProductUpdateRequest();
        _productRepositoryMock.Setup(r => r.GetProductById(1)).ReturnsAsync((Domain.Model.Product)null);
        var service = CreateService();

        // Act
        var result = await service.UpdateProductAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
        _productRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Domain.Model.Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllProductsAsync_Should_Return_Products_From_Cache()
    {
        // Arrange
        var cachedProducts = new List<ProductResponseDto> { CreateProductResponse() };
        SetupCacheGet(cachedProducts);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Data.Should().BeEquivalentTo(cachedProducts);
        _productRepositoryMock.Verify(x => x.Read(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetProductWithIdAsync_Should_Return_Product_From_Cache()
    {
        // Arrange
        var cachedProduct = CreateProductResponse();
        SetupCacheGet(cachedProduct);

        // Act
        var result = await _sut.GetProductWithIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Data.Should().BeEquivalentTo(cachedProduct);
        _productRepositoryMock.Verify(x => x.GetProductById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllProductsAsync_Should_Return_Products_From_Database_When_Cache_Empty()
    {
        // Arrange
        var products = new List<Domain.Model.Product> { CreateProduct() };
        SetupCacheGet<List<ProductResponseDto>>(null);
        SetupProductRead(products);
        SetupCacheSet();

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Data.Should().HaveCount(1);
        result.Data[0].Should().BeEquivalentTo(new ProductResponseDto
        {
            ProductName = products[0].Name,
            Description = products[0].Description,
            Price = products[0].Price,
            DiscountRate = products[0].DiscountRate,
            ImageUrl = products[0].ImageUrl,
            StockQuantity = products[0].StockQuantity,
            CategoryId = products[0].CategoryId
        });
        _cacheServiceMock.Verify(x => x.SetAsync(
            AllProductsCacheKey,
            It.IsAny<List<ProductResponseDto>>(),
            It.IsAny<TimeSpan>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllProductsAsync_Should_Return_Failure_When_No_Products()
    {
        // Arrange
        SetupCacheGet<List<ProductResponseDto>>(null);
        SetupProductRead(new List<Domain.Model.Product>());

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No products found");
        result.Data.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteProductAsync_Should_Delete_Product_When_Exists()
    {
        // Arrange
        var product = CreateProduct();
        SetupProductById(product);
        SetupCacheRemove();

        // Act
        var result = await _sut.DeleteProductAsync(product.ProductId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        _productRepositoryMock.Verify(x => x.Delete(It.Is<Domain.Model.Product>(p => 
            p.ProductId == product.ProductId)), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveAsync(AllProductsCacheKey), Times.Once);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        _loggerMock.Verify(x => x.LogInformation(
            It.Is<string>(s => s.Contains("Product deleted successfully")),
            It.IsAny<object[]>()
        ), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteProductAsync_Should_Return_Failure_When_Product_Not_Found()
    {
        // Arrange
        SetupProductById(null);

        // Act
        var result = await _sut.DeleteProductAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
        _productRepositoryMock.Verify(x => x.Delete(It.IsAny<Domain.Model.Product>()), Times.Never);
        _cacheServiceMock.Verify(x => x.RemoveAsync(AllProductsCacheKey), Times.Never);
        _categoryServiceMock.Verify(x => x.CategoryCacheInvalidateAsync(), Times.Never);
        _unitOfWorkMock.Verify(x => x.Commit(), Times.Never);
    }
}
