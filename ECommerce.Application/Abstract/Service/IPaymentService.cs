using ECommerce.Application.Utility;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract.Service;

public interface IPaymentService
{
    Task<Result<Iyzipay.Model.Payment>> ProcessPaymentAsync(Order order, Buyer buyer, Address shippingAddress, Address billingAddress, PaymentCard paymentCard, List<BasketItem> basketItems);
}

