using ECommerce.Application.Abstract;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System.Globalization;
using ECommerce.Shared.Enum;
using Address = Iyzipay.Model.Address;
using BasketItem = Iyzipay.Model.BasketItem;
using Buyer = Iyzipay.Model.Buyer;
using PaymentCard = Iyzipay.Model.PaymentCard;

namespace ECommerce.Application.Services.Payment;

public class PaymentService(ILogService logger, INotificationService notificationService) : IPaymentService
{
    private readonly Options _options = new()
    {
        ApiKey = Environment.GetEnvironmentVariable("IYZICO_API_KEY"),
        SecretKey = Environment.GetEnvironmentVariable("IYZICO_SECRET_KEY"),
        BaseUrl = Environment.GetEnvironmentVariable("IYZICO_BASE_URL")
    };

    public async Task<Result<Iyzipay.Model.Payment>> ProcessPaymentAsync(Order order, Domain.Model.Buyer buyer, Domain.Model.Address shippingAddress, Domain.Model.Address billingAddress, Domain.Model.PaymentCard paymentCard, List<Domain.Model.BasketItem> basketItems)
    {
        try
        {
            CreatePaymentRequest request = CreatePaymentRequest(order, buyer, shippingAddress, billingAddress, paymentCard, basketItems);
            Iyzipay.Model.Payment payment = await Iyzipay.Model.Payment.Create(request, _options);
            if (payment.Status != "success")
            {
                logger.LogWarning(ErrorMessages.PaymentFailed, payment.ErrorMessage);
                return Result<Iyzipay.Model.Payment>.Failure(payment.ErrorMessage ?? ErrorMessages.PaymentFailed);
            }
            await notificationService.CreateNotificationAsync("Payment Success", $"Payment successful for order {order.Id}" + $"Total price: {payment.Price}", NotificationType.Payment);
            return Result<Iyzipay.Model.Payment>.Success(payment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.PaymentFailed, ex.Message);
            return Result<Iyzipay.Model.Payment>.Failure(ex.Message);
        }
    }

    private CreatePaymentRequest CreatePaymentRequest(Order order, Domain.Model.Buyer buyer, Domain.Model.Address shippingAddress, Domain.Model.Address billingAddress, Domain.Model.PaymentCard paymentCard, List<Domain.Model.BasketItem> basketItems) => new()
    {
        Locale = Locale.TR.ToString(),
        ConversationId = Guid.NewGuid().ToString(),
        Price = CalculateTotalPrice(basketItems),
        PaidPrice = CalculateTotalPrice(basketItems),
        Currency = Currency.TRY.ToString(),
        Installment = 1,
        BasketId = order.Id.ToString(),
        PaymentChannel = nameof(PaymentChannel.WEB),
        PaymentGroup = nameof(PaymentGroup.PRODUCT),
        PaymentCard = MapToCard(paymentCard),
        Buyer = MapToBuyer(buyer),
        ShippingAddress = MapToAddress(shippingAddress, order.ShippingAddress),
        BillingAddress = MapToAddress(billingAddress, order.BillingAddress),
        BasketItems = MapToBasketItems(basketItems)
    };
    private PaymentCard MapToCard(Domain.Model.PaymentCard paymentCard) => new()
    {
        CardHolderName = paymentCard.CardHolderName,
        CardNumber = paymentCard.CardNumber,
        ExpireMonth = paymentCard.ExpirationMonth.ToString(),
        ExpireYear = paymentCard.ExpirationYear.ToString(),
        Cvc = paymentCard.Cvc,
        RegisterCard = paymentCard.RegisterCard
    };

    private Buyer MapToBuyer(Domain.Model.Buyer buyer) => new()
    {
        Id = buyer.Id.ToString(),
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

    private Address MapToAddress(Domain.Model.Address address, string description) => new()
    {
        ContactName = address.ContactName,
        City = address.City,
        Country = address.Country,
        Description = description,
        ZipCode = address.ZipCode
    };

    private List<BasketItem> MapToBasketItems(List<Domain.Model.BasketItem> basketItems) => basketItems.Select(item => new BasketItem
    {
        Id = item.ExternalId,
        Name = item.ProductName,
        Category1 = "Physical",
        ItemType = nameof(BasketItemType.PHYSICAL),
        Price = CalculateTotalPrice(basketItems),
    }).ToList();

    private string CalculateTotalPrice(List<Domain.Model.BasketItem> basketItems) => basketItems.Sum(item => item.UnitPrice * item.Quantity).ToString(CultureInfo.InvariantCulture);
}