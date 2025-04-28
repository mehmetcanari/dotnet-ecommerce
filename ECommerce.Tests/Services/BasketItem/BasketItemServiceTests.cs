using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Services.BasketItem;
using Moq;
using Xunit;

namespace ECommerce.Tests.Services.BasketItem;

public class BasketItemServiceTests
{
    private readonly Mock<IBasketItemRepository> _basketItemRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public BasketItemServiceTests()
    {
        _basketItemRepositoryMock = new Mock<IBasketItemRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILoggingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    private BasketItemService CreateService() => new BasketItemService(
        _basketItemRepositoryMock.Object,
        _productRepositoryMock.Object,
        _accountRepositoryMock.Object,
        _loggerMock.Object,
        _cacheServiceMock.Object,
        _unitOfWorkMock.Object);

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
    public async Task GetAllBasketItemsAsync_Should_Return_Items_From_Cache()
    {
        var cachedItems = new List<BasketItemResponseDto> { new() { 
            ProductId = 1,
            ProductName = "Test Product",
            AccountId = 1,
            Quantity = 1,
            UnitPrice = 100
        } };
        _cacheServiceMock.Setup(c => c.GetAsync<List<BasketItemResponseDto>>(It.IsAny<string>()))
            .ReturnsAsync(cachedItems);
        var service = CreateService();

        var result = await service.GetAllBasketItemsAsync("test@example.com");

        Assert.Single(result);
        Assert.Equal(cachedItems[0].ProductId, result[0].ProductId);
        _loggerMock.Verify(l => l.LogInformation("Basket items fetched from cache"), Times.Once);
    }

    [Fact]
    public async Task GetAllBasketItemsAsync_Should_Return_Items_From_Repository_When_Not_In_Cache()
    {
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        _cacheServiceMock.Setup(c => c.GetAsync<List<BasketItemResponseDto>>(It.IsAny<string>()))
            .ReturnsAsync((List<BasketItemResponseDto>)null);
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _cacheServiceMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<BasketItemResponseDto>>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);
        var service = CreateService();

        var result = await service.GetAllBasketItemsAsync(account.Email);

        Assert.Single(result);
        Assert.Equal(basketItem.ProductId, result[0].ProductId);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<List<BasketItemResponseDto>>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetAllBasketItemsAsync_Should_ThrowException_When_Account_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAllBasketItemsAsync("notfound@example.com"));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while fetching all basket items"), Times.Once);
    }

    [Fact]
    public async Task GetAllBasketItemsAsync_Should_ThrowException_When_No_Basket_Items()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAllBasketItemsAsync(account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while fetching all basket items"), Times.Once);
    }

    [Fact]
    public async Task CreateBasketItemAsync_Should_Create_Item_Successfully()
    {
        var account = CreateAccount();
        var product = CreateProduct();
        var request = new CreateBasketItemRequestDto { ProductId = product.ProductId, Quantity = 1 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _productRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Product> { product });
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.CreateBasketItemAsync(request, account.Email);

        _basketItemRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.BasketItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task CreateBasketItemAsync_Should_ThrowException_When_Account_Not_Found()
    {
        var request = new CreateBasketItemRequestDto { ProductId = 1, Quantity = 1 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.CreateBasketItemAsync(request, "notfound@example.com"));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while creating basket item"), Times.Once);
    }

    [Fact]
    public async Task CreateBasketItemAsync_Should_ThrowException_When_Product_Not_Found()
    {
        var account = CreateAccount();
        var request = new CreateBasketItemRequestDto { ProductId = 1, Quantity = 1 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _productRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Product>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.CreateBasketItemAsync(request, account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while creating basket item"), Times.Once);
    }

    [Fact]
    public async Task CreateBasketItemAsync_Should_ThrowException_When_Not_Enough_Stock()
    {
        var account = CreateAccount();
        var product = CreateProduct(stock: 5);
        var request = new CreateBasketItemRequestDto { ProductId = product.ProductId, Quantity = 10 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _productRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Product> { product });
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.CreateBasketItemAsync(request, account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while creating basket item"), Times.Once);
    }

    [Fact]
    public async Task UpdateBasketItemAsync_Should_Update_Item_Successfully()
    {
        var account = CreateAccount();
        var product = CreateProduct();
        var basketItem = CreateBasketItem();
        var request = new UpdateBasketItemRequestDto { BasketItemId = basketItem.BasketItemId, ProductId = product.ProductId, Quantity = 2 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _productRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Product> { product });
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.UpdateBasketItemAsync(request, account.Email);

        _basketItemRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.BasketItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBasketItemAsync_Should_ThrowException_When_Account_Not_Found()
    {
        var request = new UpdateBasketItemRequestDto { BasketItemId = 1, ProductId = 1, Quantity = 1 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UpdateBasketItemAsync(request, "notfound@example.com"));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while updating basket item"), Times.Once);
    }

    [Fact]
    public async Task UpdateBasketItemAsync_Should_ThrowException_When_Basket_Item_Not_Found()
    {
        var account = CreateAccount();
        var request = new UpdateBasketItemRequestDto { BasketItemId = 1, ProductId = 1, Quantity = 1 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UpdateBasketItemAsync(request, account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while updating basket item"), Times.Once);
    }

    [Fact]
    public async Task UpdateBasketItemAsync_Should_ThrowException_When_Product_Not_Found()
    {
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        var request = new UpdateBasketItemRequestDto { BasketItemId = basketItem.BasketItemId, ProductId = 1, Quantity = 1 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _productRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Product>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UpdateBasketItemAsync(request, account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while updating basket item"), Times.Once);
    }

    [Fact]
    public async Task UpdateBasketItemAsync_Should_ThrowException_When_Not_Enough_Stock()
    {
        var account = CreateAccount();
        var product = CreateProduct(stock: 5);
        var basketItem = CreateBasketItem();
        var request = new UpdateBasketItemRequestDto { BasketItemId = basketItem.BasketItemId, ProductId = product.ProductId, Quantity = 10 };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _productRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Product> { product });
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UpdateBasketItemAsync(request, account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while updating basket item"), Times.Once);
    }

    [Fact]
    public async Task DeleteAllBasketItemsAsync_Should_Delete_Items_Successfully()
    {
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.DeleteAllBasketItemsAsync(account.Email);

        _basketItemRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.BasketItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAllBasketItemsAsync_Should_ThrowException_When_Account_Not_Found()
    {
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.DeleteAllBasketItemsAsync("notfound@example.com"));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while deleting basket items"), Times.Once);
    }

    [Fact]
    public async Task DeleteAllBasketItemsAsync_Should_ThrowException_When_No_Items_To_Delete()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.DeleteAllBasketItemsAsync(account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while deleting basket items"), Times.Once);
    }

    [Fact]
    public async Task ClearBasketItemsIncludeOrderedProductAsync_Should_Clear_Items_Successfully()
    {
        var product = CreateProduct();
        var basketItem = CreateBasketItem();
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.ClearBasketItemsIncludeOrderedProductAsync(product);

        _basketItemRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.BasketItem>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task ClearBasketItemsIncludeOrderedProductAsync_Should_Not_Delete_When_No_Matching_Items()
    {
        var product = CreateProduct();
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem>());
        var service = CreateService();

        await service.ClearBasketItemsIncludeOrderedProductAsync(product);

        _basketItemRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.BasketItem>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task ClearBasketItemsCacheAsync_Should_Clear_Cache_Successfully()
    {
        _cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.ClearBasketItemsCacheAsync();

        _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
    }
}
