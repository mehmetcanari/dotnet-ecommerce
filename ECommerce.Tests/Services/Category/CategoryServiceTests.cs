using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Interfaces.Service;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services.Category;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    private CategoryService CreateService() => new CategoryService(
        _categoryRepositoryMock.Object,
        _loggerMock.Object,
        _cacheServiceMock.Object,
        _unitOfWorkMock.Object);

    private Domain.Model.Category CreateCategory(int id = 1, string name = "Test Category")
        => new Domain.Model.Category
        {
            CategoryId = id,
            Name = name,
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    [Fact]
    public async Task CreateCategoryAsync_Should_Create_Category_Successfully()
    {
        var request = new CreateCategoryRequestDto { Name = "New Category", Description = "New Description" };
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category>());
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.CreateCategoryAsync(request);

        _categoryRepositoryMock.Verify(r => r.Create(It.Is<Domain.Model.Category>(c => 
            c.Name == request.Name && 
            c.Description == request.Description)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("Category created successfully"), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_Should_ThrowException_When_Category_Exists()
    {
        var existingCategory = CreateCategory();
        var request = new CreateCategoryRequestDto { Name = existingCategory.Name, Description = "New Description" };
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category> { existingCategory });
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.CreateCategoryAsync(request));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error creating category"), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_Should_Delete_Category_Successfully()
    {
        var category = CreateCategory();
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category> { category });
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.DeleteCategoryAsync(category.CategoryId);

        _categoryRepositoryMock.Verify(r => r.Delete(It.Is<Domain.Model.Category>(c => c.CategoryId == category.CategoryId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("Category deleted successfully"), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_Should_ThrowException_When_Category_Not_Found()
    {
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.DeleteCategoryAsync(1));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error deleting category"), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_Should_Update_Category_Successfully()
    {
        var category = CreateCategory();
        var request = new UpdateCategoryRequestDto { Name = "Updated Name", Description = "Updated Description" };
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category> { category });
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.UpdateCategoryAsync(category.CategoryId, request);

        _categoryRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Category>(c => 
            c.CategoryId == category.CategoryId && 
            c.Name == request.Name && 
            c.Description == request.Description)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("Category updated successfully"), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_Should_ThrowException_When_Category_Not_Found()
    {
        var request = new UpdateCategoryRequestDto { Name = "Updated Name", Description = "Updated Description" };
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UpdateCategoryAsync(1, request));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error updating category"), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_Should_Return_Category_From_Cache()
    {
        var categoryId = 1;
        var cachedCategory = new CategoryResponseDto
        {
            CategoryId = categoryId,
            Name = "Cached Category",
            Description = "Cached Description",
            Products = new List<ProductResponseDto>()
        };
        _cacheServiceMock.Setup(c => c.GetAsync<CategoryResponseDto>(It.IsAny<string>()))
            .ReturnsAsync(cachedCategory);
        var service = CreateService();

        var result = await service.GetCategoryByIdAsync(categoryId);

        Assert.Equal(cachedCategory.CategoryId, result.CategoryId);
        Assert.Equal(cachedCategory.Name, result.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_Should_Return_Category_From_Repository_When_Not_In_Cache()
    {
        var category = CreateCategory();
        var categoryId = category.CategoryId;
        _cacheServiceMock.Setup(c => c.GetAsync<CategoryResponseDto>(It.IsAny<string>()))
            .ReturnsAsync((CategoryResponseDto)null);
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category> { category });
        _cacheServiceMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<CategoryResponseDto>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        var result = await service.GetCategoryByIdAsync(categoryId);

        Assert.Equal(category.CategoryId, result.CategoryId);
        Assert.Equal(category.Name, result.Name);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<CategoryResponseDto>(), It.IsAny<TimeSpan>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("Category retrieved successfully"), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_Should_ThrowException_When_Category_Not_Found()
    {
        var categoryId = 1;
        _cacheServiceMock.Setup(c => c.GetAsync<CategoryResponseDto>(It.IsAny<string>()))
            .ReturnsAsync((CategoryResponseDto)null);
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetCategoryByIdAsync(categoryId));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error getting category by id"), Times.Once);
    }

    [Fact]
    public async Task CategoryCacheInvalidateAsync_Should_Clear_All_Category_Caches()
    {
        var categories = new List<Domain.Model.Category> { CreateCategory(1), CreateCategory(2) };
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(categories);
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.CategoryCacheInvalidateAsync();

        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Exactly(categories.Count));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error invalidating category cache"), Times.Never);
    }

    [Fact]
    public async Task CategoryCacheInvalidateAsync_Should_Handle_Empty_Categories()
    {
        _categoryRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Category>());
        var service = CreateService();

        await service.CategoryCacheInvalidateAsync();

        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error invalidating category cache"), Times.Never);
    }
}
