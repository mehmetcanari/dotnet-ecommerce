using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Services.Payment;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Moq;
using Xunit;
using FluentAssertions;

namespace ECommerce.Tests.Services.Payment;

[Trait("Category", "Payment")]
[Trait("Category", "Service")]
public class PaymentServiceTests
{
    private readonly Mock<ILoggingService> _loggerMock;
    private readonly Mock<IPaymentProvider> _paymentProviderMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IPaymentService _paymentService;

    public PaymentServiceTests()
    {
        _loggerMock = new Mock<ILoggingService>();
        _paymentProviderMock = new Mock<IPaymentProvider>();
        _notificationServiceMock = new Mock<INotificationService>();
        _paymentService = new IyzicoPaymentService(_loggerMock.Object, _paymentProviderMock.Object, _notificationServiceMock.Object);
    }

    private Domain.Model.Order CreateOrder(double totalAmount = 100)
    {
        var basketItems = new List<Domain.Model.BasketItem>
        {
            new Domain.Model.BasketItem
            {
                AccountId = 1,
                ExternalId = Guid.NewGuid().ToString(),
                Quantity = 1,
                UnitPrice = totalAmount,
                ProductId = 1,
                ProductName = "Test Product",
                IsOrdered = false
            }
        };

        return new Domain.Model.Order
        {
            OrderId = 1,
            AccountId = 1,
            BasketItems = basketItems,
            ShippingAddress = "Test Shipping Address",
            BillingAddress = "Test Billing Address"
        };
    }

    private Domain.Model.Buyer CreateBuyer()
        => new Domain.Model.Buyer
        {
            Id = "1",
            Name = "John",
            Surname = "Doe",
            Email = "email@email.com",
            IdentityNumber = "74300864791",
            RegistrationAddress = "Test Address",
            City = "Istanbul",
            Country = "Turkey",
            Ip = "85.34.78.112",
            GsmNumber = "5551234567",
            ZipCode = "34000"
        };

    private Domain.Model.Address CreateAddress()
        => new Domain.Model.Address
        {
            ContactName = "John Doe",
            City = "Istanbul",
            Country = "Turkey",
            Description = "Test Address",
            ZipCode = "34000"
        };

    private Domain.Model.PaymentCard CreatePaymentCard(string cardNumber = "5528790000000008")
        => new Domain.Model.PaymentCard
        {
            CardHolderName = "John Doe",
            CardNumber = cardNumber,
            ExpirationMonth = 12,
            ExpirationYear = 2030,
            CVC = "123",
            RegisterCard = 0
        };

    [Fact]
    [Trait("Operation", "Process")]
    public async Task ProcessPayment_WithValidPayment_ShouldReturnSuccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("IYZICO_API_KEY", "dummy");
        Environment.SetEnvironmentVariable("IYZICO_SECRET_KEY", "dummy");
        Environment.SetEnvironmentVariable("IYZICO_BASE_URL", "https://sandbox-api.iyzipay.com");

        var order = CreateOrder();
        var buyer = CreateBuyer();
        var shippingAddress = CreateAddress();
        var billingAddress = CreateAddress();
        var paymentCard = CreatePaymentCard();

        var paymentResponse = new Iyzipay.Model.Payment
        {
            Status = "success",
            PaymentId = "123456",
            Price = order.BasketItems.Sum(x => x.UnitPrice * x.Quantity).ToString(),
            Currency = "TRY"
        };
        _paymentProviderMock.Setup(p => p.CreateAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<Options>()))
            .ReturnsAsync(paymentResponse);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, order.BasketItems.ToList());

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Status.Should().Be("success");
        _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    [Trait("Operation", "Process")]
    public async Task ProcessPayment_WithInvalidCard_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrder();
        var buyer = CreateBuyer();
        var shippingAddress = CreateAddress();
        var billingAddress = CreateAddress();
        var paymentCard = CreatePaymentCard("4111111111111111"); // Invalid card number

        var paymentResponse = new Iyzipay.Model.Payment
        {
            Status = "failure",
            ErrorMessage = "Invalid card number"
        };
        _paymentProviderMock.Setup(p => p.CreateAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<Options>()))
            .ReturnsAsync(paymentResponse);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, order.BasketItems.ToList());

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    [Trait("Operation", "Process")]
    public async Task ProcessPayment_WithInsufficientFunds_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOrder(1000000); // Very high amount
        var buyer = CreateBuyer();
        var shippingAddress = CreateAddress();
        var billingAddress = CreateAddress();
        var paymentCard = CreatePaymentCard();

        var paymentResponse = new Iyzipay.Model.Payment
        {
            Status = "failure",
            ErrorMessage = "Insufficient funds"
        };
        _paymentProviderMock.Setup(p => p.CreateAsync(It.IsAny<CreatePaymentRequest>(), It.IsAny<Options>()))
            .ReturnsAsync(paymentResponse);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(order, buyer, shippingAddress, billingAddress, paymentCard, order.BasketItems.ToList());

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
} 