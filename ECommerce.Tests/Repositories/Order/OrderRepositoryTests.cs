using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Repositories.Order;

[Trait("Category", "Order")]
[Trait("Category", "Repository")]
public class OrderRepositoryTests
{
    private readonly StoreDbContext _context;
    private readonly IOrderRepository _repository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StoreDbContext(options);
        _repository = new OrderRepository(_context);
    }

    private Domain.Model.Order CreateOrder(int orderId = 1, int accountId = 1, OrderStatus status = OrderStatus.Pending, int basketItemId = 1)
        => new Domain.Model.Order
        {
            OrderId = orderId,
            UserId = accountId,
            OrderDate = DateTime.UtcNow,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address",
            Status = status,
            BasketItems = new List<Domain.Model.BasketItem>
            {
                new Domain.Model.BasketItem
                {
                    BasketItemId = basketItemId,
                    UserId = accountId,
                    ProductId = 1,
                    Quantity = 1,
                    UnitPrice = 100,
                    ProductName = "Test Product",
                    IsOrdered = true,
                    ExternalId = Guid.NewGuid().ToString()
                }
            }
        };

    [Fact]
    [Trait("Operation", "Create")]
    public async Task Create_Should_Add_Order_To_Database()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        await _repository.Create(order);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Orders.FindAsync(order.OrderId);
        result.Should().NotBeNull();
        result.UserId.Should().Be(order.UserId);
        result.Status.Should().Be(order.Status);
    }

    [Fact]
    [Trait("Operation", "Read")]
    public async Task Read_Should_Return_Orders_With_Pagination()
    {
        // Arrange
        var orders = new List<Domain.Model.Order>
        {
            CreateOrder(1, 1, OrderStatus.Pending, 1),
            CreateOrder(2, 1, OrderStatus.Delivered, 2),
            CreateOrder(3, 1, OrderStatus.Pending, 3)
        };

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.Read(1, 2);

        // Assert
        result.Should().HaveCount(2);
        result.Select(o => o.OrderId).Should().Contain(new[] {1, 2});
    }

    [Fact]
    [Trait("Operation", "Read")]
    public async Task Read_Should_Include_BasketItems()
    {
        // Arrange
        var order = CreateOrder();
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.Read();

        // Assert
        result.Should().HaveCount(1);
        result[0].BasketItems.Should().HaveCount(1);
        result[0].BasketItems.First().ProductName.Should().Be("Test Product");
    }

    [Fact]
    [Trait("Operation", "GetPendingOrders")]
    public async Task GetAccountPendingOrders_Should_Return_Pending_Orders()
    {
        // Arrange
        var orders = new List<Domain.Model.Order>
        {
            CreateOrder(1, 1, OrderStatus.Pending, 1),
            CreateOrder(2, 1, OrderStatus.Delivered, 2),
            CreateOrder(3, 1, OrderStatus.Pending, 3)
        };

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAccountPendingOrders(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.Status.Should().Be(OrderStatus.Pending));
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetOrderById_Should_Return_Order_When_Exists()
    {
        // Arrange
        var order = CreateOrder();
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrderById(order.OrderId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(order.UserId);
        result.Status.Should().Be(order.Status);
        result.BasketItems.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Operation", "GetById")]
    public async Task GetOrderById_Should_Return_Null_When_Order_Not_Exists()
    {
        // Act
        var result = await _repository.GetOrderById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetByAccountId")]
    public async Task GetOrderByAccountId_Should_Return_Order_When_Exists()
    {
        // Arrange
        var order = CreateOrder();
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrderByAccountId(order.UserId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(order.UserId);
        result.Status.Should().Be(order.Status);
    }

    [Fact]
    [Trait("Operation", "GetByAccountId")]
    public async Task GetOrderByAccountId_Should_Return_Null_When_Order_Not_Exists()
    {
        // Act
        var result = await _repository.GetOrderByAccountId(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Operation", "GetAccountOrders")]
    public async Task GetAccountOrders_Should_Return_All_Account_Orders()
    {
        // Arrange
        var orders = new List<Domain.Model.Order>
        {
            CreateOrder(1, 1, OrderStatus.Pending, 1),
            CreateOrder(2, 1, OrderStatus.Delivered, 2),
            CreateOrder(3, 2, OrderStatus.Pending, 3)
        };

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAccountOrders(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.UserId.Should().Be(1));
    }

    [Fact]
    [Trait("Operation", "Update")]
    public async Task Update_Should_Modify_Order()
    {
        // Arrange
        var order = CreateOrder();
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        order.Status = OrderStatus.Delivered;
        _repository.Update(order);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Orders.FindAsync(order.OrderId);
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    [Trait("Operation", "Delete")]
    public async Task Delete_Should_Remove_Order()
    {
        // Arrange
        var order = CreateOrder();
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(order);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Orders.FindAsync(order.OrderId);
        result.Should().BeNull();
    }

    private void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 