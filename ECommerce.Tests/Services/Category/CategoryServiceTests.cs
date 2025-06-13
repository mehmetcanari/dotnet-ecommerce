using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Category;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.DTO.Response.Category;
using ECommerce.Application.DTO.Response.Product;
using ECommerce.Application.Services.Category;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Utility;
using FluentValidation;
using ECommerce.Application.Validations.Category;

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
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ICategoryService _sut;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _loggerMock = new Mock<ILoggingService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new CategoryService(
            _serviceProviderMock.Object,
            _categoryRepositoryMock.Object,
            _loggerMock.Object,
            _cacheServiceMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object);
    }

    private CreateCategoryRequestDto CreateCategoryRequest()
        => new CreateCategoryRequestDto
        {
            Name = "Test Category",
            Description = "Test Description"
        };

    private UpdateCategoryRequestDto CreateCategoryUpdateRequest()
        => new UpdateCategoryRequestDto
        {
            Name = "Updated Category",
            Description = "Updated Description"
        };

    private Domain.Model.Category CreateCategory(int id = 1)
        => new Domain.Model.Category
        {
            CategoryId = id,
            Name = "Test Category",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateCategoryAsync_Should_Create_Category_Successfully()
    {
        // Arrange
        var request = CreateCategoryRequest();
        var validator = new CategoryCreateValidation();
        _serviceProviderMock.Setup(x => x.GetService(typeof(IValidator<CreateCategoryRequestDto>)))
            .Returns(validator);
        _categoryRepositoryMock.Setup(r => r.Read(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Category>());
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateCategoryCommand>(cmd => cmd.CreateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _unitOfWorkMock.Setup(u => u.Commit())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateCategoryAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateCategoryCommand>(cmd => cmd.CreateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateCategoryAsync_Should_Return_Failure_When_Category_Exists()
    {
        // Arrange
        var request = CreateCategoryRequest();
        _mediatorMock.Setup(m => m.Send(
            It.Is<CreateCategoryCommand>(cmd => cmd.CreateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Category already exists"));

        // Act
        var result = await _sut.CreateCategoryAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category already exists", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateCategoryCommand>(cmd => cmd.CreateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteCategoryAsync_Should_Delete_Category_Successfully()
    {
        // Arrange
        var category = CreateCategory();
        _categoryRepositoryMock.Setup(r => r.Read(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Category>());
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == category.CategoryId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _unitOfWorkMock.Setup(u => u.Commit())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteCategoryAsync(category.CategoryId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == category.CategoryId),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteCategoryAsync_Should_Return_Failure_When_Category_Not_Found()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == 1),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Category not found"));

        // Act
        var result = await _sut.DeleteCategoryAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateCategoryAsync_Should_Update_Category_Successfully()
    {
        // Arrange
        var category = CreateCategory();
        var request = CreateCategoryUpdateRequest();
        var validator = new CategoryUpdateValidation();
        _serviceProviderMock.Setup(x => x.GetService(typeof(IValidator<UpdateCategoryRequestDto>)))
            .Returns(validator);
        _categoryRepositoryMock.Setup(r => r.Read(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Domain.Model.Category>());
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mediatorMock.Setup(m => m.Send(
            It.Is<UpdateCategoryCommand>(cmd => cmd.CategoryId == category.CategoryId && cmd.UpdateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        _unitOfWorkMock.Setup(u => u.Commit())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateCategoryAsync(category.CategoryId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCategoryCommand>(cmd => cmd.CategoryId == category.CategoryId && cmd.UpdateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateCategoryAsync_Should_Return_Failure_When_Category_Not_Found()
    {
        // Arrange
        var request = CreateCategoryUpdateRequest();
        _mediatorMock.Setup(m => m.Send(
            It.Is<UpdateCategoryCommand>(cmd => cmd.CategoryId == 1 && cmd.UpdateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Category not found"));

        // Act
        var result = await _sut.UpdateCategoryAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateCategoryCommand>(cmd => cmd.CategoryId == 1 && cmd.UpdateCategoryRequestDto == request),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryByIdAsync_Should_Return_Category_From_Cache()
    {
        // Arrange
        var categoryId = 1;
        var cachedCategory = new CategoryResponseDto
        {
            CategoryId = categoryId,
            Name = "Cached Category",
            Description = "Cached Description",
            Products = new List<ProductResponseDto>()
        };

        _mediatorMock.Setup(m => m.Send(
            It.Is<GetCategoryByIdQuery>(cmd => cmd.CategoryId == categoryId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CategoryResponseDto>.Success(cachedCategory));

        // Act
        var result = await _mediatorMock.Object.Send(new GetCategoryByIdQuery { CategoryId = categoryId });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(cachedCategory.CategoryId, result.Data.CategoryId);
        Assert.Equal(cachedCategory.Name, result.Data.Name);
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetCategoryByIdQuery>(cmd => cmd.CategoryId == categoryId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryByIdAsync_Should_Return_Failure_When_Category_Not_Found()
    {
        // Arrange
        var categoryId = 1;
        _mediatorMock.Setup(m => m.Send(
            It.Is<GetCategoryByIdQuery>(cmd => cmd.CategoryId == categoryId),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CategoryResponseDto>.Failure("Category not found"));

        // Act
        var result = await _mediatorMock.Object.Send(new GetCategoryByIdQuery { CategoryId = categoryId });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found", result.Error);
        _mediatorMock.Verify(m => m.Send(
            It.Is<GetCategoryByIdQuery>(cmd => cmd.CategoryId == categoryId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Cache")]
    public async Task CategoryCacheInvalidateAsync_Should_Clear_All_Category_Caches()
    {
        // Arrange
        var categories = new List<Domain.Model.Category> { CreateCategory(1), CreateCategory(2) };
        _categoryRepositoryMock.Setup(r => r.Read(1, 50))
            .ReturnsAsync(categories);

        // Act
        await _sut.CategoryCacheInvalidateAsync();

        // Assert
        _cacheServiceMock.Verify(c => c.RemoveAsync("category:1"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("category:2"), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Cache")]
    public async Task CategoryCacheInvalidateAsync_Should_Handle_Empty_Categories()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.Read(1, 50))
            .ReturnsAsync(new List<Domain.Model.Category>());

        // Act
        await _sut.CategoryCacheInvalidateAsync();

        // Assert
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error invalidating category cache"), Times.Never);
    }
}
