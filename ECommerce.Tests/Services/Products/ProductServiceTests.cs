using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Product;
using ECommerce.Application.Services.Product;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Product;

namespace ECommerce.Tests.Services.Product;

[Trait("Category", "Product")]
[Trait("Category", "Service")]
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly IProductService _sut;

    private const string AllProductsCacheKey = "products";

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILoggingService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new ProductService(
            _productRepositoryMock.Object,
            _categoryServiceMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _unitOfWorkMock.Object,
            _serviceProviderMock.Object,
            _mediatorMock.Object
        );
    }

    private void SetupProductById(Domain.Model.Product product)
    {
        _productRepositoryMock.Setup(x => x.GetProductById(It.IsAny<int>()))
            .ReturnsAsync(product);
    }

    private void SetupCacheRemove()
    {
        _cacheServiceMock.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupCategoryCacheInvalidate()
    {
        _categoryServiceMock.Setup(x => x.CategoryCacheInvalidateAsync())
            .Returns(Task.CompletedTask);
    }

    private void SetupSetProductCache()
    {
        _productRepositoryMock.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Product>());
        
        _cacheServiceMock.Setup(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
    }

    private ProductService CreateService() => new ProductService(
        _productRepositoryMock.Object,
        _categoryServiceMock.Object,
        _loggerMock.Object,
        _cacheServiceMock.Object,
        _unitOfWorkMock.Object,
        _serviceProviderMock.Object,
        _mediatorMock.Object
    );

    private ProductCreateRequestDto CreateProductRequest()
        => new ProductCreateRequestDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 100.00,
            DiscountRate = 0.10,
            ImageUrl = "http://example.com/image.jpg",
            StockQuantity = 10,
            CategoryId = 1
        };

    private ProductUpdateRequestDto CreateProductUpdateRequest()
        => new ProductUpdateRequestDto
        {
            Name = "Updated Product",
            Description = "Updated Description",
            Price = 150.00,
            DiscountRate = 0.20,
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
            Price = 100,
            DiscountRate = 0.10,
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
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateProductCommand>(cmd => cmd.ProductCreateRequest == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Product with this name already exists"));

        var service = CreateService();

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product with this name already exists", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateProductCommand>(cmd => cmd.ProductCreateRequest == request),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateProductAsync_Should_Create_Product_When_Not_Exists()
    {
        // Arrange
        var request = CreateProductRequest();
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateProductCommand>(cmd => cmd.ProductCreateRequest == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        SetupCacheRemove();
        SetupCategoryCacheInvalidate();
        SetupSetProductCache();
        var service = CreateService();

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateProductCommand>(cmd => cmd.ProductCreateRequest == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateProductAsync_Should_Update_Product_When_Exists()
    {
        // Arrange
        var product = CreateProduct();
        var request = CreateProductUpdateRequest();
        SetupProductById(product);
        SetupCacheRemove();
        SetupCategoryCacheInvalidate();
        SetupSetProductCache();

        _mediatorMock.Setup(m => m.Send(
            It.Is<UpdateProductCommand>(cmd => cmd.Id == product.ProductId && cmd.ProductUpdateRequest == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var service = CreateService();

        // Act
        var result = await service.UpdateProductAsync(product.ProductId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateProductCommand>(cmd => cmd.Id == product.ProductId && cmd.ProductUpdateRequest == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateProductAsync_Should_Return_Failure_When_Product_Not_Found()
    {
        // Arrange
        var request = CreateProductUpdateRequest();
        _mediatorMock.Setup(m => m.Send(
            It.Is<UpdateProductCommand>(cmd => cmd.Id == 1 && cmd.ProductUpdateRequest == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Product not found"));

        var service = CreateService();

        // Act
        var result = await service.UpdateProductAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateProductCommand>(cmd => cmd.Id == 1 && cmd.ProductUpdateRequest == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
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
