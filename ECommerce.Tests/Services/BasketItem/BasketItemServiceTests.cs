using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Services.BasketItem;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Application.Utility;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services.BasketItem;

[Trait("Category", "BasketItem")]
[Trait("Category", "Service")]
public class BasketItemServiceTests
{
    private readonly Mock<IBasketItemRepository> _basketItemRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public BasketItemServiceTests()
    {
        _basketItemRepositoryMock = new Mock<IBasketItemRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILoggingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
    }

    private BasketItemService CreateService() => new BasketItemService(
        _basketItemRepositoryMock.Object,
        _productRepositoryMock.Object,
        _accountRepositoryMock.Object,
        _loggerMock.Object,
        _cacheServiceMock.Object,
        _unitOfWorkMock.Object,
        _serviceProviderMock.Object,
        _currentUserServiceMock.Object);

    private void SetupCurrentUser(string email)
    {
        _currentUserServiceMock.Setup(c => c.GetCurrentUserEmail())
            .Returns(Result<string>.Success(email));
    }

    private void SetupAccount(Domain.Model.Account account)
    {
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail(account.Email))
            .ReturnsAsync(account);
    }

    private void SetupProduct(Domain.Model.Product product)
    {
        _productRepositoryMock.Setup(r => r.GetProductById(product.ProductId))
            .ReturnsAsync(product);
    }

    private void SetupBasketItem(Domain.Model.Account account, Domain.Model.BasketItem basketItem)
    {
        _basketItemRepositoryMock.Setup(r => r.GetNonOrderedBasketItems(account))
            .ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
    }

    private void SetupBasketItemWithId(Domain.Model.Account account, Domain.Model.BasketItem basketItem)
    {
        _basketItemRepositoryMock.Setup(r => r.GetSpecificAccountBasketItemWithId(basketItem.BasketItemId, account))
            .ReturnsAsync(basketItem);
    }

    private void SetupCache<T>(string key, T data)
    {
        _cacheServiceMock.Setup(c => c.GetAsync<T>(key))
            .ReturnsAsync(data);
    }

    private void SetupCacheRemoval()
    {
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    private void SetupBasketItems(List<Domain.Model.BasketItem> basketItems)
    {
        _basketItemRepositoryMock.Setup(r => r.GetNonOrderedBasketItems(It.IsAny<Domain.Model.Account>()))
            .ReturnsAsync(basketItems);
    }

    private Domain.Model.Account CreateAccount(int id = 1, string email = "test@example.com")
        => new Domain.Model.Account
        {
            Id = id,
            Email = email,
            Name = "Test",
            Surname = "User",
            Role = "User",
            IdentityNumber = "12345678901",
            City = "Istanbul",
            Country = "Turkey",
            ZipCode = "34000",
            Address = "Test Address",
            PhoneNumber = "5551234567",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

    private Domain.Model.Product CreateProduct(int id = 1, int stock = 10, decimal price = 100)
        => new Domain.Model.Product
        {
            ProductId = id,
            Name = "Test Product",
            Description = "Test Description",
            StockQuantity = stock,
            Price = price,
            DiscountRate = 0,
            ImageUrl = "https://example.com/image.jpg"
        };

    private Domain.Model.BasketItem CreateBasketItem(int accountId = 1, int productId = 1, int quantity = 1)
        => new Domain.Model.BasketItem
        {
            BasketItemId = 1,
            AccountId = accountId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = 100,
            ProductName = "Test Product",
            IsOrdered = false,
            ExternalId = Guid.NewGuid().ToString()
        };

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllBasketItemsAsync_Should_Return_Items_From_Cache()
    {
        // Arrange
        var cachedItems = new List<BasketItemResponseDto> { new() { 
            ProductId = 1,
            ProductName = "Test Product",
            AccountId = 1,
            Quantity = 1,
            UnitPrice = 100
        } };
        SetupCache(GetAllBasketItemsCacheKey, cachedItems);
        SetupCurrentUser("test@example.com");
        var service = CreateService();

        // Act
        var result = await service.GetAllBasketItemsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Single(result.Data);
        Assert.Equal(cachedItems[0].ProductId, result.Data[0].ProductId);
        _loggerMock.Verify(l => l.LogInformation("Basket items fetched from cache"), Times.Once);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllBasketItemsAsync_Should_Return_Items_From_Repository_When_Not_In_Cache()
    {
        // Arrange
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        SetupCache<List<BasketItemResponseDto>>(GetAllBasketItemsCacheKey, null);
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItem(account, basketItem);
        SetupCacheRemoval();
        var service = CreateService();

        // Act
        var result = await service.GetAllBasketItemsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Single(result.Data);
        Assert.Equal(basketItem.ProductId, result.Data[0].ProductId);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<BasketItemResponseDto>>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllBasketItemsAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        SetupCurrentUser("notfound@example.com");
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail("notfound@example.com"))
            .ReturnsAsync((Domain.Model.Account)null);
        var service = CreateService();

        // Act
        var result = await service.GetAllBasketItemsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllBasketItemsAsync_Should_Return_Failure_When_No_Basket_Items()
    {
        // Arrange
        var account = CreateAccount();
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        _basketItemRepositoryMock.Setup(r => r.GetNonOrderedBasketItems(account))
            .ReturnsAsync(new List<Domain.Model.BasketItem>());
        var service = CreateService();

        // Act
        var result = await service.GetAllBasketItemsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No basket items found.", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateBasketItemAsync_Should_Create_Item_Successfully()
    {
        // Arrange
        var account = CreateAccount();
        var product = CreateProduct();
        var request = new CreateBasketItemRequestDto { ProductId = product.ProductId, Quantity = 1 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupProduct(product);
        SetupCacheRemoval();
        var service = CreateService();

        // Act
        var result = await service.CreateBasketItemAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _basketItemRepositoryMock.Verify(r => r.Create(It.Is<Domain.Model.BasketItem>(b => 
            b.AccountId == account.Id && 
            b.ProductId == product.ProductId && 
            b.Quantity == request.Quantity)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Basket item created successfully")), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateBasketItemAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        var request = new CreateBasketItemRequestDto { ProductId = 1, Quantity = 1 };
        SetupCurrentUser("notfound@example.com");
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail("notfound@example.com"))
            .ReturnsAsync((Domain.Model.Account)null);
        var service = CreateService();

        // Act
        var result = await service.CreateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateBasketItemAsync_Should_Return_Failure_When_Product_Not_Found()
    {
        // Arrange
        var account = CreateAccount();
        var request = new CreateBasketItemRequestDto { ProductId = 1, Quantity = 1 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        _productRepositoryMock.Setup(r => r.GetProductById(request.ProductId))
            .ReturnsAsync((Domain.Model.Product)null);
        var service = CreateService();

        // Act
        var result = await service.CreateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateBasketItemAsync_Should_Return_Failure_When_Not_Enough_Stock()
    {
        // Arrange
        var account = CreateAccount();
        var product = CreateProduct(stock: 5);
        var request = new CreateBasketItemRequestDto { ProductId = product.ProductId, Quantity = 10 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupProduct(product);
        var service = CreateService();

        // Act
        var result = await service.CreateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Not enough stock", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateBasketItemAsync_Should_Update_Item_Successfully()
    {
        // Arrange
        var account = CreateAccount();
        var product = CreateProduct();
        var basketItem = CreateBasketItem();
        var request = new UpdateBasketItemRequestDto { BasketItemId = basketItem.BasketItemId, ProductId = product.ProductId, Quantity = 2 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItemWithId(account, basketItem);
        SetupProduct(product);
        SetupCacheRemoval();
        var service = CreateService();

        // Act
        var result = await service.UpdateBasketItemAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _basketItemRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.BasketItem>(b => 
            b.BasketItemId == basketItem.BasketItemId && 
            b.ProductId == product.ProductId && 
            b.Quantity == request.Quantity)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Basket item updated successfully")), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateBasketItemAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        var request = new UpdateBasketItemRequestDto { BasketItemId = 1, ProductId = 1, Quantity = 1 };
        SetupCurrentUser("notfound@example.com");
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail("notfound@example.com"))
            .ReturnsAsync((Domain.Model.Account)null);
        var service = CreateService();

        // Act
        var result = await service.UpdateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateBasketItemAsync_Should_Return_Failure_When_Basket_Item_Not_Found()
    {
        // Arrange
        var account = CreateAccount();
        var request = new UpdateBasketItemRequestDto { BasketItemId = 1, ProductId = 1, Quantity = 1 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        _basketItemRepositoryMock.Setup(r => r.GetSpecificAccountBasketItemWithId(request.BasketItemId, account))
            .ReturnsAsync((Domain.Model.BasketItem)null);
        var service = CreateService();

        // Act
        var result = await service.UpdateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Basket item not found", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateBasketItemAsync_Should_Return_Failure_When_Product_Not_Found()
    {
        // Arrange
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        var request = new UpdateBasketItemRequestDto { BasketItemId = basketItem.BasketItemId, ProductId = 1, Quantity = 1 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItemWithId(account, basketItem);
        _productRepositoryMock.Setup(r => r.GetProductById(request.ProductId))
            .ReturnsAsync((Domain.Model.Product)null);
        var service = CreateService();

        // Act
        var result = await service.UpdateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Product not found", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateBasketItemAsync_Should_Return_Failure_When_Not_Enough_Stock()
    {
        // Arrange
        var account = CreateAccount();
        var product = CreateProduct(stock: 5);
        var basketItem = CreateBasketItem();
        var request = new UpdateBasketItemRequestDto { BasketItemId = basketItem.BasketItemId, ProductId = product.ProductId, Quantity = 10 };
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItemWithId(account, basketItem);
        SetupProduct(product);
        var service = CreateService();

        // Act
        var result = await service.UpdateBasketItemAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Not enough stock", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteAllNonOrderedBasketItemsAsync_Should_Delete_Items_Successfully()
    {
        // Arrange
        var account = CreateAccount();
        var basketItems = new List<Domain.Model.BasketItem> { CreateBasketItem() };
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItems(basketItems);
        var service = CreateService();

        // Act
        var result = await service.DeleteAllNonOrderedBasketItemsAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _basketItemRepositoryMock.Verify(r => r.Delete(It.Is<Domain.Model.BasketItem>(b => 
            b.AccountId == account.Id && 
            !b.IsOrdered)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("All basket items deleted successfully", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteAllNonOrderedBasketItemsAsync_Should_Return_Failure_When_Account_Not_Found()
    {
        // Arrange
        SetupCurrentUser("notfound@example.com");
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail("notfound@example.com"))
            .ReturnsAsync((Domain.Model.Account)null);
        var service = CreateService();

        // Act
        var result = await service.DeleteAllNonOrderedBasketItemsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Account not found", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteAllNonOrderedBasketItemsAsync_Should_Return_Failure_When_No_Items_To_Delete()
    {
        // Arrange
        var account = CreateAccount();
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        _basketItemRepositoryMock.Setup(r => r.GetNonOrderedBasketItems(account))
            .ReturnsAsync(new List<Domain.Model.BasketItem>());
        var service = CreateService();

        // Act
        var result = await service.DeleteAllNonOrderedBasketItemsAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No basket items found to delete", result.Error);
        _basketItemRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Clear")]
    public async Task ClearBasketItemsIncludeOrderedProductAsync_Should_Clear_Items_Successfully()
    {
        // Arrange
        var product = CreateProduct();
        var basketItem = CreateBasketItem();
        _basketItemRepositoryMock.Setup(r => r.GetNonOrderedBasketItemIncludeSpecificProduct(product.ProductId))
            .ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        SetupCacheRemoval();
        var service = CreateService();

        // Act
        await service.ClearBasketItemsIncludeOrderedProductAsync(product);

        // Assert
        _basketItemRepositoryMock.Verify(r => r.Delete(It.Is<Domain.Model.BasketItem>(b => b.BasketItemId == basketItem.BasketItemId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Basket items cleared successfully")), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Clear")]
    public async Task ClearBasketItemsIncludeOrderedProductAsync_Should_Not_Delete_When_No_Matching_Items()
    {
        // Arrange
        var account = CreateAccount();
        var basketItems = new List<Domain.Model.BasketItem> { CreateBasketItem() };
        var product = CreateProduct();
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItems(basketItems);
        var service = CreateService();

        // Act
        await service.ClearBasketItemsIncludeOrderedProductAsync(product);

        // Assert
        _basketItemRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    private const string GetAllBasketItemsCacheKey = "GetAllBasketItems";
}
