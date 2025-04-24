using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IPaymentService
{
    Task<Iyzipay.Model.Payment> ProcessPaymentAsync(Order order, Buyer buyer, Address shippingAddress, Address billingAddress, PaymentCard paymentCard, List<BasketItem> basketItems);
}

