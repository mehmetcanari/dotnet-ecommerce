using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Services.Category;
using ECommerce.Domain.Abstract.Repository;
using FluentAssertions;

namespace ECommerce.Tests.Services.Category;

[Trait("Category", "Category")]
[Trait("Category", "Service")]
public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    private const string CategoryCacheKey = "category:{0}";
    private const string GetAllCategoriesCacheKey = "categories";

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    private CategoryService CreateService() => new CategoryService(
        _serviceProviderMock.Object,
        _categoryRepositoryMock.Object,
        _loggerMock.Object,
        _cacheServiceMock.Object,
        _unitOfWorkMock.Object);

    private void SetupCategoryRead(Domain.Model.Category category = null)
    {
        var categories = category != null ? new List<Domain.Model.Category> { category } : new List<Domain.Model.Category>();
        _categoryRepositoryMock.Setup(r => r.Read(1, 50)).ReturnsAsync(categories);
    }

    private void SetupCategoryById(Domain.Model.Category category)
    {
        _categoryRepositoryMock.Setup(r => r.GetCategoryById(It.IsAny<int>()))
            .ReturnsAsync(category);
        if (category != null)
        {
            _categoryRepositoryMock.Setup(r => r.GetCategoryById(category.CategoryId))
                .ReturnsAsync(category);
        }
    }

    private void SetupCacheGet<T>(T value)
    {
        _cacheServiceMock.Setup(c => c.GetAsync<T>(It.IsAny<string>()))
            .ReturnsAsync(value);
    }

    private void SetupCacheSet()
    {
        _cacheServiceMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupCacheRemove()
    {
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private Domain.Model.Category CreateCategory(int id = 1, string name = "Test Category")
        => new Domain.Model.Category
        {
            CategoryId = id,
            Name = name,
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private CategoryResponseDto CreateCategoryResponse(int id = 1, string name = "Test Category")
        => new CategoryResponseDto
        {
            CategoryId = id,
            Name = name,
            Description = "Test Description",
            Products = new List<ProductResponseDto>()
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateCategoryAsync_Should_Create_Category_Successfully()
    {
        // Arrange
        var request = new CreateCategoryRequestDto { Name = "New Category", Description = "New Description" };
        SetupCategoryRead();
        SetupCacheRemove();
        _categoryRepositoryMock.Setup(r => r.CheckCategoryExistsWithName(request.Name)).ReturnsAsync(false);
        var service = CreateService();

        // Act
        var result = await service.CreateCategoryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _categoryRepositoryMock.Verify(r => r.Create(It.Is<Domain.Model.Category>(c => 
            c.Name == request.Name && 
            c.Description == request.Description)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("Category created successfully"), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateCategoryAsync_Should_Return_Failure_When_Category_Exists()
    {
        // Arrange
        var existingCategory = CreateCategory();
        var request = new CreateCategoryRequestDto { Name = existingCategory.Name, Description = "New Description" };
        _categoryRepositoryMock.Setup(r => r.CheckCategoryExistsWithName(existingCategory.Name)).ReturnsAsync(true);
        var service = CreateService();

        // Act
        var result = await service.CreateCategoryAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category already exists", result.Error);
        _categoryRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteCategoryAsync_Should_Delete_Category_Successfully()
    {
        // Arrange
        var category = CreateCategory();
        category.Products = new List<ECommerce.Domain.Model.Product>();
        SetupCategoryById(category);
        SetupCacheRemove();
        _categoryRepositoryMock.Setup(r => r.Read(1, 50)).ReturnsAsync(new List<ECommerce.Domain.Model.Category> { category });
        var service = CreateService();

        // Act
        var result = await service.DeleteCategoryAsync(category.CategoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        _categoryRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteCategoryAsync_Should_Return_Failure_When_Category_Not_Found()
    {
        // Arrange
        SetupCategoryById(null);
        var service = CreateService();

        // Act
        var result = await service.DeleteCategoryAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateCategoryAsync_Should_Update_Category_Successfully()
    {
        // Arrange
        var category = CreateCategory();
        category.Products = new List<ECommerce.Domain.Model.Product>();
        var request = new UpdateCategoryRequestDto { Name = "Updated Name", Description = "Updated Description" };
        SetupCategoryById(category);
        SetupCacheRemove();
        _categoryRepositoryMock.Setup(r => r.Read(1, 50)).ReturnsAsync(new List<ECommerce.Domain.Model.Category> { category });
        var service = CreateService();

        // Act
        var result = await service.UpdateCategoryAsync(category.CategoryId, request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.Category>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateCategoryAsync_Should_Return_Failure_When_Category_Not_Found()
    {
        // Arrange
        var request = new UpdateCategoryRequestDto { Name = "Updated Name", Description = "Updated Description" };
        SetupCategoryById(null);
        var service = CreateService();

        // Act
        var result = await service.UpdateCategoryAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryByIdAsync_Should_Return_Category_From_Cache()
    {
        // Arrange
        var categoryId = 1;
        var cachedCategory = CreateCategoryResponse(categoryId, "Cached Category");
        SetupCacheGet(cachedCategory);
        var service = CreateService();

        // Act
        var result = await service.GetCategoryByIdAsync(categoryId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(cachedCategory.CategoryId, result.Data.CategoryId);
        Assert.Equal(cachedCategory.Name, result.Data.Name);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryByIdAsync_Should_Return_Category_From_Repository_When_Not_In_Cache()
    {
        // Arrange
        var category = CreateCategory();
        var categoryId = category.CategoryId;
        SetupCacheGet<CategoryResponseDto>(null);
        SetupCategoryById(category);
        SetupCacheSet();
        var service = CreateService();

        // Act
        var result = await service.GetCategoryByIdAsync(categoryId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(category.CategoryId, result.Data.CategoryId);
        Assert.Equal(category.Name, result.Data.Name);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<CategoryResponseDto>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryByIdAsync_Should_Return_Failure_When_Category_Not_Found()
    {
        // Arrange
        SetupCategoryById(null);
        var service = CreateService();

        // Act
        var result = await service.GetCategoryByIdAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
    }

    [Fact]
    [Trait("Operation", "Cache")]
    public async Task CategoryCacheInvalidateAsync_Should_Clear_All_Category_Caches()
    {
        // Arrange
        var categories = new List<Domain.Model.Category> { CreateCategory(1), CreateCategory(2) };
        _categoryRepositoryMock.Setup(r => r.Read(1, 50))
            .ReturnsAsync(categories);
        var service = CreateService();

        // Act
        await service.CategoryCacheInvalidateAsync();

        // Assert
        _cacheServiceMock.Verify(c => c.RemoveAsync("category:1"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("category:2"), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Cache")]
    public async Task CategoryCacheInvalidateAsync_Should_Handle_Empty_Categories()
    {
        // Arrange
        SetupCategoryRead();
        var service = CreateService();

        // Act
        await service.CategoryCacheInvalidateAsync();

        // Assert
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error invalidating category cache"), Times.Never);
    }
}
