using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;

namespace ECommerce.Application.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly ILoggingService _logger;
    private readonly Options _options;

    public PaymentService(ILoggingService logger)
    {
        _logger = logger;
        _options = new Options
        {
            ApiKey = "sandbox-1234567890",
            SecretKey = "sandbox-1234567890",
            BaseUrl = "https://sandbox-api.iyzipay.com"
        };
    }

    public 
}
