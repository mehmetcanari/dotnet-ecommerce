using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Application.Services.Order;
using ECommerce.Domain.Model;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Iyzipay.Model;
using Iyzipay.Request;

namespace ECommerce.Tests.Services.Order;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IBasketItemService> _basketItemServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IBasketItemRepository> _basketItemRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _basketItemServiceMock = new Mock<IBasketItemService>();
        _productServiceMock = new Mock<IProductService>();
        _basketItemRepositoryMock = new Mock<IBasketItemRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILoggingService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _paymentServiceMock = new Mock<IPaymentService>();
    }

    private OrderService CreateService() => new OrderService(
        _orderRepositoryMock.Object,
        _basketItemServiceMock.Object,
        _basketItemRepositoryMock.Object,
        _unitOfWorkMock.Object,
        _productServiceMock.Object,
        _accountRepositoryMock.Object,
        _loggerMock.Object,
        _httpContextAccessorMock.Object,
        _paymentServiceMock.Object);

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

    private OrderCreateRequestDto CreateOrderRequest()
        => new OrderCreateRequestDto
        {
            PaymentCard = new Domain.Model.PaymentCard
            {
                CardHolderName = "Test User",
                CardNumber = "5528790000000008",
                ExpirationMonth = 12,
                ExpirationYear = 2030,
                CVC = "123",
                RegisterCard = 0
            }
        };

    [Fact]
    public async Task AddOrderAsync_Should_Create_Order_Successfully()
    {
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        var request = CreateOrderRequest();
        var paymentResult = new Iyzipay.Model.Payment { Status = "success" };

        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _paymentServiceMock.Setup(p => p.ProcessPaymentAsync(
            It.IsAny<Domain.Model.Order>(), 
            It.IsAny<Domain.Model.Buyer>(), 
            It.IsAny<Domain.Model.Address>(), 
            It.IsAny<Domain.Model.Address>(), 
            It.IsAny<Domain.Model.PaymentCard>(), 
            It.IsAny<List<Domain.Model.BasketItem>>()))
            .ReturnsAsync(paymentResult);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        var service = CreateService();

        await service.AddOrderAsync(request, account.Email);

        _orderRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Order>()), Times.Once);
        _basketItemServiceMock.Verify(s => s.DeleteAllBasketItemsAsync(account.Email), Times.Once);
        _productServiceMock.Verify(s => s.UpdateProductStockAsync(It.IsAny<List<Domain.Model.BasketItem>>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task AddOrderAsync_Should_Rollback_When_Payment_Fails()
    {
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        var request = CreateOrderRequest();
        var paymentResult = new Iyzipay.Model.Payment { Status = "failed", ErrorMessage = "Payment failed" };

        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _basketItemRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.BasketItem> { basketItem });
        _paymentServiceMock.Setup(p => p.ProcessPaymentAsync(
            It.IsAny<Domain.Model.Order>(), 
            It.IsAny<Domain.Model.Buyer>(), 
            It.IsAny<Domain.Model.Address>(), 
            It.IsAny<Domain.Model.Address>(), 
            It.IsAny<Domain.Model.PaymentCard>(), 
            It.IsAny<List<Domain.Model.BasketItem>>()))
            .ReturnsAsync(paymentResult);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransaction()).Returns(Task.CompletedTask);
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.AddOrderAsync(request, account.Email));
        _unitOfWorkMock.Verify(u => u.RollbackTransaction(), Times.Once);
        _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_Should_Cancel_Orders_Successfully()
    {
        var account = CreateAccount();
        var order = new Domain.Model.Order 
        { 
            AccountId = account.Id, 
            Status = OrderStatus.Pending,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address"
        };
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order> { order });
        var service = CreateService();

        await service.CancelOrderAsync(account.Email);

        _orderRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_Should_ThrowException_When_No_Pending_Orders()
    {
        var account = CreateAccount();
        _accountRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Account> { account });
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.CancelOrderAsync(account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while cancelling orders: {Message}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderByIdAsync_Should_Delete_Order_Successfully()
    {
        var order = new Domain.Model.Order 
        { 
            OrderId = 1,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address"
        };
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order> { order });
        var service = CreateService();

        await service.DeleteOrderByIdAsync(order.OrderId);

        _orderRepositoryMock.Verify(r => r.Delete(It.Is<Domain.Model.Order>(o => o.OrderId == order.OrderId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderByIdAsync_Should_ThrowException_When_Order_Not_Found()
    {
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.DeleteOrderByIdAsync(1));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while deleting order: {Message}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_Should_Return_Orders_Successfully()
    {
        var order = new Domain.Model.Order
        {
            OrderId = 1,
            AccountId = 1,
            BasketItems = new List<Domain.Model.BasketItem> { CreateBasketItem() },
            OrderDate = DateTime.UtcNow,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address",
            Status = OrderStatus.Pending
        };
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order> { order });
        var service = CreateService();

        var result = await service.GetAllOrdersAsync();

        Assert.Single(result);
        Assert.Equal(order.AccountId, result[0].AccountId);
        Assert.Equal(order.OrderDate, result[0].OrderDate);
        Assert.Equal(order.ShippingAddress, result[0].ShippingAddress);
        Assert.Equal(order.BillingAddress, result[0].BillingAddress);
        Assert.Equal(order.Status, result[0].Status);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrdersAsync_Should_ThrowException_When_No_Orders()
    {
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetAllOrdersAsync());
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while fetching all orders: {Message}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetUserOrdersAsync_Should_Return_User_Orders_Successfully()
    {
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        basketItem.IsOrdered = true;
        
        var order = new Domain.Model.Order
        {
            OrderId = 1,
            AccountId = account.Id,
            BasketItems = new List<Domain.Model.BasketItem> { basketItem },
            OrderDate = DateTime.UtcNow,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address",
            Status = OrderStatus.Pending
        };
        
        // Setup account repository to return the test account
        _accountRepositoryMock.Setup(r => r.Read())
            .ReturnsAsync(new List<Domain.Model.Account> { account });
            
        // Setup order repository to return orders filtered by AccountId
        _orderRepositoryMock.Setup(r => r.Read())
            .ReturnsAsync(new List<Domain.Model.Order> { order });
            
        var service = CreateService();

        var result = await service.GetUserOrdersAsync(account.Email);

        Assert.Single(result);
        Assert.Equal(order.AccountId, result[0].AccountId);
        Assert.Equal(order.OrderDate, result[0].OrderDate);
        Assert.Equal(order.ShippingAddress, result[0].ShippingAddress);
        Assert.Equal(order.BillingAddress, result[0].BillingAddress);
        Assert.Equal(order.Status, result[0].Status);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetUserOrdersAsync_Should_ThrowException_When_No_Orders()
    {
        var account = CreateAccount();
        
        // Setup account repository to return the test account
        _accountRepositoryMock.Setup(r => r.Read())
            .ReturnsAsync(new List<Domain.Model.Account> { account });
            
        // Setup order repository to return empty list
        _orderRepositoryMock.Setup(r => r.Read())
            .ReturnsAsync(new List<Domain.Model.Order>());
            
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetUserOrdersAsync(account.Email));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while fetching user orders: {Message}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task GetOrderByIdAsync_Should_Return_Order_Successfully()
    {
        var order = new Domain.Model.Order
        {
            OrderId = 1,
            AccountId = 1,
            BasketItems = new List<Domain.Model.BasketItem> { CreateBasketItem() },
            OrderDate = DateTime.UtcNow,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address",
            Status = OrderStatus.Pending
        };
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order> { order });
        var service = CreateService();

        var result = await service.GetOrderByIdAsync(order.OrderId);

        Assert.Equal(order.AccountId, result.AccountId);
        Assert.Equal(order.OrderDate, result.OrderDate);
        Assert.Equal(order.ShippingAddress, result.ShippingAddress);
        Assert.Equal(order.BillingAddress, result.BillingAddress);
        Assert.Equal(order.Status, result.Status);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetOrderByIdAsync_Should_ThrowException_When_Order_Not_Found()
    {
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.GetOrderByIdAsync(1));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while fetching order with id: {Message}", It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusByAccountIdAsync_Should_Update_Status_Successfully()
    {
        var order = new Domain.Model.Order 
        { 
            OrderId = 1, 
            AccountId = 1, 
            Status = OrderStatus.Pending,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address"
        };
        var request = new OrderUpdateRequestDto { Status = OrderStatus.Delivered };
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order> { order });
        var service = CreateService();

        await service.UpdateOrderStatusByAccountIdAsync(order.AccountId, request);

        _orderRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Order>(o => o.Status == request.Status)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusByAccountIdAsync_Should_ThrowException_When_Order_Not_Found()
    {
        var request = new OrderUpdateRequestDto { Status = OrderStatus.Delivered };
        _orderRepositoryMock.Setup(r => r.Read()).ReturnsAsync(new List<Domain.Model.Order>());
        var service = CreateService();

        await Assert.ThrowsAsync<Exception>(() => service.UpdateOrderStatusByAccountIdAsync(1, request));
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Unexpected error while updating order status: {Message}", It.IsAny<object[]>()), Times.Once);
    }
}
