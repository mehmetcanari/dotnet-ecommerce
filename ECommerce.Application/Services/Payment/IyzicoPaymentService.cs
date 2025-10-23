using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Globalization;

namespace ECommerce.Application.Services.Payment;

public class IyzicoPaymentService : IPaymentService
{
    private readonly Options _options;
    private readonly ILoggingService _logger;
    private readonly IPaymentProvider _paymentProvider;
    private readonly INotificationService _notificationService;

    public IyzicoPaymentService(ILoggingService logger, IPaymentProvider paymentProvider, INotificationService notificationService)
    {
        _logger = logger;
        _paymentProvider = paymentProvider;
        _options = new Options
        {
            ApiKey = Environment.GetEnvironmentVariable("IYZICO_API_KEY"),
            SecretKey = Environment.GetEnvironmentVariable("IYZICO_SECRET_KEY"),
            BaseUrl = Environment.GetEnvironmentVariable("IYZICO_BASE_URL")
        };
        _notificationService = notificationService;
    }

    public async Task<Result<Iyzipay.Model.Payment>> ProcessPaymentAsync(
        Domain.Model.Order order,
        Domain.Model.Buyer buyer,
        Domain.Model.Address shippingAddress,
        Domain.Model.Address billingAddress,
        Domain.Model.PaymentCard paymentCard,
        List<Domain.Model.BasketItem> basketItems)
    {
        try
        {
            CreatePaymentRequest request = CreatePaymentRequest(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems);
            Iyzipay.Model.Payment payment = await _paymentProvider.CreateAsync(request, _options);
            if (payment.Status != "success")
            {
                _logger.LogWarning(ErrorMessages.PaymentFailed, payment.ErrorMessage);
                return Result<Iyzipay.Model.Payment>.Failure(payment.ErrorMessage ?? ErrorMessages.PaymentFailed);
            }
            await _notificationService.CreateNotificationAsync("Payment Success", $"Payment successful for order {order.OrderId}" + $"Total price: {payment.Price}", NotificationType.Payment);
            return Result<Iyzipay.Model.Payment>.Success(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.PaymentFailed, ex.Message);
            return Result<Iyzipay.Model.Payment>.Failure(ex.Message);
        }
    }

    private CreatePaymentRequest CreatePaymentRequest(Domain.Model.Order order, Domain.Model.Buyer buyer, Domain.Model.Address shippingAddress, Domain.Model.Address billingAddress, Domain.Model.PaymentCard paymentCard, List<Domain.Model.BasketItem> basketItems) => new CreatePaymentRequest
    {
        Locale = Locale.TR.ToString(),
        ConversationId = Guid.NewGuid().ToString(),
        Price = CalculateTotalPrice(basketItems),
        PaidPrice = CalculateTotalPrice(basketItems),
        Currency = Currency.TRY.ToString(),
        Installment = 1,
        BasketId = order.OrderId.ToString(),
        PaymentChannel = nameof(PaymentChannel.WEB),
        PaymentGroup = nameof(PaymentGroup.PRODUCT),
        PaymentCard = MapToIyzicoPaymentCard(paymentCard),
        Buyer = MapToIyzicoBuyer(buyer),
        ShippingAddress = MapToIyzicoAddress(shippingAddress, order.ShippingAddress),
        BillingAddress = MapToIyzicoAddress(billingAddress, order.BillingAddress),
        BasketItems = MapToIyzicoBasketItems(basketItems)
    };

    private string CalculateTotalPrice(List<Domain.Model.BasketItem> basketItems) => basketItems.Sum(item => item.UnitPrice * item.Quantity).ToString(CultureInfo.InvariantCulture);

    private Iyzipay.Model.PaymentCard MapToIyzicoPaymentCard(Domain.Model.PaymentCard paymentCard) => new Iyzipay.Model.PaymentCard
    {
        CardHolderName = paymentCard.CardHolderName,
        CardNumber = paymentCard.CardNumber,
        ExpireMonth = paymentCard.ExpirationMonth.ToString(),
        ExpireYear = paymentCard.ExpirationYear.ToString(),
        Cvc = paymentCard.CVC,
        RegisterCard = paymentCard.RegisterCard
    };

    private Iyzipay.Model.Buyer MapToIyzicoBuyer(Domain.Model.Buyer buyer) => new Iyzipay.Model.Buyer
    {
        Id = buyer.Id,
        Name = buyer.Name,
        Surname = buyer.Surname,
        GsmNumber = buyer.GsmNumber,
        Email = buyer.Email,
        IdentityNumber = buyer.IdentityNumber,
        RegistrationAddress = buyer.RegistrationAddress,
        Ip = buyer.Ip,
        City = buyer.City,
        Country = buyer.Country,
        ZipCode = buyer.ZipCode
    };

    private Iyzipay.Model.Address MapToIyzicoAddress(Domain.Model.Address address, string description) => new Iyzipay.Model.Address
    {
        ContactName = address.ContactName,
        City = address.City,
        Country = address.Country,
        Description = description,
        ZipCode = address.ZipCode
    };

    private List<Iyzipay.Model.BasketItem> MapToIyzicoBasketItems(List<Domain.Model.BasketItem> basketItems) => basketItems.Select(item => new Iyzipay.Model.BasketItem
    {
        Id = item.ExternalId,
        Name = item.ProductName,
        Category1 = "Physical",
        ItemType = nameof(BasketItemType.PHYSICAL),
        Price = CalculateTotalPrice(basketItems),
    }).ToList();
}