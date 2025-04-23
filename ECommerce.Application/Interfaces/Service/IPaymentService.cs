using ECommerce.Domain.Model;

namespace ECommerce.Application.Interfaces.Service;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(PaymentDetails paymentDetails);
}

