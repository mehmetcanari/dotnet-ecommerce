using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace ECommerce.Tests.Repositories.Category;

[Trait("Category", "Category")]
[Trait("Category", "Repository")]
public class CategoryRepositoryTests
{
    private readonly StoreDbContext _context;
    private readonly ICategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StoreDbContext(options);
        _repository = new CategoryRepository(_context);
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

    [Fact]
    [Trait("Operation", "Create")]
    public async Task Create_Should_Add_Category_To_Database()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        await _repository.Create(category);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Categories.FindAsync(category.CategoryId);
        result.Should().NotBeNull();
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
    }

    [Fact]
    [Trait("Operation", "Read")]
    public async Task Read_Should_Return_Categories_With_Pagination()
    {
        // Arrange
        var categories = new List<Domain.Model.Category>
        {
            CreateCategory(1, "Category 1"),
            CreateCategory(2, "Category 2"),
            CreateCategory(3, "Category 3")
        };

        await _context.Categories.AddRangeAsync(categories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.Read(1, 2);

        // Assert
        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().Contain(new[] {"Category 1", "Category 2"});
    }

    [Fact]
    [Trait("Operation", "CheckExists")]
    public async Task CheckCategoryExistsWithName_Should_Return_True_When_Category_Exists()
    {
        // Arrange
        var category = CreateCategory();
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CheckCategoryExistsWithName(category.Name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Operation", "CheckExists")]
    public async Task CheckCategoryExistsWithName_Should_Return_False_When_Category_Not_Exists()
    {
        // Act
        var result = await _repository.CheckCategoryExistsWithName("NonExistentCategory");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryById_Should_Return_Category_When_Exists()
    {
        // Arrange
        var category = CreateCategory();
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCategoryById(category.CategoryId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(category.Name);
        result.Description.Should().Be(category.Description);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetCategoryById_Should_Return_Null_When_Category_Not_Exists()
    {
        // Act
        var result = await _repository.GetCategoryById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task Update_Should_Modify_Category()
    {
        // Arrange
        var category = CreateCategory();
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        category.Name = "Updated Name";
        category.Description = "Updated Description";
        _repository.Update(category);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Categories.FindAsync(category.CategoryId);
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_Category()
    {
        // Arrange
        var category = CreateCategory();
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(category);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Categories.FindAsync(category.CategoryId);
        result.Should().BeNull();
    }

    private void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 