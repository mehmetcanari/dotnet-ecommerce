using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;

namespace ECommerce.Application.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly ILoggingService _logger;

    public PaymentService(ILoggingService logger)
    {
        _logger = logger;
    }

    public Task<bool> ProcessPaymentAsync(PaymentDetails paymentDetails)
    {
        _logger.LogInformation("Payment successful");
        return Task.FromResult(true);
    }
}
