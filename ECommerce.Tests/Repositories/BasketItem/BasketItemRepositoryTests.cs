using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Repositories.BasketItem;

[Trait("Category", "BasketItem")]
[Trait("Category", "Repository")]
public class BasketItemRepositoryTests
{
    private readonly StoreDbContext _context;
    private readonly IBasketItemRepository _repository;

    public BasketItemRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StoreDbContext(options);
        _repository = new BasketItemRepository(_context);
    }

    private Domain.Model.Account CreateAccount(int id = 1)
        => new Domain.Model.Account
        {
            Id = id,
            Name = "Test",
            Surname = "User",
            Email = "test@example.com",
            IdentityNumber = "1234567890",
            City = "City",
            Country = "Country",
            ZipCode = "00000",
            Address = "Address",
            PhoneNumber = "5555555555",
            DateOfBirth = DateTime.UtcNow,
            Role = "User"
        };

    private Domain.Model.BasketItem CreateBasketItem(int basketItemId = 1, int accountId = 1, int productId = 1, bool isOrdered = false)
        => new Domain.Model.BasketItem
        {
            BasketItemId = basketItemId,
            AccountId = accountId,
            ProductId = productId,
            Quantity = 1,
            UnitPrice = 100,
            ProductName = "Test Product",
            IsOrdered = isOrdered,
            ExternalId = Guid.NewGuid().ToString()
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task Create_Should_Add_BasketItem_To_Database()
    {
        // Arrange
        var basketItem = CreateBasketItem();

        // Act
        await _repository.Create(basketItem);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.BasketItems.FindAsync(basketItem.BasketItemId);
        result.Should().NotBeNull();
        result.AccountId.Should().Be(basketItem.AccountId);
        result.ProductId.Should().Be(basketItem.ProductId);
        result.IsOrdered.Should().Be(basketItem.IsOrdered);
    }

    [Fact]
    [Trait("Operation", "GetNonOrdered")]
    public async Task GetNonOrderedBasketItems_Should_Return_Non_Ordered_Items()
    {
        // Arrange
        var account = CreateAccount();
        var basketItems = new List<Domain.Model.BasketItem>
        {
            CreateBasketItem(1, account.Id, 1, false),
            CreateBasketItem(2, account.Id, 2, true),
            CreateBasketItem(3, account.Id, 3, false)
        };

        await _context.BasketItems.AddRangeAsync(basketItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNonOrderedBasketItems(account);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(b => b.IsOrdered.Should().BeFalse());
        result.Should().AllSatisfy(b => b.AccountId.Should().Be(account.Id));
    }

    [Fact]
    [Trait("Operation", "GetSpecific")]
    public async Task GetSpecificAccountBasketItemWithId_Should_Return_BasketItem_When_Exists()
    {
        // Arrange
        var account = CreateAccount();
        var basketItem = CreateBasketItem(1, account.Id);
        await _context.BasketItems.AddAsync(basketItem);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSpecificAccountBasketItemWithId(basketItem.BasketItemId, account);

        // Assert
        result.Should().NotBeNull();
        result.BasketItemId.Should().Be(basketItem.BasketItemId);
        result.AccountId.Should().Be(account.Id);
    }

    [Fact]
    [Trait("Operation", "GetSpecific")]
    public async Task GetSpecificAccountBasketItemWithId_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var account = CreateAccount();

        // Act
        var result = await _repository.GetSpecificAccountBasketItemWithId(999, account);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetByProduct")]
    public async Task GetNonOrderedBasketItemIncludeSpecificProduct_Should_Return_Items_With_Product()
    {
        // Arrange
        var basketItems = new List<Domain.Model.BasketItem>
        {
            CreateBasketItem(1, 1, 1, false),
            CreateBasketItem(2, 2, 1, true),
            CreateBasketItem(3, 3, 1, false)
        };

        await _context.BasketItems.AddRangeAsync(basketItems);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNonOrderedBasketItemIncludeSpecificProduct(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(b => b.ProductId.Should().Be(1));
        result.Should().AllSatisfy(b => b.IsOrdered.Should().BeFalse());
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task Update_Should_Modify_BasketItem()
    {
        // Arrange
        var basketItem = CreateBasketItem();
        await _context.BasketItems.AddAsync(basketItem);
        await _context.SaveChangesAsync();

        // Act
        basketItem.Quantity = 2;
        basketItem.IsOrdered = true;
        _repository.Update(basketItem);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.BasketItems.FindAsync(basketItem.BasketItemId);
        result.Should().NotBeNull();
        result.Quantity.Should().Be(2);
        result.IsOrdered.Should().BeTrue();
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_BasketItem()
    {
        // Arrange
        var basketItem = CreateBasketItem();
        await _context.BasketItems.AddAsync(basketItem);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(basketItem);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.BasketItems.FindAsync(basketItem.BasketItemId);
        result.Should().BeNull();
    }

    private void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 