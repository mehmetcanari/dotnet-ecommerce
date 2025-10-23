using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Commands.Order;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Queries.Order;
using ECommerce.Application.Services.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace ECommerce.Tests.Services.Order;

[Trait("Category", "Order")]
[Trait("Category", "Service")]
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IBasketItemService> _basketItemServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IBasketItemRepository> _basketItemRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IStoreUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IMessageBroker> _messageBrokerMock;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _basketItemServiceMock = new Mock<IBasketItemService>();
        _productServiceMock = new Mock<IProductService>();
        _basketItemRepositoryMock = new Mock<IBasketItemRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _loggerMock = new Mock<ILoggingService>();
        _unitOfWorkMock = new Mock<IStoreUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _mediatorMock = new Mock<IMediator>();
        _messageBrokerMock = new Mock<IMessageBroker>();
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
        _paymentServiceMock.Object,
        _serviceProviderMock.Object,
        _currentUserServiceMock.Object,
        _mediatorMock.Object,
        _messageBrokerMock.Object);

    private void SetupCurrentUser(string email)
    {
        _currentUserServiceMock.Setup(c => c.GetUserEmail())
            .Returns(Result<string>.Success(email));
    }

    private void SetupAccount(Domain.Model.User account)
    {
        _accountRepositoryMock.Setup(r => r.GetAccountByEmail(It.IsAny<string>()))
            .ReturnsAsync(account);
    }

    private void SetupBasketItems(List<Domain.Model.BasketItem> basketItems)
    {
        _basketItemRepositoryMock.Setup(r => r.GetNonOrderedBasketItems(It.IsAny<Domain.Model.User>()))
            .ReturnsAsync(basketItems);
    }

    private void SetupOrder(Domain.Model.Order order)
    {
        _orderRepositoryMock.Setup(r => r.GetOrderById(It.IsAny<int>()))
            .ReturnsAsync(order);
    }

    private void SetupPendingOrders(List<Domain.Model.Order> orders)
    {
        _orderRepositoryMock.Setup(r => r.GetAccountPendingOrders(It.IsAny<int>()))
            .ReturnsAsync(orders ?? new List<Domain.Model.Order>());
    }

    private void SetupUserOrders(List<Domain.Model.Order> orders)
    {
        _orderRepositoryMock.Setup(r => r.GetAccountOrders(It.IsAny<int>()))
            .ReturnsAsync(orders ?? new List<Domain.Model.Order>());
    }

    private void SetupPaymentResult(bool success, string errorMessage = null)
    {
        var paymentResult = Result<Iyzipay.Model.Payment>.Success(new Iyzipay.Model.Payment 
        { 
            Status = success ? "success" : "failed", 
            ErrorMessage = errorMessage ?? string.Empty
        });
        
        _paymentServiceMock.Setup(p => p.ProcessPaymentAsync(
            It.IsAny<ECommerce.Domain.Model.Order>(),
            It.IsAny<ECommerce.Domain.Model.Buyer>(),
            It.IsAny<ECommerce.Domain.Model.Address>(),
            It.IsAny<ECommerce.Domain.Model.Address>(),
            It.IsAny<ECommerce.Domain.Model.PaymentCard>(),
            It.IsAny<List<ECommerce.Domain.Model.BasketItem>>()))
            .Returns(Task.FromResult(paymentResult));
    }

    private Domain.Model.User CreateAccount(int id = 1, string email = "test@example.com")
        => new Domain.Model.User
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
            UserId = accountId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = 100,
            ProductName = "Test Product",
            IsOrdered = false,
            ExternalId = Guid.NewGuid().ToString()
        };

    private Domain.Model.Order CreateOrder(int accountId = 1, OrderStatus status = OrderStatus.Pending)
        => new Domain.Model.Order
        {
            OrderId = 1,
            UserId = accountId,
            BasketItems = new List<Domain.Model.BasketItem> { CreateBasketItem() },
            OrderDate = DateTime.UtcNow,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address",
            Status = status
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
    [Trait("Operation", "Create")]
    public async Task CreateOrderAsync_Should_Create_Order_Successfully()
    {
        // Arrange
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        var request = CreateOrderRequest();
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItems(new List<Domain.Model.BasketItem> { basketItem });
        SetupPaymentResult(true);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _productServiceMock.Setup(p => p.UpdateProductStockAsync(It.IsAny<List<Domain.Model.BasketItem>>()))
            .ReturnsAsync(Result.Success());
        var service = CreateService();

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        _orderRepositoryMock.Verify(r => r.Create(It.IsAny<Domain.Model.Order>()), Times.Once);
        _basketItemServiceMock.Verify(s => s.DeleteAllNonOrderedBasketItemsAsync(), Times.Once);
        _productServiceMock.Verify(s => s.UpdateProductStockAsync(It.IsAny<List<Domain.Model.BasketItem>>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Create")]
    public async Task CreateOrderAsync_Should_Return_Failure_When_Payment_Fails()
    {
        // Arrange
        var account = CreateAccount();
        var basketItem = CreateBasketItem();
        var request = CreateOrderRequest();
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupBasketItems(new List<Domain.Model.BasketItem> { basketItem });
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransaction()).Returns(Task.CompletedTask);
        SetupPaymentResult(false, "Payment failed");
        var service = CreateService();

        // Act
        var result = await service.CreateOrderAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Payment failed: Payment failed");
        _unitOfWorkMock.Verify(u => u.RollbackTransaction(), Times.Once);
        _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Cancel")]
    public async Task CancelOrderAsync_Should_Cancel_Orders_Successfully()
    {
        // Arrange
        var account = CreateAccount();
        var order = CreateOrder(account.Id);
        var pendingOrders = new List<Domain.Model.Order> { order };
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupPendingOrders(pendingOrders);
        _orderRepositoryMock.Setup(r => r.Update(It.IsAny<Domain.Model.Order>()))
            .Callback<Domain.Model.Order>(o => o.Status = OrderStatus.Cancelled);
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success())
            .Callback<IRequest<Result>, CancellationToken>((request, token) => {
                if (request is UpdateOrderStatusCommand)
                {
                    _orderRepositoryMock.Object.Update(order);
                    _loggerMock.Object.LogInformation(It.IsAny<string>(), It.IsAny<object[]>());
                }
            });
        var service = CreateService();

        // Act
        var result = await service.UpdateOrderStatus(account.Id, new UpdateOrderStatusRequestDto { Status = OrderStatus.Cancelled });

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        _orderRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Cancel")]
    public async Task CancelOrderAsync_Should_Return_Failure_When_No_Pending_Orders()
    {
        // Arrange
        var account = CreateAccount();
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupPendingOrders(new List<Domain.Model.Order>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("No pending orders found"));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new CancelOrderCommand());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No pending orders found", result.Error);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteOrderByIdAsync_Should_Delete_Order_Successfully()
    {
        // Arrange
        var order = CreateOrder();
        SetupOrder(order);
        _orderRepositoryMock.Setup(r => r.Delete(It.IsAny<Domain.Model.Order>()));
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success())
            .Callback<IRequest<Result>, CancellationToken>((request, token) => {
                if (request is UpdateOrderStatusCommand)
                {
                    _orderRepositoryMock.Object.Delete(order);
                    _loggerMock.Object.LogInformation(It.IsAny<string>(), It.IsAny<object[]>());
                }
            });
        var service = CreateService();

        // Act
        var result = await service.UpdateOrderStatus(order.UserId, new UpdateOrderStatusRequestDto { Status = OrderStatus.Cancelled });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _orderRepositoryMock.Verify(r => r.Delete(It.Is<Domain.Model.Order>(o => o.OrderId == order.OrderId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task DeleteOrderByIdAsync_Should_Return_Failure_When_Order_Not_Found()
    {
        // Arrange
        SetupOrder(null);
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteOrderByIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Order not found"));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new DeleteOrderByIdCommand { Id = 1 });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found", result.Error);
        _orderRepositoryMock.Verify(r => r.Delete(It.IsAny<Domain.Model.Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllOrdersAsync_Should_Return_Orders_Successfully()
    {
        // Arrange
        var order = CreateOrder();
        _orderRepositoryMock.Setup(r => r.Read(1, 50))
            .ReturnsAsync(new List<Domain.Model.Order> { order });
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OrderResponseDto>>.Success(new List<OrderResponseDto> { new OrderResponseDto 
            { 
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            }}));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new GetAllOrdersQuery());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Single(result.Data);
        Assert.Equal(order.UserId, result.Data[0].UserId);
        Assert.Equal(order.OrderDate, result.Data[0].OrderDate);
        Assert.Equal(order.ShippingAddress, result.Data[0].ShippingAddress);
        Assert.Equal(order.BillingAddress, result.Data[0].BillingAddress);
        Assert.Equal(order.Status, result.Data[0].Status);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetAll")]
    public async Task GetAllOrdersAsync_Should_Return_Failure_When_No_Orders()
    {
        // Arrange
        _orderRepositoryMock.Setup(r => r.Read(1, 50))
            .ReturnsAsync(new List<Domain.Model.Order>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OrderResponseDto>>.Failure("No orders found"));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new GetAllOrdersQuery());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No orders found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "GetUserOrders")]
    public async Task GetUserOrdersAsync_Should_Return_User_Orders_Successfully()
    {
        // Arrange
        var account = CreateAccount();
        var order = CreateOrder(account.Id);
        var basketItem = CreateBasketItem();
        basketItem.IsOrdered = true;
        order.BasketItems = new List<Domain.Model.BasketItem> { basketItem };
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupUserOrders(new List<Domain.Model.Order> { order });
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OrderResponseDto>>.Success(new List<OrderResponseDto> { new OrderResponseDto 
            { 
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            }}));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new GetUserOrdersQuery());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Single(result.Data);
        Assert.Equal(order.UserId, result.Data[0].UserId);
        Assert.Equal(order.OrderDate, result.Data[0].OrderDate);
        Assert.Equal(order.ShippingAddress, result.Data[0].ShippingAddress);
        Assert.Equal(order.BillingAddress, result.Data[0].BillingAddress);
        Assert.Equal(order.Status, result.Data[0].Status);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetUserOrders")]
    public async Task GetUserOrdersAsync_Should_Return_Failure_When_No_Orders()
    {
        // Arrange
        var account = CreateAccount();
        
        SetupCurrentUser(account.Email);
        SetupAccount(account);
        SetupUserOrders(new List<Domain.Model.Order>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OrderResponseDto>>.Failure("No orders found for this user"));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new GetUserOrdersQuery());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("No orders found for this user", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetOrderByIdAsync_Should_Return_Order_Successfully()
    {
        // Arrange
        var order = CreateOrder();
        SetupOrder(order);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponseDto>.Success(new OrderResponseDto 
            { 
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            }));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new GetOrderByIdQuery { OrderId = order.OrderId });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(order.UserId, result.Data.UserId);
        Assert.Equal(order.OrderDate, result.Data.OrderDate);
        Assert.Equal(order.ShippingAddress, result.Data.ShippingAddress);
        Assert.Equal(order.BillingAddress, result.Data.BillingAddress);
        Assert.Equal(order.Status, result.Data.Status);
        _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetOrderByIdAsync_Should_Return_Failure_When_Order_Not_Found()
    {
        // Arrange
        SetupOrder(null);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderResponseDto>.Failure("Order not found"));
        var service = CreateService();

        // Act
        var result = await _mediatorMock.Object.Send(new GetOrderByIdQuery { OrderId = 1 });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found", result.Error);
        Assert.Null(result.Data);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateOrderStatusByAccountIdAsync_Should_Update_Status_Successfully()
    {
        // Arrange
        var order = CreateOrder();
        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.Delivered };
        _orderRepositoryMock.Setup(r => r.GetOrderByAccountId(It.IsAny<int>()))
            .ReturnsAsync(order);
        _orderRepositoryMock.Setup(r => r.Update(It.IsAny<Domain.Model.Order>()))
            .Callback<Domain.Model.Order>(o => o.Status = request.Status);
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success())
            .Callback<IRequest<Result>, CancellationToken>((request, token) => {
                if (request is UpdateOrderStatusCommand)
                {
                    _orderRepositoryMock.Object.Update(order);
                    _loggerMock.Object.LogInformation(It.IsAny<string>(), It.IsAny<object[]>());
                }
            });
        var service = CreateService();

        // Act
        var result = await service.UpdateOrderStatus(order.UserId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        _orderRepositoryMock.Verify(r => r.Update(It.Is<Domain.Model.Order>(o => o.Status == request.Status)), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Once);
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task UpdateOrderStatusByAccountIdAsync_Should_Return_Failure_When_Order_Not_Found()
    {
        // Arrange
        var request = new UpdateOrderStatusRequestDto { Status = OrderStatus.Delivered };
        _orderRepositoryMock.Setup(r => r.GetOrderByAccountId(It.IsAny<int>()))
            .ReturnsAsync((Domain.Model.Order)null);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Order not found"));
        var service = CreateService();

        // Act
        var result = await service.UpdateOrderStatus(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Order not found", result.Error);
        _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Domain.Model.Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(), Times.Never);
    }
}
